using Apis;
using Interface;

namespace HanziAnhVuHsk.Extensions;
public static class OcrApiExtensions
{
    public static IEndpointRouteBuilder MapOcrApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/ocr/v{version:int}").MapOcrApi().WithTags("Ocr Service Api");
        return builder;
    }

    public static RouteGroupBuilder MapOcrApi(this RouteGroupBuilder group)
    {

        group.MapPost("/image", OcrApi.PostImage)
            .Accepts<IFormFile>("multipart/form-data") // Specify that the endpoint accepts a file upload
            .DisableAntiforgery(); // Disable antiforgery for this endpoint since it's an API endpoint and not a form submission

        group.MapPost("/pdf/jobs", OcrApi.PostPdfJobAsync)
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery();

        group.MapGet("/pdf/jobs/{jobId}", OcrApi.GetPdfJobStatusAsync);
        group.MapGet("/pdf/jobs/{jobId}/result", OcrApi.GetPdfJobResultAsync);
        return group;
    }
}

