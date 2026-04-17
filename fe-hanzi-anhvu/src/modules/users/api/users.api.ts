import http from "@/shared/utils/http";
import { PaginationInfo } from "@/shared/types/store.type";

const endpoints = {
    getListUsers: "/search/v1/users",
};

export type UserSortBy =
    | "CreatedAt"
    | "UpdatedAt"
    | "CurrentLevel"
    | "Email"
    | "Username";

export type SortDirection = "Asc" | "Desc";

export interface UserListQueryParams {
    email?: string;
    username?: string;
    isActive?: boolean;
    phoneNumber?: string;
    take?: number;
    page?: number;
    sortBy?: UserSortBy;
    orderByDescending?: boolean;
}

export interface UserListItem {
    id: string;
    email: string;
    username: string;
    phoneNumber: string | null;
    isActive: boolean;
    createdAt: string;
    updatedAt: string;
    currentLevel: number;
}

export interface UserListResponse {
    items: UserListItem[];
    pagination: PaginationInfo;
}

function sanitizeQueryParams(params: UserListQueryParams) {
    return Object.fromEntries(
        Object.entries(params).filter(
            ([, value]) =>
                value === false || value === 0 ? true : Boolean(value), // giữ lại nếu value là false hoặc 0,
            // vì chúng có thể là giá trị hợp lệ mà người dùng muốn tìm kiếm,
            //  ví dụ: isActive=false hoặc currentLevel=0
        ),
    );
}

export const usersApi = {
    async getListUsers(params: UserListQueryParams = {}) {
        const response = await http.get<UserListResponse>(
            endpoints.getListUsers,
            {
                params: sanitizeQueryParams(params),
            },
        );

        return response;
    },
};
