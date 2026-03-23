# zh-learning-core

Chinese learning platform with OCR pipeline for Chinese documents.

## Architecture

- `fe-hanzi-anhvu`: Next.js frontend.
- `backend/hanzi-anhvu-hsk`: .NET 8 API gateway.
- `backend/ocr-service`: Python FastAPI + PaddleOCR engine.

Request flow:

`Frontend -> .NET API -> OCR FastAPI (PaddleOCR)`

## OCR capabilities (MVP)

- OCR single image (sync): `POST /api/ocr/image`
- OCR PDF (async job):
  - `POST /api/ocr/pdf/jobs`
  - `GET /api/ocr/pdf/jobs/{jobId}`
  - `GET /api/ocr/pdf/jobs/{jobId}/result`

## Prerequisites

- Node.js 20+
- .NET SDK 8.0+
- Docker + Docker Compose

## Run with Docker

From repository root:

Backend + OCR only (recommended first step):

```bash
docker compose up --build
```

Include frontend too:

```bash
docker compose --profile frontend up --build
```

Services:

- Frontend: `http://localhost:3000`
- .NET API: `http://localhost:8080`
- OCR service: `http://localhost:8000`

## Local run without Docker

### 1) OCR service

```bash
cd backend/ocr-service
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

### 2) .NET API gateway

```bash
cd backend/hanzi-anhvu-hsk
dotnet restore
dotnet run
```

### 3) Frontend

```bash
cd fe-hanzi-anhvu
npm install
npm run dev
```

## Notes

- In Docker, `.NET` calls OCR at `http://ocr-service:8000`.
- In local dev, `.NET` uses `http://localhost:8000` from `appsettings.Development.json`.
- OCR service supports image preprocessing (grayscale, denoise, contrast) for better Chinese text accuracy.

## AI Quick Start (Session Continuity)

Use this startup sequence in every new AI session:

1. Read `./.ai-context/SESSION_HANDOFF.md`
2. Read `./.ai-context/PROJECT_INDEX.md`
3. Read `./.github/copilot-instructions.md`
4. Then open only task-relevant files

If you open only backend in Visual Studio, use backend-local context files:

1. Read `./backend/.ai-context/SESSION_HANDOFF.md`
2. Read `./backend/.ai-context/PROJECT_INDEX.md`
3. Read `./backend/.github/copilot-instructions.md`

After each non-trivial task, update the relevant `SESSION_HANDOFF.md` with:

- Files changed
- Decisions and assumptions
- Validation commands and outcomes
- Next recommended action

// chạy thêm frontend
// docker compose --profile frontend up --build
