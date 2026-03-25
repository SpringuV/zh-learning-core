using Interface;

namespace Apis;
public  class OcrApi // handle OCR related endpoints, delegate the actual work to IOcrClient which is implemented in OcrClient, and OcrClient will call the OCR service (e.g. Google Cloud Vision API) to perform OCR tasks
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
