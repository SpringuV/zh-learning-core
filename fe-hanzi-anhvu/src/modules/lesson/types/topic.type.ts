import {
    ExerciseDifficulty,
    ExerciseOption,
    ExerciseType,
    SkillType,
} from "@/modules/lesson/types/exercise.type";
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

// #region Start Or Countinue
export interface LearningExerciseSessionPracticeDTOResponse {
    exerciseId: string;
    question: string;
    skillType: SkillType;
    difficulty: ExerciseDifficulty;
    description: string;
    answerd: string | null; // đáp án đã lưu của người dùng cho bài tập này trong session, nếu có
    options: ExerciseOption[];
    audioUrl?: string | null;
    imageUrl?: string | null;
    explanation?: string | null;
}

export interface LearningExerciseSessionItemDTOResponse {
    sessionItemId: string;
    exerciseId: string;
    attemptId: number;
    sequenceNo: number;
    orderIndex: number;
    status: string;
    viewedAt: string;
    answeredAt?: string;
}

interface LearningTopicResponse {
    sessionId: string;
    totalExercises: number;
    currentSequenceNo: number;
    sessionItems: LearningExerciseSessionItemDTOResponse[];
    firstExercise: LearningExerciseSessionPracticeDTOResponse;
}

interface StartLearningExerciseDTOResponse extends LearningExerciseSessionPracticeDTOResponse {}

interface StartLearningSessionItemDTOResponse extends LearningExerciseSessionItemDTOResponse {}
export interface StartLearningTopicResponse extends Omit<
    LearningTopicResponse,
    "firstExercise" | "sessionItems"
> {
    sessionItems: StartLearningSessionItemDTOResponse[];
    firstExercise: StartLearningExerciseDTOResponse;
}

// extend StartLearningTopicResponse since the response from backend for continue learning is the same as start learning, just without topicProgressId
export interface CountinueLearningSessionItemResponse extends LearningExerciseSessionItemDTOResponse {}
export interface CountinueLearningExerciseDTOResponse extends LearningExerciseSessionPracticeDTOResponse {}
// chỉ override sessionItems và firstExercise, còn lại giữ nguyên giống StartLearningTopicResponse
export interface CountinueLearningSessionResponse extends Omit<
    LearningTopicResponse,
    "sessionItems" | "firstExercise"
> {
    sessionItems: CountinueLearningSessionItemResponse[];
    firstExercise: CountinueLearningExerciseDTOResponse;
}

export interface ExerciseSessionItemsSnapshotItemsResponse extends Omit<
    LearningTopicResponse,
    "firstExercise"
> {}

export interface ResultCompleteSessionOverviewResponse {
    sessionId: string;
    userId: string;
    totalExercises: number;
    totalScore: number;
    totalCorrect: number;
    totalWrong: number;
    scoreListening: number;
    scoreReading: number;
    timeSpentSeconds: number;
    completedAt: string;
}
export interface CompleteLearningSessionResponse extends ResultCompleteSessionOverviewResponse {}

// #endregion
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

export interface TopicClientDashboardItemResponse {
    id: string;
    title: string;
    slug: string;
    topicType: TopicType;
    examYear?: number | null;
    examCode?: string | null;
    estimatedTimeMinutes: number;
    description: string;
    totalExercises: number;
    orderIndex: number;
    status: "NotStarted" | "Abandoned" | "InProgress" | "Completed";
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
