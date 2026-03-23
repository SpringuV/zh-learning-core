using Interface;

namespace Apis;
public  class OcrApi
{
    public static async Task<IResult> PostImage(IFormFile file, IOcrClient ocrClient, CancellationToken ct)
    {
        var result = await ocrClient.PostFileAsync("/v1/ocr/image", file, ct);
        return Results.Ok(result);
    }

    public static async Task<IResult> PostPdfJobAsync(IFormFile file, IOcrClient ocrClient, CancellationToken ct)
    {
        var result = await ocrClient.PostFileAsync("/v1/ocr/pdf/jobs", file, ct);
        return Results.Ok(result);
    }

    public static async Task<IResult> GetPdfJobStatusAsync(string jobId, IOcrClient ocrClient, CancellationToken ct)
    {
        var result = await ocrClient.GetAsync($"/v1/ocr/pdf/jobs/{jobId}", ct);
        return Results.Ok(result);
    }

    public static async Task<IResult> GetPdfJobResultAsync(string jobId, IOcrClient ocrClient, CancellationToken ct)
    {
        var result = await ocrClient.GetAsync($"/v1/ocr/pdf/jobs/{jobId}/result", ct);
        return Results.Ok(result);
    }
}
