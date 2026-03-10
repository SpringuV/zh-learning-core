# hanzi-anhvu-hsk (.NET API Gateway)

## Run locally

```bash
dotnet restore
dotnet run
```

Default URL: `http://localhost:5208` (or configured by ASP.NET).

## Endpoints

- `GET /health`
- `POST /api/ocr/image` (multipart form-data, field `file`)
- `POST /api/ocr/pdf/jobs` (multipart form-data, field `file`)
- `GET /api/ocr/pdf/jobs/{jobId}`
- `GET /api/ocr/pdf/jobs/{jobId}/result`

## Configuration

`appsettings.json`:

```json
"OcrService": {
  "BaseUrl": "http://ocr-service:8000",
  "TimeoutSeconds": 120
}
```

In local dev outside Docker, `appsettings.Development.json` points to `http://localhost:8000`.
