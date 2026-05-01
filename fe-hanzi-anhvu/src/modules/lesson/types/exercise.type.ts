import { TopicType } from "@/modules/lesson/types/topic.type";
import { PaginationInfo } from "@/shared/types/store.type";

export type ExerciseSortBy = "CreatedAt" | "UpdatedAt" | "OrderIndex";

export interface ExerciseListQueryParams {
    question?: string;
    isPublished?: boolean;
    skillType?: SkillType;
    exerciseType?: ExerciseType;
    difficulty?: ExerciseDifficulty;
    context?: ExerciseContext;
    // default
    take?: number;
    page?: number;
    startCreatedAt?: Date | string;
    endCreatedAt?: Date | string;
    sortBy?: ExerciseSortBy;
    orderByDescending?: boolean;
}
export type ExerciseType =
    | "ListenDialogueChoice"
    | "ListenImageChoice"
    | "ListenFillBlank"
    | "ListenSentenceJudge"
    | "ReadFillBlank"
    | "ReadComprehension"
    | "ReadSentenceOrder"
    | "ReadMatch"
    | "WriteHanzi"
    | "WritePinyin"
    | "WriteSentence";
export type SkillType = "Listening" | "Reading" | "Writing" | "Speaking";
export type ExerciseDifficulty = "Easy" | "Medium" | "Hard";
export type ExerciseContext = "Learning" | "Classroom" | "Mixed";
export interface ExerciseOption {
    id: string;
    text: string;
}

export interface ExerciseDetailResponse {
    exerciseId: string;
    question: string;
    exerciseType: ExerciseType;
    skillType: SkillType;
    difficulty: ExerciseDifficulty;
    context: ExerciseContext;
    description: string;
    options: ExerciseOption[];
    correctAnswer: string;
    isPublished: boolean;
    orderIndex: number;
    audioUrl?: string | null;
    imageUrl?: string | null;
    explanation?: string | null;
    slug: string;
    createdAt: string;
    updatedAt: string;
}

export interface ExerciseCreateRequest {
    topicId: string;
    question: string;
    description: string;
    exerciseType: ExerciseType;
    skillType: SkillType;
    exerciseContext?: ExerciseContext;
    difficulty: ExerciseDifficulty;
    correctAnswer: string; // có thể là id của option đúng hoặc text của đáp án đúng, tùy vào loại bài tập
    audioUrl?: string;
    imageUrl?: string;
    explanation?: string;
    options?: ExerciseOption[];
}

export interface ExerciseListItem {
    exerciseId: string;
    question: string;
    exerciseType: ExerciseType;
    orderIndex: number;
    context: ExerciseContext;
    skillType: SkillType;
    difficulty: ExerciseDifficulty;
    isPublished: boolean;
    createdAt: string;
    updatedAt: string;
}
export interface TopicMetadataForExerciseAdmin {
    id: string;
    title: string;
    estimatedTimeMinutes: number;
    isPublished: boolean;
    topicType: TopicType;
    examYear?: number | null;
    examCode?: string | null;
    slug: string;
    totalExercises: number;
}

export interface TopicExercisesOverviewResponse {
    parentMetadata: TopicMetadataForExerciseAdmin | null;
    items: ExerciseListItem[];
    pagination: PaginationInfo;
}

export interface ExerciseReorderRequest {
    topicId: string;
    orderedExerciseIds: string[];
}

export interface UpdateExerciseRequest {
    exerciseId: string;
    description?: string;
    question?: string;
    exerciseType?: ExerciseType;
    skillType?: SkillType;
    difficulty?: ExerciseDifficulty;
    exerciseContext?: ExerciseContext;
    correctAnswer?: string;
    audioUrl?: string;
    imageUrl?: string;
    explanation?: string;
    options?: ExerciseOption[];
}
