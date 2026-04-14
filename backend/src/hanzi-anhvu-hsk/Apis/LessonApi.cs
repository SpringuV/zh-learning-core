using FluentValidation;

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

    #region Exercise API
    public static async Task<IResult> CreateExercise(
        [FromBody] ExerciseCreateRequestDTO request, 
        ILessonService lessonService,
        ILogger<LessonApi> logger,
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.CreateExerciseAsync(request, ct);
            return result.Success ? Results.Ok(result) : HandleFailureResult(result);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "request" : e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

            logger.LogWarning(
                ex,
                "Validation failed in CreateExercise for TopicId {TopicId}, ExerciseType {ExerciseType}",
                request.TopicId,
                request.ExerciseType);

            return Results.ValidationProblem(
                errors,
                title: "Validation failed",
                detail: "One or more validation errors occurred.",
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
}