using FluentValidation;
namespace HanziAnhVuHsk.Apis.LessonApi;

public class CourseApi
{

    #region CreateCourse
    public static async Task<IResult> CreateCourse(
        [FromBody] CreateCourseRequestDTO request, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.CreateCourseAsync(request, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region Publish
    public static async Task<IResult> PublishCourse(
        [FromRoute] Guid courseId, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.PublishCourseAsync(courseId, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region UnPublish
    public static async Task<IResult> UnPublishCourse(
        [FromRoute] Guid courseId, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.UnPublishCourseAsync(courseId, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region ReOrder
    public static async Task<IResult> CourseReOrder(
        [FromBody] CourseReorderRequestDTO request, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.ReorderCoursesAsync(request, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
    #region Update
    public static async Task<IResult> UpdateCourse(
        [FromBody] UpdateCourseRequestDTO request, 
        ILessonService lessonService, 
        CancellationToken ct)
    {
        try
        {
            var result = await lessonService.UpdateCourseAsync(request, ct);
            return result.Success ? Results.Ok(result) : Helper.HandleFailureResult(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
    #endregion
 
}