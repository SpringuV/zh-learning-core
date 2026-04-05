import { AxiosError } from "axios";
import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs));
}

export const getErrorMessage = (error: unknown): string => {
    if (error instanceof AxiosError) {
        const message = (error.response?.data as any)?.message;
        if (message) return message;

        if (error.response?.status === 404) {
            return "Không tìm thấy dữ liệu người dùng";
        }
        if (error.response?.status === 403) {
            return "Bạn không có quyền truy cập";
        }
        if (error.response?.status === 500) {
            return "Lỗi máy chủ. Vui lòng thử lại sau";
        }
        return error.message || "Lỗi kết nối";
    }
    if (error instanceof Error) {
        return error.message;
    }
    return "Có lỗi xảy ra. Vui lòng thử lại";
};
