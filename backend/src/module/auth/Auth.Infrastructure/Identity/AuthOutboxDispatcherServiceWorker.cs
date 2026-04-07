using System.Reflection;
using System.Text.Json;
using HanziAnhVu.Shared.EventBus.Abstracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Infrastructure.Outbox;

namespace Auth.Infrastructure.Identity;

// BackgroundService chịu trách nhiệm lắng nghe NOTIFY từ PostgreSQL và đẩy event từ outbox lên event bus.
public sealed class AuthOutboxDispatcherServiceWorker : BackgroundService
{
    // Tạo scope để resolve các service scoped khi xử lý từng event.
    private readonly IServiceScopeFactory _scopeFactory;
    // Đọc cấu hình, đặc biệt là chuỗi kết nối database.
    private readonly IConfiguration _config;
    // Ghi log phục vụ theo dõi và xử lý sự cố.
    private readonly ILogger<AuthOutboxDispatcherServiceWorker> _logger;

    // Constructor inject các dependency cần thiết cho service.
    public AuthOutboxDispatcherServiceWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<AuthOutboxDispatcherServiceWorker> logger)
    {
        // Lưu scope factory để tạo scope mới mỗi lần xử lý.
        _scopeFactory = scopeFactory;
        // Lưu cấu hình để đọc connection string. 
        _config = config;
        // Lưu logger để ghi log runtime.
        _logger = logger;
    }

    // Entry point của BackgroundService: xử lý sự kiện tồn trước, sau đó vào vòng lắng nghe liên tục.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Quét và xử lý các event chưa xử lý từ lần chạy trước.
        await ProcessMissedEventsAsync(stoppingToken);

        // Vòng lặp chính của worker cho đến khi ứng dụng dừng.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Bắt đầu lắng nghe channel outbox.
                await ListenAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Thoát vòng lặp khi nhận tín hiệu hủy hợp lệ.
                break;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi bất ngờ và thử lại sau khoảng nghỉ ngắn.
                _logger.LogError(ex, "Outbox LISTEN loop failed. Retrying in 2 seconds.");
                // Tránh retry nóng gây tốn tài nguyên.
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    // Thiết lập kết nối PostgreSQL và lắng nghe NOTIFY trên channel outbox.
    private async Task ListenAsync(CancellationToken ct)
    {
        // Lấy connection string từ cấu hình.
        var connectionString = _config.GetConnectionString("AuthIdentityDbConnection")
            // Ném lỗi rõ ràng nếu thiếu cấu hình bắt buộc.
            ?? throw new InvalidOperationException("Missing connection string for AuthIdentityDbConnection.");

        // Mở kết nối PostgreSQL.
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);

        // Đăng ký LISTEN trên channel đã cấu hình ở trigger/database.
        await using (var cmd = new NpgsqlCommand("LISTEN outbox_channel", conn))
        {
            _logger.LogInformation("Executing LISTEN command on PostgreSQL for module {module}...", nameof(AuthOutboxDispatcherServiceWorker));
            await cmd.ExecuteNonQueryAsync(ct);
        }

        // Khi nhận notification thì xử lý payload bất đồng bộ.
        conn.Notification += (_, e) =>
        {
            // Fire-and-forget có xử lý lỗi bên trong HandleNotificationAsync.
            _ = HandleNotificationAsync(e.Payload, ct);
        };

        // Ghi log trạng thái đã vào chế độ lắng nghe.
        _logger.LogInformation("Listening for outbox notifications on channel 'outbox_channel'...");

        try
        {
            await conn.WaitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Auth outbox listener cancelled.");
        }
    }

    // Parse payload từ NOTIFY và chuyển sang xử lý event tương ứng.
    private async Task HandleNotificationAsync(string payload, CancellationToken ct)
    {
        try
        {
            // Ưu tiên deserialize payload đầy đủ của OutboxMessage (theo trigger row_to_json(NEW)).
            var outboxMessage = JsonSerializer.Deserialize<AuthOutboxMessage>(payload);
            if (outboxMessage is not null &&
                outboxMessage.Id != Guid.Empty &&
                !string.IsNullOrWhiteSpace(outboxMessage.Type))
            {
                // Xử lý trực tiếp từ payload notification, không cần query DB để lấy message.
                await ProcessEventAsync(outboxMessage, ct);
                return;
            }

            // Fallback tương thích: payload cũ chỉ chứa Id.
            var outbox = JsonSerializer.Deserialize<OutboxNotificationPayload>(payload);
            if (outbox is null)
            {
                // Log cảnh báo khi payload không hợp lệ.
                _logger.LogWarning("Received invalid outbox notification payload: {Payload}", payload);
                return;
            }

            // Xử lý event theo Id vừa nhận.
            await ProcessEventAsync(outbox.Id, ct);
        }
        catch (Exception ex)
        {
            // Bắt lỗi để tránh crash luồng notification.
            _logger.LogError(ex, "Failed to process outbox notification payload.");
        }
    }

    // Xử lý event trực tiếp từ payload NOTIFY đầy đủ (không cần query message theo Id).
    private async Task ProcessEventAsync(AuthOutboxMessage message, CancellationToken ct)
    {
        // Tạo scope mới để resolve DbContext và EventBus theo vòng đời scoped.
        await using var scope = _scopeFactory.CreateAsyncScope();
        // Resolve DbContext outbox.
        var db = scope.ServiceProvider.GetRequiredService<AuthIdentityDbContext>();
        // Resolve event bus để publish integration event.
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        try
        {
            // Publish event ra event bus trực tiếp từ message trong payload notify.
            _logger.LogInformation("Publishing outbox event {Id} of type {Type} from notification payload...", message.Id, message.Type);
            await PublishAsync(eventBus, message, ct);

            // Cập nhật trạng thái đã xử lý vào DB mà không cần load entity trước.
            await db.OutboxMessages
                .Where(x => x.Id == message.Id && x.ProcessedOnUtc == null)
                .ExecuteUpdateAsync(
                    setter => setter
                        .SetProperty(x => x.ProcessedOnUtc, _ => DateTime.UtcNow)
                        .SetProperty(x => x.Error, _ => null),
                    ct);
        }
        catch (Exception ex)
        {
            // Khi publish thất bại, tăng retry và lưu lỗi để retry/recovery.
            await db.OutboxMessages
                .Where(x => x.Id == message.Id && x.ProcessedOnUtc == null)
                .ExecuteUpdateAsync(
                    setter => setter
                        .SetProperty(x => x.RetryCount, x => x.RetryCount + 1)
                        .SetProperty(x => x.Error, _ => ex.Message),
                    ct);

            // Ghi log lỗi chi tiết.
            _logger.LogError(ex, "Failed to process outbox event {Id}", message.Id);
        }
    }

    // Xử lý một event cụ thể theo outboxId.
    private async Task ProcessEventAsync(Guid outboxId, CancellationToken ct)
    {
        // Tạo scope mới để resolve DbContext và EventBus theo vòng đời scoped.
        await using var scope = _scopeFactory.CreateAsyncScope();
        // Resolve DbContext outbox.
        var db = scope.ServiceProvider.GetRequiredService<AuthIdentityDbContext>();
        // Resolve event bus để publish integration event.
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        // Lấy message chưa xử lý tương ứng với outboxId.
        var message = await db.OutboxMessages
            .FirstOrDefaultAsync(x => x.Id == outboxId && x.ProcessedOnUtc == null, ct);

        // Không có message hợp lệ thì thoát sớm.
        if (message is null) return;

        try
        {
            // Publish event ra event bus.
            _logger.LogInformation("Publishing outbox event {Id} of type {Type}...", message.Id, message.Type);
            await PublishAsync(eventBus, message, ct);

            // Đánh dấu đã xử lý thành công.
            message.ProcessedOnUtc = DateTime.UtcNow;
            // Xóa thông tin lỗi cũ nếu có.
            message.Error = null;
        }
        catch (Exception ex)
        {
            // Tăng số lần retry khi publish thất bại.
            message.RetryCount++;
            // Lưu thông tin lỗi để truy vết.
            message.Error = ex.Message;
            // Ghi log lỗi chi tiết.
            _logger.LogError(ex, "Failed to process outbox event {Id}", outboxId);
        }

        // Persist trạng thái xử lý message.
        await db.SaveChangesAsync(ct);
    }

    // Xử lý các outbox event bị bỏ lỡ khi service khởi động lại.
    private async Task ProcessMissedEventsAsync(CancellationToken ct)
    {
        // Tạo scope để truy cập DbContext.
        await using var scope = _scopeFactory.CreateAsyncScope();
        // Resolve DbContext outbox.
        var db = scope.ServiceProvider.GetRequiredService<AuthIdentityDbContext>();

        // Lấy danh sách event chưa xử lý và còn trong giới hạn retry.
        var missed = await db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null && x.RetryCount < 3)
            .OrderBy(x => x.OccurredOnUtc)
            .ToListAsync(ct);

        // Log số lượng event tồn được xử lý ở startup.
        _logger.LogInformation(
            "Processing {Count} missed outbox events on startup", missed.Count);

        // Xử lý tuần tự từng event tồn.
        foreach (var msg in missed)
        {
            await ProcessEventAsync(msg.Id, ct);
        }
    }

    // Publish một outbox message thông qua generic method của IEventBus.
    private static async Task PublishAsync(IEventBus eventBus, AuthOutboxMessage message, CancellationToken cancellationToken)
    {
        // Resolve Type từ tên type đã lưu trong outbox.
        var eventType = Type.GetType(message.Type, throwOnError: false)
            ?? throw new InvalidOperationException($"Unable to resolve outbox event type '{message.Type}'.");

        // Deserialize payload ra instance event đúng type.
        var integrationEvent = OutboxMessageSerialization.DeserializePayload(message, eventType)
            ?? throw new InvalidOperationException($"Unable to deserialize outbox payload for '{message.Type}'.");

        // Lấy method PublishAsync<TEvent> từ interface IEventBus.
        var publishMethod = typeof(IEventBus)
            .GetMethod(nameof(IEventBus.PublishAsync), BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Unable to locate IEventBus.PublishAsync method.");

        // Đóng generic method với eventType runtime.
        var genericMethod = publishMethod.MakeGenericMethod(eventType);

        // Gọi method publish bằng reflection.
        var publishTask = genericMethod.Invoke(eventBus, [integrationEvent, cancellationToken]) as Task
            ?? throw new InvalidOperationException($"Unable to publish outbox event '{message.Type}'.");

        // Chờ hoàn tất publish.
        await publishTask;
    }

    // Payload tối thiểu nhận từ PostgreSQL NOTIFY.
    private sealed record OutboxNotificationPayload(Guid Id);
}
