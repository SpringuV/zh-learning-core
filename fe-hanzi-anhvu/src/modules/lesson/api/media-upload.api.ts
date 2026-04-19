import http from "@/shared/utils/http";

type MediaFolder = "images" | "audio";

type SignUploadUrlRequest = {
    fileName: string;
    contentType: string;
    folder: MediaFolder;
    expirySeconds?: number;
};

type SignUploadUrlResponse = {
    uploadUrl: string;
    httpMethod: string;
    objectKey: string;
    publicUrl: string;
    expiresAtUtc: string;
};

type SignUploadUrlEnvelope = {
    success: boolean;
    data?: SignUploadUrlResponse;
    message?: string;
    errorCode?: string;
};

const endpoint = "/media/v1/uploads/sign";

const parseErrorMessage = (error: unknown): string => {
    if (typeof error === "object" && error !== null) {
        const maybeApiError = error as {
            response?: {
                data?: {
                    message?: string;
                    detail?: string;
                    title?: string;
                    errorCode?: string;
                };
            };
            message?: string;
        };

        return (
            maybeApiError.response?.data?.message ??
            maybeApiError.response?.data?.detail ??
            maybeApiError.response?.data?.title ??
            maybeApiError.response?.data?.errorCode ??
            maybeApiError.message ??
            "Không thể upload media."
        );
    }

    return "Không thể upload media.";
};

const createSignedUploadUrl = async (payload: SignUploadUrlRequest) => {
    const response = await http.post<SignUploadUrlEnvelope>(endpoint, {
        fileName: payload.fileName,
        contentType: payload.contentType,
        folder: payload.folder,
        expirySeconds: payload.expirySeconds,
    });

    if (!response.data.success || !response.data.data) {
        throw new Error(response.data.message || "Không thể tạo signed URL.");
    }

    return response.data.data;
};

const uploadFileToSignedUrl = async (
    signed: SignUploadUrlResponse,
    file: File,
) => {
    const uploadResponse = await fetch(signed.uploadUrl, {
        method: signed.httpMethod || "PUT",
        headers: {
            "Content-Type": file.type,
        },
        body: file,
    });

    if (!uploadResponse.ok) {
        throw new Error(
            `Upload thất bại (${uploadResponse.status} ${uploadResponse.statusText}).`,
        );
    }
};

export const mediaUploadApi = {
    async uploadToR2(file: File, folder: MediaFolder) {
        if (!file.name) {
            throw new Error("FileName không hợp lệ.");
        }

        if (!file.type) {
            throw new Error("ContentType không hợp lệ.");
        }

        const signed = await createSignedUploadUrl({
            fileName: file.name,
            contentType: file.type,
            folder,
        });

        await uploadFileToSignedUrl(signed, file);

        return {
            publicUrl: signed.publicUrl,
            objectKey: signed.objectKey,
        };
    },
    parseErrorMessage,
};
