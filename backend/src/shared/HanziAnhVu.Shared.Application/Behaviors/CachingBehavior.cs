using MediatR;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace HanziAnhVu.Shared.Application.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>(
    IDistributedCache distributedCache, // dependency injection của IDistributedCache để tương tác với cache store (ví dụ: Redis).
    ICacheVersionService cacheVersionService,
    ILogger<CachingBehavior<TRequest, TResponse>> logger) // dependency injection của ILogger để ghi log thông tin về cache hits/misses.
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Kiểm tra nếu request không implement ICacheableRequest<TResponse> thì bỏ qua caching và tiếp tục pipeline bình thường.
        if (request is not ICacheableRequest<TResponse> cacheableRequest)
        {
            return await next(cancellationToken);
        }
        // Nếu CacheKey trống hoặc null thì bỏ qua caching và tiếp tục pipeline bình thường.
        if (string.IsNullOrWhiteSpace(cacheableRequest.CacheKey))
        {
            return await next(cancellationToken);
        }

        var effectiveCacheKey = await BuildEffectiveCacheKeyAsync(request, cacheableRequest, cancellationToken);

        // Cố gắng lấy response từ cache bằng CacheKey. Nếu có thì deserialize và return luôn mà không cần gọi handler.
        //  Nếu không có thì gọi handler để lấy response, sau đó serialize và lưu vào cache với CacheKey
        var cachedBytes = await _distributedCache.GetAsync(effectiveCacheKey, cancellationToken);
        if (cachedBytes is { Length: > 0 })
        {
            var cachedResponse = JsonSerializer.Deserialize<TResponse>(cachedBytes);
            if (cachedResponse is not null)
            {
                _logger.LogInformation("Cache hit for request {RequestName} with key {CacheKey}", typeof(TRequest).Name, effectiveCacheKey);
                return cachedResponse;
            }
        }

        _logger.LogInformation("Cache miss for request {RequestName} with key {CacheKey}", typeof(TRequest).Name, effectiveCacheKey);

        var response = await next(cancellationToken);
        var payload = JsonSerializer.SerializeToUtf8Bytes(response);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheableRequest.CacheDuration,
        };
        await _distributedCache.SetAsync(effectiveCacheKey, payload, options, cancellationToken);

        return response;
    }

    private async Task<string> BuildEffectiveCacheKeyAsync(
        TRequest request,
        ICacheableRequest<TResponse> cacheableRequest,
        CancellationToken cancellationToken)
    {
        // Để tránh cache key quá dài khi CacheKey có nhiều trường hoặc giá trị lớn, 
        // thì sẽ hash CacheKey để tạo thành một chuỗi ngắn hơn, đồng thời vẫn đảm bảo 
        // tính duy nhất của cache key dựa trên nội dung của CacheKey.
        var requestKeyHash = ComputeCacheKeyHash(cacheableRequest.CacheKey);

        // Nếu request không implement ICacheScopeRequest hoặc CacheScope 
        // trống thì sử dụng cache key đơn giản chỉ gồm tên request và hash của CacheKey.
        if (request is not ICacheScopeRequest scopedRequest || string.IsNullOrWhiteSpace(scopedRequest.CacheScope))
        {
            return $"{typeof(TRequest).Name}:h:{requestKeyHash}";
        }

        try
        {
            // Nếu request implement ICacheScopeRequest và có CacheScope thì sẽ kết hợp 
            // version token của cache scope vào cache key.
            var versionToken = await _cacheVersionService.GetVersionTokenAsync(scopedRequest.CacheScope, cancellationToken);
            return $"{scopedRequest.CacheScope}:v:{versionToken}:h:{requestKeyHash}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to resolve cache scope version for {RequestName}. Fallback to original key.",
                typeof(TRequest).Name);

            return $"{typeof(TRequest).Name}:h:{requestKeyHash}";
        }
    }

    private static string ComputeCacheKeyHash(string rawCacheKey)
    {
        var bytes = Encoding.UTF8.GetBytes(rawCacheKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}