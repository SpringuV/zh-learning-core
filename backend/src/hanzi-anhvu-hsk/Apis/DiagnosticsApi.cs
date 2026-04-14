using System.Data.Common;
using Auth.Infrastructure.Identity;
using Classroom.Infrastructure;
using Lesson.Infrastructure;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Users.Infrastructure;

namespace HanziAnhVuHsk.Apis;

public static class DiagnosticsApi
{
    private const int TopTablesLimit = 20;
    private const int MaxKeysPerPrefix = 500;

    public static async Task<IResult> GetStorageSummary(
        AuthIdentityDbContext authDb,
        UserModuleDbContext usersDb,
        LessonDbContext lessonDb,
        ClassroomDbContext classroomDb,
        IConfiguration configuration,
        CancellationToken ct)
    {
        try
        {
            var databases = new List<DatabaseStorageSummary>
            {
                await GetDatabaseSummaryAsync("auth", authDb, ct),
                await GetDatabaseSummaryAsync("users", usersDb, ct),
                await GetDatabaseSummaryAsync("lesson", lessonDb, ct),
                await GetDatabaseSummaryAsync("classroom", classroomDb, ct),
            };

            var redisSummary = await GetRedisSummaryAsync(configuration, ct);

            var response = new StorageDiagnosticsResponse(
                GeneratedAtUtc: DateTimeOffset.UtcNow,
                DatabaseCount: databases.Count,
                TotalDatabaseBytes: databases.Sum(d => d.DatabaseBytes),
                Databases: databases,
                Redis: redisSummary);

            return Results.Ok(response);
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<DatabaseStorageSummary> GetDatabaseSummaryAsync(
        string module,
        DbContext dbContext,
        CancellationToken ct)
    {
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(ct);
        }

        var databaseName = await ExecuteScalarStringAsync(
            connection,
            "SELECT current_database();",
            ct) ?? module;

        var databaseBytes = await ExecuteScalarInt64Async(
            connection,
            "SELECT pg_database_size(current_database());",
            ct);

        const string tableSql = """
            SELECT
                schemaname,
                relname,
                pg_total_relation_size(quote_ident(schemaname) || '.' || quote_ident(relname)) AS total_bytes,
                pg_relation_size(quote_ident(schemaname) || '.' || quote_ident(relname)) AS table_bytes,
                pg_indexes_size(quote_ident(schemaname) || '.' || quote_ident(relname)) AS index_bytes,
                n_live_tup
            FROM pg_stat_user_tables
            ORDER BY total_bytes DESC
            LIMIT @limit;
            """;

        var tables = new List<TableStorageSummary>();
        await using var command = connection.CreateCommand();
        command.CommandText = tableSql;
        AddParameter(command, "limit", TopTablesLimit);

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            tables.Add(new TableStorageSummary(
                Schema: reader.GetString(0),
                Table: reader.GetString(1),
                TotalBytes: reader.GetInt64(2),
                TableBytes: reader.GetInt64(3),
                IndexBytes: reader.GetInt64(4),
                EstimatedRows: reader.IsDBNull(5) ? 0 : Convert.ToInt64(reader.GetValue(5))));
        }

        return new DatabaseStorageSummary(
            Module: module,
            Database: databaseName,
            DatabaseBytes: databaseBytes,
            TopTables: tables);
    }

    private static async Task<RedisStorageSummary> GetRedisSummaryAsync(IConfiguration configuration, CancellationToken ct)
    {
        var redisConnectionString =
            configuration.GetConnectionString("redis-hanzi")
            ?? configuration["Redis:ConnectionString"];

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            return new RedisStorageSummary(
                IsAvailable: false,
                DatabaseIndex: 0,
                TotalKeysInDatabase: 0,
                UsedMemoryBytes: 0,
                PeakUsedMemoryBytes: 0,
                ScopeBreakdown: [],
                Notes: "Missing Redis connection string.");
        }

        await using var mux = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
        var endpoints = mux.GetEndPoints();
        if (endpoints.Length == 0)
        {
            return new RedisStorageSummary(
                IsAvailable: false,
                DatabaseIndex: 0,
                TotalKeysInDatabase: 0,
                UsedMemoryBytes: 0,
                PeakUsedMemoryBytes: 0,
                ScopeBreakdown: [],
                Notes: "Redis endpoint not found.");
        }

