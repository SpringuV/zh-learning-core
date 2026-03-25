using HanziAnhVuHsk.Api.Apis;

namespace HanziAnhVuHsk.Extensions;

public static class SearchApiExtensions
{
    public static IEndpointRouteBuilder MapSearchApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/search/v{version:int}")
            .MapSearchApi()
            .WithTags("Search Api");

        return builder;
    }

    public static RouteGroupBuilder MapSearchApi(this RouteGroupBuilder group)
    {
        group.MapPost("/documents", SearchApi.IndexDocument)
            .Accepts<SearchApi.IndexSearchDocumentRequest>("application/json");

        group.MapGet("/documents", SearchApi.SearchDocuments);

        return group;
    }
}
