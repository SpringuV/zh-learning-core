export type UiStatus = "idle" | "loading" | "success" | "error" | "warning";

export interface AdminBaseListResponse<T> {
    total: number;
    items: T[];
    hasNextPage: boolean;
    nextCursor: string;
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
