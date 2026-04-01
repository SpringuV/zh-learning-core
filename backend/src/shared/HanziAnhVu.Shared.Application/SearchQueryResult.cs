namespace HanziAnhVu.Shared.Application;

public sealed record SearchQueryResult<T>(
    long Total,
    IReadOnlyList<T> Items,
    bool HasNextPage,
    string NextCursor);