        var server = mux.GetServer(endpoints[0]);
        var db = mux.GetDatabase();

        var usedMemoryBytes = ReadMemoryInfo(server, "used_memory");
        var peakUsedMemoryBytes = ReadMemoryInfo(server, "used_memory_peak");
        var totalKeys = server.DatabaseSize(db.Database);

        var scopeBreakdown = new List<RedisScopeSummary>
        {
            await GetRedisScopeSummaryAsync(server, db, "search:course:admin", ct),
            await GetRedisScopeSummaryAsync(server, db, "search:topic:admin", ct),
            await GetRedisScopeSummaryAsync(server, db, "cache:scope-version", ct),
        };

        return new RedisStorageSummary(
            IsAvailable: true,
            DatabaseIndex: db.Database,
            TotalKeysInDatabase: totalKeys,
            UsedMemoryBytes: usedMemoryBytes,
            PeakUsedMemoryBytes: peakUsedMemoryBytes,
            ScopeBreakdown: scopeBreakdown,
            Notes: $"Each prefix samples up to {MaxKeysPerPrefix} keys.");
    }

    private static async Task<RedisScopeSummary> GetRedisScopeSummaryAsync(
        IServer server,
        IDatabase db,
        string prefix,
        CancellationToken ct)
    {
        var pattern = $"{prefix}*";
        var keys = server.Keys(db.Database, pattern: pattern, pageSize: 250)
            .Take(MaxKeysPerPrefix)
            .ToArray();

        long sampledBytes = 0;
        foreach (var key in keys)
        {
            ct.ThrowIfCancellationRequested();
            var usage = await db.ExecuteAsync("MEMORY", "USAGE", key);
            if (!usage.IsNull && long.TryParse(usage.ToString(), out var bytes))
            {
                sampledBytes += bytes;
            }
        }

        return new RedisScopeSummary(
            Prefix: prefix,
            SampledKeyCount: keys.Length,
            SampledBytes: sampledBytes,
            EstimatedBytesPerKey: keys.Length == 0 ? 0 : sampledBytes / keys.Length,
            IsSampleLimited: keys.Length >= MaxKeysPerPrefix);
    }

    private static long ReadMemoryInfo(IServer server, string key)
    {
        foreach (var section in server.Info("memory"))
        {
            foreach (var pair in section)
            {
                if (!pair.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (long.TryParse(pair.Value, out var value))
                {
                    return value;
                }
            }
        }

        return 0;
    }

    private static async Task<long> ExecuteScalarInt64Async(DbConnection connection, string sql, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = await command.ExecuteScalarAsync(ct);
        return result is null || result is DBNull ? 0 : Convert.ToInt64(result);
    }

    private static async Task<string?> ExecuteScalarStringAsync(DbConnection connection, string sql, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = await command.ExecuteScalarAsync(ct);
        return result is null || result is DBNull ? null : Convert.ToString(result);
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    public sealed record StorageDiagnosticsResponse(
        DateTimeOffset GeneratedAtUtc,
        int DatabaseCount,
        long TotalDatabaseBytes,
        IReadOnlyList<DatabaseStorageSummary> Databases,
        RedisStorageSummary Redis);

    public sealed record DatabaseStorageSummary(
        string Module,
        string Database,
        long DatabaseBytes,
        IReadOnlyList<TableStorageSummary> TopTables);

    public sealed record TableStorageSummary(
        string Schema,
        string Table,
        long TotalBytes,
        long TableBytes,
        long IndexBytes,
        long EstimatedRows);

    public sealed record RedisStorageSummary(
        bool IsAvailable,
        int DatabaseIndex,
        long TotalKeysInDatabase,
        long UsedMemoryBytes,
        long PeakUsedMemoryBytes,
        IReadOnlyList<RedisScopeSummary> ScopeBreakdown,
        string Notes);

    public sealed record RedisScopeSummary(
        string Prefix,
        int SampledKeyCount,
        long SampledBytes,
        long EstimatedBytesPerKey,
        bool IsSampleLimited);
}