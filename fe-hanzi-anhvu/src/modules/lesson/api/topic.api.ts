import { AdminBaseListResponse } from "@/shared/types/store.type";
import http from "@/shared/utils/http";

export type TopicSortBy =
    | "CreatedAt"
    | "UpdatedAt"
    | "TotalExercises"
    | "ExamYear"
    | "TotalExercises";
export type TopicType = "Learning" | "Exam";

export interface TopicCreateRequest {
    CourseId: string;
    Title: string;
    Description: string;
    TopicType: TopicType;
    EstimatedTimeMinutes: number;
    ExamYear?: number;
    ExamCode?: string;
}

export type TopicCreateResponseData = {
    TopicId: string;
};

export type TopicFormState = {
    title: string;
    description: string;
    topicType: TopicType;
    estimatedTimeMinutes: string;
    examYear: string;
    examCode: string;
};

export interface TopicListItemAdmin {
    id: string;
    title: string;
    orderIndex: number;
    topicType: string;
    examYear?: number | null;
    examCode?: string | null;
    isPublished: boolean;
    totalExercises: number;
    createdAt: string;
    updatedAt: string;
}

export interface CourseMetadataForTopicAdmin {
    id: string;
    title: string;
    hskLevel: number;
    slug: string;
    isPublished: boolean;
    totalTopics: number;
    totalStudentsEnrolled: number;
}

export interface CourseTopicsOverviewResponse {
    course: CourseMetadataForTopicAdmin | null;
    total: number;
    items: TopicListItemAdmin[];
    hasNextPage: boolean;
    nextCursor: string;
}

export interface TopicQueryParams {
    title?: string;
    topicType?: TopicType;
    startCreatedAt?: string;
    endCreatedAt?: string;
    isPublished?: boolean;
    sortBy?: TopicSortBy;
    orderByDescending?: boolean;
    take?: number;
    searchAfterValues?: string | null;
}

// #region API Endpoints
const endPoints = {
    createTopic: "lesson/v1/topic",
    listTopics: "search/v1/topics",
    courseTopics: "search/v1/course-topics",
};

export const topicApi = {
    async createTopic(payload: TopicCreateRequest) {
        return await http.post<TopicCreateResponseData>(
            endPoints.createTopic,
            payload,
        );
    },
    async getListTopics(
        courseId: string,
        queryParams: Partial<TopicQueryParams>,
    ) {
        return await http.get<AdminBaseListResponse<TopicListItemAdmin>>(
            endPoints.listTopics,
            {
                params: {
                    courseId,
                    ...queryParams,
                },
            },
        );
    },
    async getCourseTopicsOverview(
        courseId: string,
        queryParams: Partial<TopicQueryParams>,
    ) {
        return await http.get<CourseTopicsOverviewResponse>(
            endPoints.courseTopics,
            {
                params: {
                    courseId,
                    ...queryParams,
                },
            },
        );
    },
};
// #endregion
