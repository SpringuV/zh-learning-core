export type UiStatus = "idle" | "loading" | "success" | "error" | "warning";

export interface PaginationInfo {
    page: number;
    pageSize: number;
    total: number;
}

export interface BaseResponse<T = void> {
    // Generic type T, default to void if not provided
    success: boolean;
    message: string;
    data?: T;
    errorCode?: number;
}

// factory functions to create success and error responses
export const BaseResponse = {
    success: <T>(data: T, message = "Success"): BaseResponse<T> => ({
        success: true,
        message,
        data: data,
    }),
    error: (message: string, errorCode?: number): BaseResponse => ({
        success: false,
        message,
        errorCode: errorCode,
    }),
};

export interface AdminBaseListResponse<T> {
    items: T[];
    pagination: PaginationInfo;
}
// Generic type T, default to void if not provided
export type ParamTypes<T extends Record<string, unknown>> = Partial<T>;

export const sanitizeQueryParams = <T extends Record<string, unknown>>(
    params: ParamTypes<T>,
): ParamTypes<T> => {
    return Object.fromEntries(
        Object.entries(params).filter(
            // mảng các cặp [key, value] của object params, sau đó lọc bỏ những cặp có value là undefined hoặc null,
            ([, value]) =>
                // === là lấy cả giá trị và kiểu dữ liệu, nên sẽ giữ lại nếu value là false hoặc 0,
                value === false || value === 0 ? true : Boolean(value),
            // vì chúng có thể là giá trị hợp lệ mà người dùng muốn tìm kiếm,
            //  ví dụ: isActive=false hoặc currentLevel=0
        ),
    ) as ParamTypes<T>;
};
