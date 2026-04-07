using System.Reflection;
using System.Text.Json;
using HanziAnhVu.Shared.EventBus.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Infrastructure.Outbox;

namespace Users.Infrastructure.Outbox;

/// <summary>
/// BackgroundService chịu trách nhiệm lắng nghe NOTIFY từ PostgreSQL trên channel "user_outbox_channel"
/// và đẩy event từ outbox lên event bus.
/// Tương tự AuthOutboxDispatcherServiceWorker nhưng dành cho Users module với channel riêng.
/// </summary>
public sealed class UserOutboxDispatcherWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    ILogger<UserOutboxDispatcherWorker> logger) : BackgroundService
{
    // Tạo scope để resolve các service scoped khi xử lý từng event
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    // Đọc cấu hình, đặc biệt là chuỗi kết nối database
    private readonly IConfiguration _config = config;
    // Ghi log phục vụ theo dõi và xử lý sự cố
    private readonly ILogger<UserOutboxDispatcherWorker> _logger = logger;

    // Entry point của BackgroundService: xử lý sự kiện tồn trước, sau đó vào vòng lắng nghe liên tục
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Quét và xử lý các event chưa xử lý từ lần chạy trước
        await ProcessMissedEventsAsync(stoppingToken);

        // Vòng lặp chính của worker cho đến khi ứng dụng dừng
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Bắt đầu lắng nghe channel user_outbox_channel
                await ListenAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Thoát vòng lặp khi nhận tín hiệu hủy hợp lệ
                break;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi bất ngờ và thử lại sau khoảng nghỉ ngắn
                _logger.LogError(ex, "User outbox LISTEN loop failed. Retrying in 2 seconds.");
                // Tránh retry nóng gây tốn tài nguyên
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    // Thiết lập kết nối PostgreSQL và lắng nghe NOTIFY trên channel user_outbox_channel
    private async Task ListenAsync(CancellationToken ct)
    {
        // Lấy connection string từ cấu hình - nên dùng Users connection string nếu có, fallback sang default
        var connectionString = _config.GetConnectionString("UsersDbConnection")
            ?? _config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string for User module database.");

        // Mở kết nối PostgreSQL
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);

        // Đăng ký LISTEN trên channel dành riêng cho Users module
        await using (var cmd = new NpgsqlCommand("LISTEN user_outbox_channel", conn))
        {
            _logger.LogInformation("Executing LISTEN command on PostgreSQL for Users module outbox...");
            await cmd.ExecuteNonQueryAsync(ct);
        }

        // Khi nhận notification thì xử lý payload bất đồng bộ
        conn.Notification += (_, e) =>
        {
            // Fire-and-forget có xử lý lỗi bên trong HandleNotificationAsync
            _ = HandleNotificationAsync(e.Payload, ct);
        };

        // Ghi log trạng thái đã vào chế độ lắng nghe
        _logger.LogInformation("Listening for User module outbox notifications on channel 'user_outbox_channel'...");

