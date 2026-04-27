namespace HanziAnhVuHsk.Apis.SearchApi;

public static class LessonSearchApi
{
    #region Course dashboard
    public static async Task<IResult> LoadCoursesForDashboardClient([FromServices] ICourseSearchQueriesService courseSearchQueriesService, CancellationToken ct)
    {
        try
        {
            var result = await courseSearchQueriesService.GetCourseForDashboardClientAsync(ct);
            return result.Success ? Results.Ok(result) : LessonApi.Helper.HandleFailureResult(result);
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
    #endregion
    #region Course Admin
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
    #endregion
    #region Topic Client
    public static async Task<IResult> SearchTopicsForClient([FromRoute] string slug, [FromServices] ITopicSearchQueriesService topicSearchQueriesService, HttpContext httpContext, CancellationToken ct)
    {
        try
        {
            var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var results = await topicSearchQueriesService.GetTopicForDashboardClientAsync(slug, userId, ct);
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
    #endregion
    #region Topic Admin
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
    #endregion

    #region TopicOverview
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
    #endregion
    #region ExerciseOverview
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
    #endregion

    #region Exercise Admin
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
    #endregion

    #region TopicDetail Admin
    public static async Task<IResult> SearchTopicDetail([FromRoute] Guid topicId, [FromServices] ITopicSearchQueriesService topicSearchQueriesService, CancellationToken ct)
    {
        try
        {
            var results = await topicSearchQueriesService.GetTopicDetailSearchItemAdminAsync(topicId, ct);
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
    #endregion
    #region ExerciseDetail Admin
    public static async Task<IResult> SearchExerciseDetail([FromRoute] Guid exerciseId, [FromServices] IExerciseSearchQueriesService exerciseSearchQueriesService, CancellationToken ct)
    {
        try
        {
            var results = await exerciseSearchQueriesService.GetExerciseDetailSearchItemAdminAsync(exerciseId, ct);
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
    #endregion
    #region CountinueExercisesSessionForTopicClient
    public static async Task<IResult> CountinueExercisesSessionForTopicClient([FromRoute] string slug, [FromServices] IExerciseSearchQueriesService exerciseSearchQueriesService, HttpContext httpContext, CancellationToken ct)
    {
        try
        {
            var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var results = await exerciseSearchQueriesService.CountinueExercisesSessionForTopicClientAsync(slug, userId, ct);
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
    #endregion
    #region ExercisePracticeItem
    // hiển thị các câu hỏi của bài tập khi user luyện tập
    public static async Task<IResult> GetExerciseSessionPracticeItemWithoutAnswer([FromRoute] Guid exerciseId, [FromServices] IExerciseSearchQueriesService exerciseSearchQueriesService, CancellationToken ct)
    {
        try
        {
            var results = await exerciseSearchQueriesService.GetExerciseSessionPracticeItemWithoutAnswerAsync(exerciseId, ct);
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
    #endregion

    #region GetSessionItemsSnapshot
    public static async Task<IResult> GetSessionItemsSnapshot([FromRoute] Guid sessionId, [FromRoute] string slug, [FromServices] ITopicSearchQueriesService topicSearchQueriesService, HttpContext httpContext, CancellationToken ct)
    {
        try
        {
            var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }
            var request = new ExerciseSessionItemsSnapshotRequest(
                SessionId: sessionId,
                Slug: slug,
                UserId: userId
            );
            var results = await topicSearchQueriesService.GetSessionItemsSnapshotAsync(request, ct);
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
    #endregion
}