namespace HanziAnhVuHsk.Apis.SearchApi;

public static class LessonSearchApi
{
    public static async Task<IResult> SearchCourses([AsParameters] CourseSearchQueryAdminRequest request, [FromServices] ICourseSearchQueriesService courseSearchQueriesService, CancellationToken ct)
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

    public static async Task<IResult> SearchTopics([AsParameters] TopicSearchQueryRequest request, [FromServices] ITopicSearchQueriesService topicSearchQueriesService, CancellationToken ct)
    {
        try
        {
            if (request.Take <= 0)
            {
                return Results.BadRequest(new { message = "Take must be greater than zero." });
            }
            var results = await topicSearchQueriesService.SearchTopicAdminAsync(request, ct);
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
    
    // This API is used to search for topics with course metadata, which is used in the topic management page.
    public static async Task<IResult> SearchCourseTopicsOverview([AsParameters] TopicSearchQueryRequest request, [FromServices] ITopicSearchQueriesService topicSearchQueriesService, CancellationToken ct)
    {
        try
        {
            if (request.Take <= 0)
            {
                return Results.BadRequest(new { message = "Take must be greater than zero." });
            }

            var results = await topicSearchQueriesService.SearchTopicAdminWithCourseMetadataAsync(request, ct);
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

    public static async Task<IResult> SearchTopicExercisesOverview([AsParameters] ExerciseSearchQueryRequest request, [FromServices] IExerciseSearchQueriesService exerciseSearchQueriesService, CancellationToken ct)
    {
        try
        {
            if (request.Take <= 0)
            {
                return Results.BadRequest(new { message = "Take must be greater than zero." });
            }
            var results = await exerciseSearchQueriesService.SearchExerciseAdminWithTopicMetadataAsync(request, ct);
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

    public static async Task<IResult> SearchExercises([AsParameters] ExerciseSearchQueryRequest request, [FromServices] IExerciseSearchQueriesService exerciseSearchQueriesService, CancellationToken ct)
    {
        try
        {
            if (request.Take <= 0)
            {
                return Results.BadRequest(new { message = "Take must be greater than zero." });
            }
            var results = await exerciseSearchQueriesService.SearchExerciseItemAdminAsync(request, ct);
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