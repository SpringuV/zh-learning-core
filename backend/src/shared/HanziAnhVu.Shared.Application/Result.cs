namespace HanziAnhVu.Shared.Contracts.DTOs;

/// <summary>
/// Generic Result pattern for API responses
/// Consistent error handling across all modules
/// </summary>
public sealed record Result<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;
    public int? ErrorCode { get; init; }

    // Factory methods
    public static Result<T> SuccessResult(T data, string message = "Operation completed successfully")
        => new() { Success = true, Data = data, Message = message };

    public static Result<T> FailureResult(string message, int errorCode)
        => new() { Success = false, Data = default, Message = message, ErrorCode = errorCode };
}

/// <summary>
/// Non-generic Result for operations that don't return data (Delete, etc.)
/// </summary>
public sealed record Result
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ErrorCode { get; init; }

    // Factory methods
    public static Result SuccessResult(string message = "Operation completed successfully")
        => new() { Success = true, Message = message };

    public static Result FailureResult(string message, int errorCode)
        => new() { Success = false, Message = message, ErrorCode = errorCode.ToString() };
}
