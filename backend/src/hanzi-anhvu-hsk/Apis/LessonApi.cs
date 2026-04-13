namespace HanziAnhVuHsk.Apis;

public class LessonApi
{
    #region Helpers Result
    // Helper method để xử lý error response chung
    private static IResult HandleFailureResult<T>(Result<T> result)
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

    private static IResult HandleFailureResult(Result result)
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
    #endregion

    #region Course API
    public static async Task<IResult> CreateCourse(
        [FromBody] CreateCourseRequestDTO request, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.CreateCourseAsync(request, ct);
            return result.Success ? Results.Ok(result) : HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }

    public static async Task<IResult> CourseReOrder(
        [FromBody] CourseReorderRequestDTO request, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.ReorderCoursesAsync(request, ct);
            return result.Success ? Results.Ok(result) : HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion

    #region Topic API
    public static async Task<IResult> CreateTopic(
        [FromBody] TopicCreateRequestDTO request, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.CreateTopicAsync(request, ct);
            return result.Success ? Results.Ok(result) : HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
}