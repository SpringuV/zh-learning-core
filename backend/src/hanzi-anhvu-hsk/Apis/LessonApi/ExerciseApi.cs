using FluentValidation;

namespace HanziAnhVuHsk.Apis.LessonApi;

public class ExerciseApi
{
    
    #region Create Exercise
    public static async Task<IResult> CreateExercise(
        [FromBody] ExerciseCreateRequestDTO request, 
        ILessonService lessonService,
        ILogger<ExerciseApi> logger,
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.CreateExerciseAsync(request, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
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
    #region Publish
    public static async Task<IResult> PublishExercise(
        [FromRoute] Guid exerciseId, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.PublishExerciseAsync(exerciseId, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region UnPublish
    public static async Task<IResult> UnPublishExercise(
        [FromRoute] Guid exerciseId, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.UnPublishExerciseAsync(exerciseId, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region ReOrder
    public static async Task<IResult> ExerciseReOrder(
        [FromBody] ExerciseReorderRequestDTO request, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.ReorderExercisesAsync(request, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region Update Exercise
    public static async Task<IResult> UpdateExercise(
        [FromBody] UpdateExerciseRequestDTO request, 
        ILessonService lessonService, 
        ILogger<ExerciseApi> logger,
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.UpdateExerciseAsync(request, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        // from FluentValidation, we want to catch this exception to return 400 with details of validation errors
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "request" : e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

            logger.LogWarning(
                ex,
                "Validation failed in UpdateExercise for ExerciseId {ExerciseId}, ExerciseType {ExerciseType}",
                request.ExerciseId,
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
    #region Delete Exercise
    public static async Task<IResult> DeleteExercise(
        [FromRoute] Guid exerciseId, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.DeleteExerciseAsync(exerciseId, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion

    #region SaveAnswer
    public static async Task<IResult> SaveAnswer(
        [FromBody] SaveAnswerRequestDTO request,
        ILessonService lessonService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        try
        {
            var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }
            var result = await lessonService.SaveAnswerAsync(request, userId, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
}