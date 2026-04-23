import {
    CourseCreateRequest,
    CourseCreateResponseApi,
    CourseDashboardItem,
    CourseListItem,
    CourseListQueryParams,
    CourseReOrderRequest,
    UpdateCourseRequest,
} from "@/modules/lesson/types/coure.type";
import {
    AdminBaseListResponse,
    BaseResponse,
    sanitizeQueryParams,
} from "@/shared/types/store.type";
import http from "@/shared/utils/http";

// #region API Endpoints
const endPoints = {
    // admin course management
    createCourse: "lesson/v1/course",
    publishCourse: (courseId: string) => `lesson/v1/course/${courseId}/publish`,
    unPublishCourse: (courseId: string) =>
        `lesson/v1/course/${courseId}/unpublish`,
    listCourse: "search/v1/courses",
    updateCourse: `lesson/v1/course`,
    reOrderCourse: "lesson/v1/course/reorder",
    deleteCourse: (courseId: string) => `lesson/v1/course/${courseId}`,
    // client course
    loadCoursesForDashboard: "search/v1/courses/dashboard",
};
// #endregion

export const courseApi = {
    async createCourse(payload: CourseCreateRequest) {
        return await http.post<CourseCreateResponseApi>(
            endPoints.createCourse,
            payload,
        );
    },
    async getListCourse(payload: Partial<CourseListQueryParams> = {}) {
        return await http.get<AdminBaseListResponse<CourseListItem>>(
            endPoints.listCourse,
            {
                params: sanitizeQueryParams(payload),
            },
        );
    },
    async publishCourse(courseId: string) {
        return await http.post(endPoints.publishCourse(courseId));
    },
    async unPublishCourse(courseId: string) {
        return await http.post(endPoints.unPublishCourse(courseId));
    },
    async reOrderCourse(payload: CourseReOrderRequest) {
        return await http.post(endPoints.reOrderCourse, payload);
    },
    async updateCourse(payload: UpdateCourseRequest) {
        return await http.patch(endPoints.updateCourse, payload);
    },
    async deleteCourse(courseId: string) {
        return await http.delete(endPoints.deleteCourse(courseId));
    },
    async loadCoursesForDashboard() {
        return await http.get<BaseResponse<CourseDashboardItem[]>>(
            endPoints.loadCoursesForDashboard,
        );
    },
};
