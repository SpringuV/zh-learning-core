using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HanziAnhVu.Shared.Infrastructure.Cache;

public class RedisCacheAdapter<T> : ICache<T>
{
    private readonly IDistributedCache _distributedCache;

    public RedisCacheAdapter(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync(string key)
    {
        var bytes = await _distributedCache.GetAsync(key);
        if (bytes is null || bytes.Length == 0)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(bytes);
    }

    public async Task SetAsync(string key, T value, TimeSpan? expiration = null)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        var options = new DistributedCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration;
        }

        await _distributedCache.SetAsync(key, bytes, options);
    }

    public Task RemoveAsync(string key)
    {
        return _distributedCache.RemoveAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var bytes = await _distributedCache.GetAsync(key);
        return bytes is not null;
    }
}
