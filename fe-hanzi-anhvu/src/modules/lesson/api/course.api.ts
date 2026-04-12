import {
    CourseCreateRequest,
    CourseCreateResponseApi,
} from "@/modules/lesson/types/coure.type";
import http from "@/shared/utils/http";
import { create } from "domain";

const endPoints = {
    createCourse: "/course/create",
};

export const courseApi = {
    async createCourse(payload: CourseCreateRequest) {
        return await http.post<CourseCreateResponseApi>(
            endPoints.createCourse,
            payload,
        );
    },
};