        // WaitAsync chờ liên tục cho đến khi bị cancel
        // Không cần while loop - WaitAsync sẽ xử lý liên tục
        try
        {
            await conn.WaitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("User outbox listener cancelled.");
        }
    }

    // Parse payload từ NOTIFY và chuyển sang xử lý event tương ứng
    private async Task HandleNotificationAsync(string payload, CancellationToken ct)
    {
        try
        {
            // Deserialize payload từ trigger row_to_json(NEW)
            var outboxMessage = JsonSerializer.Deserialize<UserModuleOutboxMessage>(payload);
            if (outboxMessage is not null &&
                outboxMessage.Id != Guid.Empty &&
                !string.IsNullOrWhiteSpace(outboxMessage.Type))
            {
                // Xử lý trực tiếp từ payload notification
                await ProcessEventAsync(outboxMessage, ct);
                return;
            }

            // Fallback: payload cũ chỉ chứa Id
            var outbox = JsonSerializer.Deserialize<OutboxNotificationPayload>(payload);
            if (outbox is null)
            {
                _logger.LogWarning("Received invalid user outbox notification payload: {Payload}", payload);
                return;
            }

            // Xử lý event theo Id vừa nhận
            await ProcessEventAsync(outbox.Id, ct);
        }
        catch (Exception ex)
        {
            // Bắt lỗi để tránh crash luồng notification
            _logger.LogError(ex, "Failed to process User module outbox notification payload.");
        }
    }

    // Xử lý event trực tiếp từ payload NOTIFY đầy đủ
    private async Task ProcessEventAsync(UserModuleOutboxMessage message, CancellationToken ct)
    {
        // Tạo scope mới để resolve DbContext và EventBus
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UserModuleDbContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        try
        {
            // Publish event ra event bus từ message trong payload
            _logger.LogInformation("Publishing user outbox event {Id} of type {Type} from notification payload...", message.Id, message.Type);
            await PublishAsync(eventBus, message, ct);

            // Cập nhật trạng thái đã xử lý mà không cần load entity trước
            await db.OutboxMessages
                .Where(x => x.Id == message.Id && x.ProcessedOnUtc == null)
                .ExecuteUpdateAsync(
                    setter => setter
                        .SetProperty(x => x.ProcessedOnUtc, _ => DateTime.UtcNow)
                        .SetProperty(x => x.Error, _ => null), // Xóa lỗi cũ nếu có
                    ct);
        }
        catch (Exception ex)
        {
            // Khi publish thất bại, tăng retry và lưu lỗi
            await db.OutboxMessages
                .Where(x => x.Id == message.Id && x.ProcessedOnUtc == null)
                .ExecuteUpdateAsync(
                    setter => setter
                        .SetProperty(x => x.RetryCount, x => x.RetryCount + 1)
                        .SetProperty(x => x.Error, _ => ex.Message),
                    ct);

            _logger.LogError(ex, "Failed to process user outbox event {Id}", message.Id);
        }
    }

    // Xử lý một event cụ thể theo outboxId
    private async Task ProcessEventAsync(Guid outboxId, CancellationToken ct)
    {
        // Tạo scope mới để resolve DbContext và EventBus
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UserModuleDbContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        // Lấy message chưa xử lý tương ứng với outboxId
        var message = await db.OutboxMessages
            .FirstOrDefaultAsync(x => x.Id == outboxId && x.ProcessedOnUtc == null, ct);

        // Không có message hợp lệ thì thoát sớm
        if (message is null) return;

        try
        {
            // Publish event ra event bus
            _logger.LogInformation("Publishing user outbox event {Id} of type {Type}...", message.Id, message.Type);
            await PublishAsync(eventBus, message, ct);

            // Đánh dấu đã xử lý thành công
            message.ProcessedOnUtc = DateTime.UtcNow;
            // Xóa thông tin lỗi cũ nếu có
            message.Error = null;
        }
        catch (Exception ex)
        {
            // Tăng số lần retry khi publish thất bại
            message.RetryCount++;
            // Lưu thông tin lỗi để truy vết
            message.Error = ex.Message;
            // Ghi log lỗi chi tiết
            _logger.LogError(ex, "Failed to process user outbox event {Id}", message.Id);
        }

        // Persist trạng thái xử lý message
        await db.SaveChangesAsync(ct);
    }

    // Xử lý các outbox event bị bỏ lỡ khi service khởi động lại
    private async Task ProcessMissedEventsAsync(CancellationToken ct)
    {
        // Tạo scope để truy cập DbContext
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UserModuleDbContext>();

        // Lấy danh sách event chưa xử lý và còn trong giới hạn retry
        var missed = await db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null && x.RetryCount < 3)
            .OrderBy(x => x.OccurredOnUtc)
            .ToListAsync(ct);

        // Log số lượng event tồn được xử lý ở startup
        _logger.LogInformation(
            "Processing {Count} missed user outbox events on startup", missed.Count);

        // Xử lý tuần tự từng event tồn
        foreach (var msg in missed)
        {
            await ProcessEventAsync(msg, ct);
        }
    }

    // Publish một outbox message thông qua generic method của IEventBus
    private static async Task PublishAsync(IEventBus eventBus, UserModuleOutboxMessage message, CancellationToken cancellationToken)
    {
        // Resolve Type từ tên type đã lưu trong outbox
        var eventType = Type.GetType(message.Type, throwOnError: false)
            ?? throw new InvalidOperationException($"Unable to resolve user outbox event type '{message.Type}'.");

        // Deserialize payload ra instance event đúng type
        var integrationEvent = OutboxMessageSerialization.DeserializePayload(message, eventType)
            ?? throw new InvalidOperationException($"Unable to deserialize user outbox payload for '{message.Type}'.");

        // Lấy method PublishAsync<TEvent> từ interface IEventBus
        var publishMethod = typeof(IEventBus)
            .GetMethod(nameof(IEventBus.PublishAsync), BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Unable to locate IEventBus.PublishAsync method.");

        // Đóng generic method với eventType runtime
        var genericMethod = publishMethod.MakeGenericMethod(eventType);

        // Gọi method publish bằng reflection
        var publishTask = genericMethod.Invoke(eventBus, [integrationEvent, cancellationToken]) as Task
            ?? throw new InvalidOperationException($"Unable to publish user outbox event '{message.Type}'.");

        // Chờ hoàn tất publish
        await publishTask;
    }

    // Payload tối thiểu nhận từ PostgreSQL NOTIFY (fallback)
    private sealed record OutboxNotificationPayload(Guid Id);
}
