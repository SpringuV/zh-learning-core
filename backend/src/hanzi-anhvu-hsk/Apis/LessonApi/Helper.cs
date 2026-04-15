namespace HanziAnhVuHsk.Apis.LessonApi;

public static class Helper
{
    // Helper method để xử lý error response chung
    public static IResult HandleFailureResult<T>(Result<T> result)
    {
        if (int.TryParse(result.ErrorCode?.ToString(), out var errorCode))
        {
            var statusCode = ((ErrorCode)errorCode).ToHttpStatusCode();
            return Results.Json(
                new { success = false, message = result.Message, errorCode = result.ErrorCode },
                statusCode: statusCode
            );
        }

        return Results.Json(
            new { success = false, message = result.Message, errorCode = result.ErrorCode },
            statusCode: StatusCodes.Status500InternalServerError
        );
    }

    public static IResult HandleFailureResult(Result result)
    {
        if (int.TryParse(result.ErrorCode?.ToString(), out var errorCode))
        {
            var statusCode = ((ErrorCode)errorCode).ToHttpStatusCode();
            return Results.Json(
                new { success = false, message = result.Message, errorCode = result.ErrorCode },
                statusCode: statusCode
            );
        }

        return Results.Json(
            new { success = false, message = result.Message, errorCode = result.ErrorCode },
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
}