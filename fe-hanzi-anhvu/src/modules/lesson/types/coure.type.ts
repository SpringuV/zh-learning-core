/*
Tính năng	            interface	            type
Extend	            ✅ Dùng extends	    ✅ Dùng & (intersection)
Merge declarations	✅ Tự động	        ❌ Không được
Union types	        ❌ Không	            ✅ Được
Primitives	        ❌ Không	            ✅ Được (e.g., type ID = string)
*/
interface BaseResponse<T = void> {
    // Generic type T, default to void if not provided
    success: boolean;
    message: string;
    Data?: T;
    ErrorCode?: number;
}

// factory functions to create success and error responses
const BaseResponse = {
    success: <T>(data: T, message = "Success"): BaseResponse<T> => ({
        success: true,
        message,
        Data: data,
    }),
    error: (message: string, errorCode?: number): BaseResponse => ({
        success: false,
        message,
        ErrorCode: errorCode,
    }),
};

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
    take?: number;
    page?: number;
    startCreatedAt?: Date | string;
    endCreatedAt?: Date | string;
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

export interface CourseReOrderRequest {
    orderedCourseIds: string[];
}
export interface UpdateCourseRequest {
    courseId: string;
    title?: string;
    description?: string;
    hskLevel?: number;
}
