namespace HanziAnhVuHsk.Apis.SearchApi;

public static class LessonSearchApi
{
    public static async Task<IResult> SearchCourses([AsParameters] CourseSearchQueryAdminRequest request, ICourseSearchQueriesService courseSearchQueriesService, CancellationToken ct)
    {
        try
        {
            if (request.Take <= 0)
            {
                return Results.BadRequest(new { message = "Take must be greater than zero." });
            }
            var results = await courseSearchQueriesService.GetCourseSearchItemAdminAsync(request, ct);
            return Results.Ok(results);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    
}