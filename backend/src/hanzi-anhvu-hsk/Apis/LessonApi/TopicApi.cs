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
}