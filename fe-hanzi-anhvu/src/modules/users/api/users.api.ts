import http from "@/shared/utils/http";

const endpoints = {
    getListUsers: "/search/v1/users",
};

export type UserSortBy = "CreatedAt" | "UpdatedAt" | "CurrentLevel";

export interface UserListQueryParams {
    email?: string;
    username?: string;
    isActive?: boolean;
    phoneNumber?: string;
    take?: number;
    searchAfterCreatedAt?: string;
    sortBy?: UserSortBy;
    orderByDescending?: boolean;
    startCreatedAt?: string;
    endCreatedAt?: string;
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
    total: number;
    items: UserListItem[];
    hasNextPage: boolean;
    nextCursor: string;
}

function sanitizeQueryParams(params: UserListQueryParams) {
    return Object.fromEntries(
        Object.entries(params).filter(([, value]) =>
            value === false || value === 0 ? true : Boolean(value),
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
