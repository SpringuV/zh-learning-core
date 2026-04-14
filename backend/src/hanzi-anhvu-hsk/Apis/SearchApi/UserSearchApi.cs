namespace HanziAnhVuHsk.Apis.SearchApi;
public static class UserSearchApi
{
    public static async Task<IResult> SearchUsers([AsParameters] UserSearchQueryRequest request, [FromServices] IUserSearchQueriesServices userSearchQueries, CancellationToken ct)
    {
        /*
        [Controller/API] 
            → SearchUsersAsync(UserSearchQueryRequest)
                → _mediator.Send(UserSearchQueries)
                    → UserSearchQueriesHandler.Handle()
                        → SearchInternalAsync() [Elasticsearch logic]
                        → Map to UserSearchItemResponse
                        → Return result
                → Return result
        */
        try
        {
            if (request.Take <= 0)
            {
                return Results.BadRequest(new { message = "Take must be greater than zero." });
            }
            var result = await userSearchQueries.SearchUsersAsync(request, ct);
            return Results.Ok(result);
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

    public static async Task<IResult> GetUser([FromRoute] string id, [FromServices] IUserSearchQueriesServices userSearchQueries, CancellationToken ct)
    {
        try
        {
            var result = await userSearchQueries.GetAsync(Guid.Parse(id), ct);
            if (result == null)
            {
                return Results.NotFound(new { message = $"User with ID {id} not found." });
            }
            return Results.Ok(result);
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