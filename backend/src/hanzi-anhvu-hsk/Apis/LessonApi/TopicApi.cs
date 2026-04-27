namespace HanziAnhVuHsk.Apis.LessonApi;

public class TopicApi
{
    #region CreateTopic
    public static async Task<IResult> CreateTopic(
        [FromBody] TopicCreateRequestDTO request, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.CreateTopicAsync(request, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region Publish
    public static async Task<IResult> PublishTopic(
        [FromRoute] Guid topicId, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.PublishTopicAsync(topicId, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region UnPublish
    public static async Task<IResult> UnPublishTopic(
        [FromRoute] Guid topicId, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.UnPublishTopicAsync(topicId, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region ReOrder
    public static async Task<IResult> TopicReOrder(
        [FromBody] TopicReorderRequestDTO request, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.ReorderTopicsAsync(request, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region Update Topic
    public static async Task<IResult> UpdateTopic(
        [FromBody] UpdateTopicRequestDTO request, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.UpdateTopicAsync(request, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region Delete Topic
    public static async Task<IResult> DeleteTopic(
        [FromRoute] Guid topicId, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.DeleteTopicAsync(topicId, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }

    #endregion
    #region StartLearning
    public static async Task<IResult> StartLearning(
        [FromRoute] string slugTopic,
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
            var request = new StartLearningRequestDTO(
                slugTopic,
                userId
            );
            var result = await lessonService.StartLearningAsync(request, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region CompleteLearning
    public static async Task<IResult> CompleteLearning(
        [FromBody] CompleteLearningSessionRequestDTO request,
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
            var result = await lessonService.CompleteLearningAsync(request, userId, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
}