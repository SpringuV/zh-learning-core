namespace Search.Contracts.DTOs;

public sealed record SearchQueryResult<T>(
    IReadOnlyList<T> Items,
    PaginationResponse Pagination);
