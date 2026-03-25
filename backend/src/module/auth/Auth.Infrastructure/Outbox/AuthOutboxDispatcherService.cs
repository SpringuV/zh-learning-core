using System.Reflection;
using System.Text.Json;
using HanziAnhVu.Shared.EventBus.Abstracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Auth.Infrastructure.Outbox;

public sealed class AuthOutboxDispatcherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthOutboxDispatcherService> _logger;

    public AuthOutboxDispatcherService(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<AuthOutboxDispatcherService> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ProcessMissedEventsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ListenAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox LISTEN loop failed. Retrying in 2 seconds.");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    private async Task ListenAsync(CancellationToken ct)
    {
        var connectionString = _config.GetConnectionString("AuthIdentityDbConnection")
            ?? throw new InvalidOperationException("Missing connection string for AuthIdentityDbConnection.");

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);

        await using (var cmd = new NpgsqlCommand("LISTEN outbox_channel", conn))
        {
            await cmd.ExecuteNonQueryAsync(ct);
        }

        conn.Notification += (_, e) =>
        {
            _ = HandleNotificationAsync(e.Payload, ct);
        };

        _logger.LogInformation("Listening for outbox notifications on channel 'outbox_channel'...");

        while (!ct.IsCancellationRequested)
        {
            await conn.WaitAsync(ct);
        }
    }

    private async Task HandleNotificationAsync(string payload, CancellationToken ct)
    {
        try
        {
            var outbox = JsonSerializer.Deserialize<OutboxNotificationPayload>(payload);
            if (outbox is null)
            {
                _logger.LogWarning("Received invalid outbox notification payload: {Payload}", payload);
                return;
            }

            await ProcessEventAsync(outbox.Id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process outbox notification payload.");
        }
    }

    // process một event cụ thể dựa trên outboxId, đảm bảo rằng chỉ có một instance của service xử lý event đó
    // tại một thời điểm thông qua việc kiểm tra ProcessedOnUtc và RetryCount
    private async Task ProcessEventAsync(Guid outboxId, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OutboxMessageDbContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        var message = await db.OutboxMessages
            .FirstOrDefaultAsync(x => x.Id == outboxId && x.ProcessedOnUtc == null, ct);

        if (message is null) return;

        try
        {
            await PublishAsync(eventBus, message, ct);

            message.ProcessedOnUtc = DateTime.UtcNow;
            message.Error = null;
        }
        catch (Exception ex)
        {
            message.RetryCount++;
            message.Error = ex.Message;
            _logger.LogError(ex, "Failed to process outbox event {Id}", outboxId);
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task ProcessMissedEventsAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OutboxMessageDbContext>();

        var missed = await db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null && x.RetryCount < 3)
            .OrderBy(x => x.OccurredOnUtc)
            .ToListAsync(ct);

        _logger.LogInformation(
            "Processing {Count} missed outbox events on startup", missed.Count);

        foreach (var msg in missed)
        {
            await ProcessEventAsync(msg.Id, ct);
        }
    }

    private static async Task PublishAsync(IEventBus eventBus, OutboxMessage message, CancellationToken cancellationToken)
    {
        var eventType = Type.GetType(message.Type, throwOnError: false)
            ?? throw new InvalidOperationException($"Unable to resolve outbox event type '{message.Type}'.");

        var integrationEvent = OutboxMessageMapper.DeserializePayload(message, eventType)
            ?? throw new InvalidOperationException($"Unable to deserialize outbox payload for '{message.Type}'.");

        var publishMethod = typeof(IEventBus)
            .GetMethod(nameof(IEventBus.PublishAsync), BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Unable to locate IEventBus.PublishAsync method.");

        var genericMethod = publishMethod.MakeGenericMethod(eventType);
        var publishTask = genericMethod.Invoke(eventBus, [integrationEvent, cancellationToken]) as Task
            ?? throw new InvalidOperationException($"Unable to publish outbox event '{message.Type}'.");

        await publishTask;
    }

    private sealed record OutboxNotificationPayload(Guid Id);
}
