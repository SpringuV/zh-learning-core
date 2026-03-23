namespace Model;

public sealed class OcrServiceOptions
{
    public const string SectionName = "OcrService";

    public string BaseUrl { get; init; } = "http://ocr-service:8000";
    public int TimeoutSeconds { get; init; } = 120;
}
