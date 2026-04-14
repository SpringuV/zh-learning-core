namespace HanziAnhVuHsk.Apis;

public static class MediaUploadApi
{
    public static async Task<IResult> CreateSignedUploadUrl(
        [FromBody] SignUploadUrlRequest request,
        HttpContext httpContext,
        IMediaUploadSigner mediaUploadSigner,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediaUploadSigner.CreateSignedUploadUrlAsync(
                request,
                httpContext.User,
                cancellationToken);

            return Results.Ok(new
            {
                success = true,
                data = result
            });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (MediaUploadBlockedException ex)
        {
            return Results.Json(new
            {
                success = false,
                message = ex.Message,
                errorCode = "MEDIA_UPLOAD_BLOCKED"
            }, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Cannot create signed upload URL");
        }
    }
}
