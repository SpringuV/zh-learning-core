from datetime import datetime
from typing import Literal

from pydantic import BaseModel, Field


class OcrLine(BaseModel):
    text: str
    confidence: float
    bbox: list[list[float]]


class OcrPageResult(BaseModel):
    page_index: int
    lines: list[OcrLine] = Field(default_factory=list)


class OcrImageResponse(BaseModel):
    text: str
    pages: list[OcrPageResult]


class OcrPdfJobCreated(BaseModel):
    job_id: str
    status: Literal["queued", "running", "completed", "failed"]


class OcrPdfJobStatus(BaseModel):
    job_id: str
    status: Literal["queued", "running", "completed", "failed"]
    progress: int
    created_at: datetime
    completed_at: datetime | None = None
    error: str | None = None


class OcrPdfResult(BaseModel):
    job_id: str
    status: Literal["queued", "running", "completed", "failed"]
    text: str | None = None
    pages: list[OcrPageResult] | None = None
    error: str | None = None
