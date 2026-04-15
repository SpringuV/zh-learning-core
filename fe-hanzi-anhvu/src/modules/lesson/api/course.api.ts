import {
    CourseCreateRequest,
    CourseCreateResponseApi,
} from "@/modules/lesson/types/coure.type";
import { AdminBaseListResponse } from "@/shared/types/store.type";
import http from "@/shared/utils/http";

export type CourseSortBy =
    | "CreatedAt"
    | "UpdatedAt"
    | "HskLevel"
    | "Title"
    | "OrderIndex"
    | "TotalStudentsEnrolled"
    | "TotalTopics";

export interface CourseListQueryParams {
    title?: string;
    take?: number;
    startCreatedAt?: Date | string;
    endCreatedAt?: Date | string;
    searchAfterValues?: string | null;
    sortBy?: CourseSortBy;
    orderByDescending?: boolean;
}

export interface CourseListItem {
    id: string;
    title: string;
    hskLevel: number;
    orderIndex: number;
    totalTopics: number;
    totalStudentsEnrolled: number;
    isPublished: boolean;
    slug: string;
    createdAt: string;
    updatedAt: string;
}

function sanitizeQueryParams(params: CourseListQueryParams) {
    return Object.fromEntries(
        Object.entries(params).filter(
            ([, value]) =>
                value === false || value === 0 ? true : Boolean(value), // giữ lại nếu value là false hoặc 0,
            // vì chúng có thể là giá trị hợp lệ mà người dùng muốn tìm kiếm,
            //  ví dụ: isActive=false hoặc currentLevel=0
        ),
    );
}

// #region API Endpoints
const endPoints = {
    createCourse: "lesson/v1/course",
    listCourse: "search/v1/courses",
};
// #endregion

export const courseApi = {
    async createCourse(payload: CourseCreateRequest) {
        return await http.post<CourseCreateResponseApi>(
            endPoints.createCourse,
            payload,
        );
    },
    async getListCourse(payload: CourseListQueryParams = {}) {
        return await http.get<AdminBaseListResponse<CourseListItem>>(
            endPoints.listCourse,
            {
                params: sanitizeQueryParams(payload),
            },
        );
    },
};
