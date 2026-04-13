namespace HanziAnhVu.Shared.Application;

// out TResponse để cho phép covariance, tức là có thể sử dụng 
// ICacheableRequest<BaseResponse> thay cho ICacheableRequest<DerivedResponse>.
// Interface này định nghĩa contract cho các request có thể được cache.
// ví dụ: các query request thường implement interface này để được caching, 
// còn command request thì không cần.
public interface ICacheableRequest<out TResponse> 
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}