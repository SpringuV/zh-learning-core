using Search.Contracts.DTOs;
using Search.Contracts.Interfaces;

namespace HanziAnhVuHsk.Api.Apis.SearchApi;
public static class UserSearchApi
{
    public static async Task<IResult> SearchUsers([AsParameters] UserSearchQueryRequest request, IUserSearchQueries userSearchQueries, CancellationToken ct)
    {
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
}