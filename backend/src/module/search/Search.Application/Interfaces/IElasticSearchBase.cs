using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Search.Application.Interfaces;

public interface IElasticSearchBase<T>
{
    Task IndexAsync(T document, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    // Tìm kiếm tài liệu dựa trên biểu thức lambda
    // ví dụ: x => x.Name.Contains("keyword")
    Task<SearchResult<T>> SearchAsync(string? email = null, string? username = null, bool? isActive = null, CancellationToken cancellationToken = default);
}

public class SearchResult<T>
{
    public long Total { get; set; }
    public List<T> Documents { get; set; } = new List<T>();
}
