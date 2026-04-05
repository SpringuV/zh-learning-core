using Search.Contracts.Interfaces;
namespace Search.Contracts.DTOs;

public sealed record UserSearchQueryRequest(
    string? Email = null,
    string? Username = null,
    bool? IsActive = null,
    string? PhoneNumber = null,
    int Take = 30,
    // keyset pagination sẽ dùng SearchAfter, sẽ lấy những document có CreatedAt nhỏ hơn timestamp của document cuối cùng trong page trước, để tránh việc skip nhiều document khi trang có nhiều kết quả
    string? SearchAfterCreatedAt = null, // dùng để phân trang, timestamp của document cuối cùng trong page trước, sẽ lấy những document có CreatedAt nhỏ hơn timestamp này
    UserSortBy SortBy = UserSortBy.CreatedAt,
    bool OrderByDescending = true,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null);

    

