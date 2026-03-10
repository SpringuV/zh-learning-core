from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8", extra="ignore")

    app_name: str = "zh-learning-ocr-service"
    ocr_lang: str = Field(default="ch", alias="OCR_LANG")
    use_gpu: bool = Field(default=False, alias="PADDLE_OCR_USE_GPU")
    preprocess_enabled: bool = Field(default=True, alias="OCR_PREPROCESS")
    max_file_size_mb: int = Field(default=20, alias="MAX_FILE_SIZE_MB")


settings = Settings()
