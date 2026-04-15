import { success } from "zod";

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
