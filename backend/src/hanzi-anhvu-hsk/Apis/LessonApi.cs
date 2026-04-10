namespace HanziAnhVuHsk.Apis;

public class LessonApi
{
    public static async Task<IResult> CreateCourse([FromBody] CreateCourseRequestDTO request, ILessonService lessonService, CancellationToken ct)
    {
        try
        {
            var result = await lessonService.CreateCourseAsync(request, ct);
            
            if (!result.Success)
            {
                var statusCode = ((ErrorCode)result.ErrorCode!.Value).ToHttpStatusCode();
                return Results.Json(
                    new { success = false, message = result.Message, errorCode = result.ErrorCode },
                    statusCode: statusCode
                );
            }
            
            return Results.Ok(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    }
}