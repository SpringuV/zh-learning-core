using MediatR;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace HanziAnhVu.Shared.Application.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>(
    IDistributedCache distributedCache, // dependency injection của IDistributedCache để tương tác với cache store (ví dụ: Redis).
    ILogger<CachingBehavior<TRequest, TResponse>> logger) // dependency injection của ILogger để ghi log thông tin về cache hits/misses.
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IDistributedCache _distributedCache = distributedCache;
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

        // Cố gắng lấy response từ cache bằng CacheKey. Nếu có thì deserialize và return luôn mà không cần gọi handler.
        //  Nếu không có thì gọi handler để lấy response, sau đó serialize và lưu vào cache với CacheKey
        var cachedBytes = await _distributedCache.GetAsync(cacheableRequest.CacheKey, cancellationToken);
        if (cachedBytes is { Length: > 0 })
        {
            var cachedResponse = JsonSerializer.Deserialize<TResponse>(cachedBytes);
            if (cachedResponse is not null)
            {
                _logger.LogInformation("Cache hit for request {RequestName} with key {CacheKey}", typeof(TRequest).Name, cacheableRequest.CacheKey);
                return cachedResponse;
            }
        }

        _logger.LogInformation("Cache miss for request {RequestName} with key {CacheKey}", typeof(TRequest).Name, cacheableRequest.CacheKey);

        var response = await next(cancellationToken);
        var payload = JsonSerializer.SerializeToUtf8Bytes(response);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheableRequest.CacheDuration,
        };
        await _distributedCache.SetAsync(cacheableRequest.CacheKey, payload, options, cancellationToken);

        return response;
    }
}