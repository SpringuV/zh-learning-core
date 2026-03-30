using System;
using System.Collections.Generic;
using System.Text;

namespace HanziAnhVu.Shared.Infrastructure;

/// <summary>
/// Abstraction for cache — dùng cho mọi kiểu dữ liệu.
/// Để phân biệt "chưa cache" với "cache với giá trị 0", dùng nullable type:
///   ICache<int?>  → GetAsync trả null (chưa có) hoặc int? (có giá trị)
///   ICache<List<T>> → GetAsync trả null (chưa có) hoặc List<T>
/// </summary>
public interface ICache<T>
{
    Task<T?> GetAsync(string key);
    Task SetAsync(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}
