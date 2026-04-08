namespace Classroom.Infrastructure.Outbox;

public sealed class ClassroomModuleDispatcherWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    ILogger<ClassroomModuleDispatcherWorker> logger
) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IConfiguration _config = config;
    private readonly ILogger<ClassroomModuleDispatcherWorker> _logger = logger;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
         await ProcessMissedEventAsync(stoppingToken); // Xử lý các event bị bỏ lỡ trước khi bắt đầu lắng nghe

        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ListenAsync(stoppingToken);
                
            } 
            catch (OperationCanceledException ) when (stoppingToken.IsCancellationRequested)
            {
                break; // Graceful shutdown
            }

            catch (Exception ex)
            {
                // Ghi log lỗi bất ngờ và thử lại sau khoảng nghỉ ngắn
                _logger.LogError(ex, "Classroom outbox LISTEN loop failed. Retrying in 2 seconds.");
                // Tránh retry nóng gây tốn tài nguyên
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
    
// Lắng nghe NOTIFY từ PostgreSQL và xử lý message mới
    private async Task ListenAsync(CancellationToken stoppingToken)
    {
        var connectionString = _config.GetConnectionString("ClassroomDbConnection") ?? throw new InvalidOperationException("Connection string 'ClassroomConnection' not found.");
        await using var connection = new Npgsql.NpgsqlConnection(connectionString);
        await connection.OpenAsync(stoppingToken);
        // Implementation for listening to PostgreSQL notifications
        
        await using (var cmd = new Npgsql.NpgsqlCommand("LISTEN classroom_outbox_channel;", connection))
        {
            _logger.LogInformation("Executing LISTEN command on PostgreSQL for Classroom module outbox...");
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }
        connection.Notification += (_,  e) =>
        {
            // fire-and-forget xử lý notification để không block thread lắng nghe
            _ = HandleNotificationAsync(e.Payload, stoppingToken);
        };
        _logger.LogInformation("Started listening to PostgreSQL notifications for Classroom module outbox.");
        
        // WaitAsync chờ liên tục cho đến khi bị cancel
        // Không cần while loop - WaitAsync sẽ xử lý liên tục
        try
        {
            await connection.WaitAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Classroom outbox listener cancelled.");
        }
    }
    private async Task HandleNotificationAsync(string payload, CancellationToken stoppingToken)
    {
        try
        {
            // Deserialize payload từ trigger row_to_json(NEW)
            var outboxMessage = JsonSerializer.Deserialize<ClassroomModuleOutboxMessage>(payload);
            if (outboxMessage is not null &&
                outboxMessage.Id != Guid.Empty &&
                !string.IsNullOrWhiteSpace(outboxMessage.Type))
            {
                // Xử lý trực tiếp từ payload notification
                await ProcessEventAsync(outboxMessage, stoppingToken);
                return;
            }

            // Fallback: payload cũ chỉ chứa Id
            var outbox = JsonSerializer.Deserialize<ClassroomNotificationPayload>(payload);
            if (outbox is null)
            {
                _logger.LogWarning("Received invalid classroom outbox notification payload: {Payload}", payload);
                return;
            }

            // Xử lý event theo Id vừa nhận
            await ProcessEventAsync(outbox.Id, stoppingToken);
        }
        catch (Exception ex)
        {
            // Bắt lỗi để tránh crash luồng notification
            _logger.LogError(ex, "Failed to process Classroom module outbox notification payload.");
        }
    }
    private async Task ProcessEventAsync(ClassroomModuleOutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        // Tạo scope mới để resolve DbContext và EventBus
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClassroomDbContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        try
        {
            // Publish event ra event bus từ message trong payload
            _logger.LogInformation("Publishing classroom outbox event {Id} of type {Type} from notification payload...", outboxMessage.Id, outboxMessage.Type);
            await PublishAsync(eventBus, outboxMessage, cancellationToken);

            // Cập nhật trạng thái đã xử lý mà không cần load entity trước
            await db.ClassroomOutboxMessages
                .Where(x => x.Id == outboxMessage.Id && x.ProcessedOnUtc == null)
                .ExecuteUpdateAsync(
                    setter => setter
                        .SetProperty(x => x.ProcessedOnUtc, _ => DateTime.UtcNow)
                        .SetProperty(x => x.Error, _ => null), // Xóa lỗi cũ nếu có
                    cancellationToken);
        }
        catch (Exception ex)
        {
            // Khi publish thất bại, tăng retry và lưu lỗi
            await db.ClassroomOutboxMessages
                .Where(x => x.Id == outboxMessage.Id && x.ProcessedOnUtc == null)
                .ExecuteUpdateAsync(
                    setter => setter
                        .SetProperty(x => x.RetryCount, x => x.RetryCount + 1)
                        .SetProperty(x => x.Error, _ => ex.Message),
                    cancellationToken);

            _logger.LogError(ex, "Failed to process classroom outbox event {Id}", outboxMessage.Id);
        }
    }
    private async Task ProcessEventAsync(Guid outboxId, CancellationToken cancellationToken)
    {
         // Tạo scope mới để resolve DbContext và EventBus
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClassroomDbContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        // Lấy message chưa xử lý tương ứng với outboxId
        var message = await db.ClassroomOutboxMessages
            .FirstOrDefaultAsync(x => x.Id == outboxId && x.ProcessedOnUtc == null, cancellationToken);

        // Không có message hợp lệ thì thoát sớm
        if (message is null) return;

        try
        {
            // Publish event ra event bus
            _logger.LogInformation("Publishing classroom outbox event {Id} of type {Type}...", message.Id, message.Type);
            await PublishAsync(eventBus, message, cancellationToken);

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
            _logger.LogError(ex, "Failed to process classroom outbox event {Id}", message.Id);
        }

        // Persist trạng thái xử lý message
        await db.SaveChangesAsync(cancellationToken);
    }
    // xử lý trường hợp có sự kiện bị bỏ lỡ (ví dụ do worker downtime) bằng cách kiểm tra outbox table định kỳ
    private async Task ProcessMissedEventAsync(CancellationToken cancellationToken)
    {
        // create scope để resolve DbContext và EventBus
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ClassroomDbContext>();
        // Lấy các message chưa được dispatch (dựa trên timestamp hoặc status)
        var missedMessages = await dbContext.ClassroomOutboxMessages
            .Where(x => x.ProcessedOnUtc == null && x.RetryCount < 3)
            .OrderBy(x => x.OccurredOnUtc)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Found {Count} missed outbox messages to process.", missedMessages.Count);
        foreach (var message in missedMessages)
        {
            await ProcessEventAsync(message, cancellationToken);
        }
    }

    
    // Publish một outbox message thông qua generic method của IEventBus
    private static async Task PublishAsync(IEventBus eventBus, ClassroomModuleOutboxMessage message, CancellationToken cancellationToken)
    {
        // Resolve Type từ tên type đã lưu trong outbox
        var eventType = Type.GetType(message.Type, throwOnError: false)
            ?? throw new InvalidOperationException($"Unable to resolve classroom outbox event type '{message.Type}'.");

        // Deserialize payload ra instance event đúng type
        var integrationEvent = OutboxMessageSerialization.DeserializePayload(message, eventType)
            ?? throw new InvalidOperationException($"Unable to deserialize classroom outbox payload for '{message.Type}'.");

        // Lấy method PublishAsync<TEvent> từ interface IEventBus
        var publishMethod = typeof(IEventBus)
            .GetMethod(nameof(IEventBus.PublishAsync), BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Unable to locate IEventBus.PublishAsync method.");

        // Đóng generic method với eventType runtime
        var genericMethod = publishMethod.MakeGenericMethod(eventType);

        // Gọi method publish bằng reflection
        var publishTask = genericMethod.Invoke(eventBus, [integrationEvent, cancellationToken]) as Task
            ?? throw new InvalidOperationException($"Unable to publish classroom outbox event '{message.Type}'.");

        // Chờ hoàn tất publish
        await publishTask;
    }

    private sealed record ClassroomNotificationPayload(Guid Id);
}