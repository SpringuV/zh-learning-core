namespace HanziAnhVuHsk.Extensions;

public static class MediaUploadApiExtension
{
    public static IEndpointRouteBuilder MapMediaUploadApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/media/v{version:int}")
            .MapMediaUploadApi()
            .WithTags("Media Upload Api")
            .RequireAuthorization();

        return builder;
    }

    public static RouteGroupBuilder MapMediaUploadApi(this RouteGroupBuilder group)
    {
        // Endpoint to create a signed URL for media upload
        group.MapPost("/uploads/sign", MediaUploadApi.CreateSignedUploadUrl)
            .WithName("CreateSignedUploadUrl");

        return group;
    }
}
