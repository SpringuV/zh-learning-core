using System.Text;
using HanziAnhVu.Shared.Application;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace HanziAnhVu.Shared.Infrastructure;

public class DistributedCacheVersionService(
    IDistributedCache distributedCache,
    ILogger<DistributedCacheVersionService> logger) : ICacheVersionService
{
    private const string ScopeVersionKeyPrefix = "cache:scope-version:";

    // ScopeVersionCacheOptions định nghĩa thời gian tồn tại của version token trong cache, 
    // đảm bảo rằng các token sẽ tự động hết hạn sau một khoảng thời gian dài 
    private static readonly DistributedCacheEntryOptions ScopeVersionCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) // token sẽ tự động hết hạn sau 15 phút nếu không có invalidate nào được gọi, tránh cache bẩn
    };

    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly ILogger<DistributedCacheVersionService> _logger = logger;

    public async Task<string> GetVersionTokenAsync(string scope, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return "0";
        }

        var scopeKey = BuildScopeVersionKey(scope);
        var raw = await _distributedCache.GetAsync(scopeKey, ct);
        if (raw is null || raw.Length == 0)
        {
            return "0";
        }
        // token được lưu dưới dạng byte[] trong cache, convert về string để sử dụng làm version token trong cache key.
        return Encoding.UTF8.GetString(raw);
    }

    // invalidate scope bằng cách tạo một version token 
    // mới và lưu vào cache với key version của scope đó,
    public async Task InvalidateScopeAsync(string scope, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return;
        }

        var scopeKey = BuildScopeVersionKey(scope);
        // GUID đủ để đảm bảo token mới khác token cũ, đồng thời ngắn hơn để giảm chiều dài cache key.
        var nextToken = Guid.NewGuid().ToString("N");

        await _distributedCache.SetAsync(
            scopeKey, // key version của scope để invalidate theo nhóm cache
            Encoding.UTF8.GetBytes(nextToken), // token mới để tất cả cache key có version token cũ sẽ trở nên không hợp lệ
            ScopeVersionCacheOptions, // đảm bảo token sẽ tự động hết hạn sau một khoảng thời gian dài để tránh cache bẩn nếu không có invalidate nào được gọi trong thời gian dài
            ct);

        _logger.LogInformation("Cache scope invalidated: {CacheScope}", scope);
    }
    // BuildScopeVersionKey tạo key version theo scope để invalidate theo nhóm cache.
    // ví dụ: nếu scope là "CourseAdminSearch" thì key version sẽ là 
    // "cache:scope-version:courseadminsearch", khi invalidate scope này thì sẽ 
    // update version token tại key này, tất cả cache key có chứa version token cũ 
    // sẽ trở nên không hợp lệ và bị coi như cache miss, buộc phải gọi handler để lấy 
    // dữ liệu mới và cache lại với version token mới.
    private static string BuildScopeVersionKey(string scope)
    {
        return $"{ScopeVersionKeyPrefix}{scope.Trim().ToLowerInvariant()}";
    }
}
