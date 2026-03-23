from __future__ import annotations

from datetime import datetime, timezone
from threading import Lock
from uuid import uuid4

from fastapi import BackgroundTasks, FastAPI, File, HTTPException, UploadFile

from app.config import settings
from app.ocr_engine import ocr_engine
from app.schemas import OcrImageResponse, OcrPdfJobCreated, OcrPdfJobStatus, OcrPdfResult

app = FastAPI(title=settings.app_name)

jobs: dict[str, dict] = {}
jobs_lock = Lock()


@app.get("/health")
def health() -> dict[str, str]:
    return {"status": "ok", "service": settings.app_name}


@app.post("/v1/ocr/image", response_model=OcrImageResponse)
async def ocr_image(file: UploadFile = File(...)) -> OcrImageResponse:
    content = await file.read()
    _validate_file_size(content)
    page = ocr_engine.run_on_image_bytes(content)
    text = "\n".join(line.text for line in page.lines)
    return OcrImageResponse(text=text, pages=[page])


@app.post("/v1/ocr/pdf/jobs", response_model=OcrPdfJobCreated)
async def create_pdf_job(background_tasks: BackgroundTasks, file: UploadFile = File(...)) -> OcrPdfJobCreated:
    content = await file.read()
    _validate_file_size(content)

    job_id = str(uuid4())
    created_at = datetime.now(timezone.utc)

    with jobs_lock:
        jobs[job_id] = {
            "job_id": job_id,
            "status": "queued",
            "progress": 0,
            "created_at": created_at,
            "completed_at": None,
            "error": None,
            "text": None,
            "pages": None,
        }

    background_tasks.add_task(_run_pdf_job, job_id, content)
    return OcrPdfJobCreated(job_id=job_id, status="queued")


@app.get("/v1/ocr/pdf/jobs/{job_id}", response_model=OcrPdfJobStatus)
def get_pdf_job_status(job_id: str) -> OcrPdfJobStatus:
    with jobs_lock:
        job = jobs.get(job_id)

    if not job:
        raise HTTPException(status_code=404, detail="job not found")

    return OcrPdfJobStatus(
        job_id=job["job_id"],
        status=job["status"],
        progress=job["progress"],
        created_at=job["created_at"],
        completed_at=job["completed_at"],
        error=job["error"],
    )


@app.get("/v1/ocr/pdf/jobs/{job_id}/result", response_model=OcrPdfResult)
def get_pdf_job_result(job_id: str) -> OcrPdfResult:
    with jobs_lock:
        job = jobs.get(job_id)

    if not job:
        raise HTTPException(status_code=404, detail="job not found")

    return OcrPdfResult(
        job_id=job["job_id"],
        status=job["status"],
        text=job["text"],
        pages=job["pages"],
        error=job["error"],
    )


def _run_pdf_job(job_id: str, pdf_content: bytes) -> None:
    _update_job(job_id, status="running", progress=5)

    try:
        page_bytes = ocr_engine.extract_pdf_pages(pdf_content)
        all_pages = []

        for index, page in enumerate(page_bytes):
            page_result = ocr_engine.run_on_image_bytes(page, page_index=index)
            all_pages.append(page_result)
            progress = int(((index + 1) / max(len(page_bytes), 1)) * 100)
            _update_job(job_id, progress=progress)

        full_text = "\n\n".join("\n".join(line.text for line in p.lines) for p in all_pages)
        _update_job(
            job_id,
            status="completed",
            progress=100,
            completed_at=datetime.now(timezone.utc),
            text=full_text,
            pages=[p.model_dump() for p in all_pages],
        )
    except Exception as exc:  # pragma: no cover
        _update_job(
            job_id,
            status="failed",
            completed_at=datetime.now(timezone.utc),
            error=str(exc),
        )


def _update_job(job_id: str, **fields: object) -> None:
    with jobs_lock:
        if job_id in jobs:
            jobs[job_id].update(fields)


def _validate_file_size(content: bytes) -> None:
    max_size_bytes = settings.max_file_size_mb * 1024 * 1024
    if len(content) > max_size_bytes:
        raise HTTPException(status_code=413, detail="file too large")
