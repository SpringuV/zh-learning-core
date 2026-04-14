
namespace HanziAnhVu.Shared.Contracts.Enums;

/// <summary>
/// Global error codes for API responses
/// Format: CATEGORY_DESCRIPTION = ErrorCodeValue
/// </summary>
public enum ErrorCode: int
{
    // Validation errors (400x)
    VALIDATION = 4001,
    INVALID_ID = 4002,
    INVALID_ENUM = 4007,
    REQUIRED_FIELD = 4006,
    INVALID_ARGUMENT = 4000,
    
    // Not found errors (404x)
    NOTFOUND = 4041,
    
    // Conflict errors (409x)
    EXISTS = 4091,
    DUPLICATE = 4099,
    
    // State errors (410x)
    INVALID_STATE = 4101,
    NOT_PUBLISHED = 4102,
    ALREADY_PUBLISHED = 4103,
    NOT_DRAFT = 4104,
    
    // Database errors (50xx)
    DATABASE_ERROR = 5001,
    
    // Internal errors (50xx)
    INTERNAL_ERROR = 5000,
    UNEXPECTED = 5005
}

public static class ErrorCodeExtensions
{
    /// <summary>
    /// Convert ErrorCode to string
    /// </summary>
    public static string ToErrorCodeString(this ErrorCode code) => code.ToString();
}

