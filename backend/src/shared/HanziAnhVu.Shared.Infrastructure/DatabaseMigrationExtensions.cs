using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure;

public static class DatabaseMigrationExtensions
{
    public static Task<int> ExecuteSqlAsync(
        this DatabaseFacade database,
        string sql,
        CancellationToken cancellationToken = default)
    {
        return database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public static Task<int> ExecuteSqlAsync(
        this DatabaseFacade database,
        FormattableString sql, // FormattableString hỗ trợ interpolated SQL với tham số an toàn
                            // Ví dụ: database.ExecuteSqlAsync($"INSERT INTO MyTable (Column) VALUES ({value})");
        CancellationToken cancellationToken = default)
    {
        // Sử dụng ExecuteSqlInterpolatedAsync để hỗ trợ interpolated SQL với tham số an toàn
        return database.ExecuteSqlInterpolatedAsync(sql, cancellationToken);
    }

    public static async Task<bool> MigrateWithRetryAsync(
        this DatabaseFacade database,
        ILogger logger,
        CancellationToken cancellationToken,
        int maxAttempts = 10,
        TimeSpan? retryDelay = null)
    {
        var delay = retryDelay ?? TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Thực hiện migration
                // là những lỗi tạm thời như database chưa sẵn sàng, network issues, deadlocks, v.v.
                // migration sẽ dùng các migration script đã được tạo ra trước đó, nên nếu có lỗi sẽ
                //  là lỗi tạm thời chứ không phải lỗi logic trong code.
                await database.MigrateAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                if (attempt >= maxAttempts)
                {
                    logger.LogError(ex,
                        "Database migration failed after {MaxAttempts} attempts.",
                        maxAttempts);
                    return false;
                }

                logger.LogWarning(ex,
                    "Database migration attempt {Attempt}/{MaxAttempts} failed. Retrying in {DelaySeconds}s.",
                    attempt,
                    maxAttempts,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
        }

        return false;
    }

    // thêm trigger để notify khi có thay đổi trên bảng outbox, giúp service có thể lắng nghe và xử lý ngay lập tức mà không cần polling
    public static Task<int> EnsureOutboxNotifyTriggerAsync(
        this DatabaseFacade database,
        string tableName = "OutboxMessages",
        string channelName = "outbox_channel",
        string functionName = "notify_outbox_change",
        string triggerName = "outbox_change_trigger",
        CancellationToken cancellationToken = default)
    {
        var safeTableName = NormalizeIdentifier(tableName);
        var safeFunctionName = NormalizeIdentifier(functionName);
        var safeTriggerName = NormalizeIdentifier(triggerName);
        var safeChannelName = channelName.Replace("'", "''");

        var sql = $"""
            CREATE OR REPLACE FUNCTION {safeFunctionName}()
            RETURNS trigger AS $$
            BEGIN
              PERFORM pg_notify('{safeChannelName}', row_to_json(NEW)::text);
              RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS {safeTriggerName} ON "{safeTableName}";

            CREATE TRIGGER {safeTriggerName}
            AFTER INSERT ON "{safeTableName}"
            FOR EACH ROW EXECUTE FUNCTION {safeFunctionName}();
            """;
        // Lưu ý: Cần đảm bảo rằng tên bảng, function, trigger và channel được 
        // chuẩn hóa để tránh lỗi SQL injection hoặc lỗi cú pháp. Hàm NormalizeIdentifier 
        // sẽ kiểm tra và làm sạch các identifier này.
        // Execute SQL để tạo function và trigger. Nếu đã tồn tại, sẽ được thay thế 
        // (CREATE OR REPLACE FUNCTION) hoặc xóa rồi tạo lại (DROP TRIGGER IF EXISTS).
        return database.ExecuteSqlAsync(sql, cancellationToken);
    }

    private static string NormalizeIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("Identifier cannot be null or whitespace.", nameof(identifier));
        }

        var normalized = identifier.Trim();
        if (normalized.Any(ch => !char.IsLetterOrDigit(ch) && ch != '_'))
        {
            throw new ArgumentException($"Identifier '{identifier}' contains invalid characters.", nameof(identifier));
        }

        return normalized;
    }
}
