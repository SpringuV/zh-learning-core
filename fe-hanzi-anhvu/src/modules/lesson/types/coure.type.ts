import { BaseResponse } from "@/shared/types/store.type";

/*
Tính năng	            interface	            type
Extend	            ✅ Dùng extends	    ✅ Dùng & (intersection)
Merge declarations	✅ Tự động	        ❌ Không được
Union types	        ❌ Không	            ✅ Được
Primitives	        ❌ Không	            ✅ Được (e.g., type ID = string)
*/
export interface CourseCreateRequest {
    Title: string;
    Description: string;
    HskLevel: number;
}
interface CourseCreateResponse {
    Id: string;
}
export type CourseCreateResponseApi = BaseResponse<CourseCreateResponse>;

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
    isPublished?: boolean;
    hskLevel?: number;
    take?: number;
    page?: number;
    startCreatedAt?: Date | string;
    endCreatedAt?: Date | string;
    sortBy?: CourseSortBy;
    orderByDescending?: boolean;
}
export interface CourseDashboardItem {
    id: string;
    title: string;
    description: string;
    hskLevel: number;
    orderIndex: number;
    totalTopics: number;
    totalStudentsEnrolled: number;
    slug: string;
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

export interface CourseReOrderRequest {
    orderedCourseIds: string[];
}
export interface UpdateCourseRequest {
    courseId: string;
    title?: string;
    description?: string;
    hskLevel?: number;
}
