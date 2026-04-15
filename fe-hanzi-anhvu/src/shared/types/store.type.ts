export type UiStatus = "idle" | "loading" | "success" | "error" | "warning";

export interface AdminBaseListResponse<T> {
    total: number;
    items: T[];
    hasNextPage: boolean;
    nextCursor: string;
}
