from __future__ import annotations

from io import BytesIO

import cv2
import numpy as np
import pypdfium2 as pdfium
from paddleocr import PaddleOCR
from PIL import Image

from app.config import settings
from app.schemas import OcrLine, OcrPageResult


class OcrEngine:
    def __init__(self) -> None:
        self._ocr = PaddleOCR(
            lang=settings.ocr_lang,
            use_gpu=settings.use_gpu,
            use_angle_cls=True,
            show_log=False,
        )

    def preprocess(self, image_np: np.ndarray) -> np.ndarray:
        if not settings.preprocess_enabled:
            return image_np

        gray = cv2.cvtColor(image_np, cv2.COLOR_BGR2GRAY)
        denoised = cv2.fastNlMeansDenoising(gray, h=10)
        contrasted = cv2.equalizeHist(denoised)
        return cv2.cvtColor(contrasted, cv2.COLOR_GRAY2BGR)

    def run_on_image_bytes(self, image_bytes: bytes, page_index: int = 0) -> OcrPageResult:
        image = Image.open(BytesIO(image_bytes)).convert("RGB")
        image_np = np.array(image)
        image_np = cv2.cvtColor(image_np, cv2.COLOR_RGB2BGR)
        image_np = self.preprocess(image_np)
        result = self._ocr.ocr(image_np, cls=True)

        lines: list[OcrLine] = []
        if result and result[0]:
            for item in result[0]:
                bbox = [[float(p[0]), float(p[1])] for p in item[0]]
                text = str(item[1][0])
                confidence = float(item[1][1])
                lines.append(OcrLine(text=text, confidence=confidence, bbox=bbox))

        return OcrPageResult(page_index=page_index, lines=lines)

    def extract_pdf_pages(self, pdf_bytes: bytes) -> list[bytes]:
        pdf = pdfium.PdfDocument(pdf_bytes)
        rendered_pages: list[bytes] = []

        for index in range(len(pdf)):
            page = pdf[index]
            bitmap = page.render(scale=2)
            pil_image = bitmap.to_pil()
            with BytesIO() as output:
                pil_image.save(output, format="PNG")
                rendered_pages.append(output.getvalue())

        return rendered_pages


ocr_engine = OcrEngine()
