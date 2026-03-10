# OCR Service (FastAPI + PaddleOCR)

## Run locally

```bash
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

## API

- `GET /health`
- `POST /v1/ocr/image` (multipart form-data with `file`)
- `POST /v1/ocr/pdf/jobs` (multipart form-data with `file`)
- `GET /v1/ocr/pdf/jobs/{job_id}`
- `GET /v1/ocr/pdf/jobs/{job_id}/result`

## Environment variables

- `OCR_LANG` default `ch`
- `PADDLE_OCR_USE_GPU` default `false`
- `OCR_PREPROCESS` default `true`
- `MAX_FILE_SIZE_MB` default `20`
