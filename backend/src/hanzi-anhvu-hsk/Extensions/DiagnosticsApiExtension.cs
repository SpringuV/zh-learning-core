namespace HanziAnhVuHsk.Extensions;

public static class DiagnosticsApiExtension
{
    public static IEndpointRouteBuilder MapDiagnosticsApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/diagnostics/v{version:int}")
            .MapDiagnosticsApi()
            .WithTags("Diagnostics Api");

        return builder;
    }

    public static RouteGroupBuilder MapDiagnosticsApi(this RouteGroupBuilder group)
    {
        group.MapGet("/storage/summary", DiagnosticsApi.GetStorageSummary)
            .RequireAuthorization();

        return group;
    }
}