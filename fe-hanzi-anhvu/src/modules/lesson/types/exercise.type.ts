import { TopicType } from "@/modules/lesson/types/topic.type";

export type ExerciseSortBy = "CreatedAt" | "UpdatedAt" | "OrderIndex";

export interface ExerciseListQueryParams {
    question?: string;
    isPublished?: boolean;
    skillType?: SkillType;
    exerciseType?: ExerciseType;
    difficulty?: string;
    context?: string;
    // default
    take?: number;
    startCreatedAt?: Date | string;
    endCreatedAt?: Date | string;
    searchAfterValues?: string | null;
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
    options?: {
        id: string;
        text: string;
    }[];
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
    topicType: TopicType;
    examYear?: number | null;
    examCode?: string | null;
    slug: string;
    totalExercises: number;
}

export interface TopicExercisesOverviewResponse {
    parentMetadata: TopicMetadataForExerciseAdmin | null;
    total: number;
    items: ExerciseListItem[];
    hasNextPage: boolean;
    nextCursor: string;
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
    isPublished?: boolean;
    audioUrl?: string;
    imageUrl?: string;
    explanation?: string;
    options?: {
        id: string;
        text: string;
    }[];
}
