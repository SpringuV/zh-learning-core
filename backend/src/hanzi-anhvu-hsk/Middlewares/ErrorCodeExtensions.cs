namespace HanziAnhVu.Shared.Contracts.Enums;

/// <summary>
/// Extension methods for ErrorCode enum - HTTP layer
/// Maps error codes to HTTP status codes
/// </summary>
public static class ErrorCodeHttpExtensions
{
    /// <summary>
    /// Get HTTP status code based on error code value
    /// </summary>
    public static int ToHttpStatusCode(this ErrorCode code) => ((int)code) switch
    {
        // 404 Not Found (4041-4047)
        >= 4041 and <= 4047 => StatusCodes.Status404NotFound,
        
        // 409 Conflict (4091-4099)
        >= 4091 and <= 4099 => StatusCodes.Status409Conflict,
        
        // 400 Bad Request (4001-4007, 4101-4105)
        (>= 4001 and <= 4007) or (>= 4101 and <= 4105) => StatusCodes.Status400BadRequest,
        
        // 500 Internal Server Error (5000-5006)
        _ => StatusCodes.Status500InternalServerError
    };
}
