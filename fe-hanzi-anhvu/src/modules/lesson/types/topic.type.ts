import { PaginationInfo } from "@/shared/types/store.type";

export type TopicSortBy =
    | "CreatedAt"
    | "UpdatedAt"
    | "TotalExercises"
    | "ExamYear"
    | "OrderIndex";
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

export interface UpdateTopicRequest {
    topicId: string;
    title?: string;
    description?: string;
    topicType?: TopicType;
    estimatedTimeMinutes?: number;
    examYear?: number;
    examCode?: string;
}

export type TopicCreateResponseData = {
    topicId: string;
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

export interface TopicDetailResponse {
    id: string;
    title: string;
    slug: string;
    orderIndex: number;
    topicType: TopicType;
    examYear?: number | null;
    examCode?: string | null;
    estimatedTimeMinutes: number;
    description: string;
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
    parentMetadata: CourseMetadataForTopicAdmin | null;
    items: TopicListItemAdmin[];
    pagination: PaginationInfo;
}
export interface TopicReOrderRequest {
    courseId: string;
    orderedTopicIds: string[];
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
    page?: number;
}
