import {
    ExerciseCreateRequest,
    ExerciseDetailResponse,
    ExerciseListItem,
    ExerciseListQueryParams,
    ExerciseReorderRequest,
    TopicExercisesOverviewResponse,
    UpdateExerciseRequest,
} from "@/modules/lesson/types/exercise.type";
import {
    AdminBaseListResponse,
    sanitizeQueryParams,
} from "@/shared/types/store.type";
import http from "@/shared/utils/http";

//#region API Endpoints
const endPoints = {
    listExercise: "search/v1/exercises",
    createExercise: "lesson/v1/exercise",
    publishExercise: (exerciseId: string) =>
        `lesson/v1/exercise/${exerciseId}/publish`,
    unPublishExercise: (exerciseId: string) =>
        `lesson/v1/exercise/${exerciseId}/unpublish`,
    topicExercisesList: `search/v1/topic-exercises`,
    reorderExercises: `lesson/v1/exercise/reorder`,
    updateExercise: `lesson/v1/exercise`,
    deleteExercise: (exerciseId: string) => `lesson/v1/exercise/${exerciseId}`,
    detailExercise: (exerciseId: string) => `search/v1/exercises/${exerciseId}`,
};
//#endregion

export const exerciseApi = {
    async getTopicExercisesListOverview(
        topicId: string,
        payload: Partial<ExerciseListQueryParams> = {},
    ) {
        return await http.get<TopicExercisesOverviewResponse>(
            endPoints.topicExercisesList,
            {
                params: {
                    ...sanitizeQueryParams(payload),
                    topicId,
                },
            },
        );
    },
    async getListExercise(
        topicId: string,
        payload: Partial<ExerciseListQueryParams> = {},
    ) {
        return await http.get<AdminBaseListResponse<ExerciseListItem>>(
            endPoints.listExercise,
            {
                params: {
                    ...sanitizeQueryParams(payload),
                    topicId,
                },
            },
        );
    },
    async createExercise(payload: ExerciseCreateRequest) {
        return await http.post(endPoints.createExercise, payload);
    },
    async publishExercise(exerciseId: string) {
        return await http.post(endPoints.publishExercise(exerciseId));
    },
    async unPublishExercise(exerciseId: string) {
        return await http.post(endPoints.unPublishExercise(exerciseId));
    },
    async reorderExercises(payload: ExerciseReorderRequest) {
        return await http.post(endPoints.reorderExercises, payload);
    },
    async updateExercise(payload: UpdateExerciseRequest) {
        return await http.patch(endPoints.updateExercise, payload);
    },
    async deleteExercise(exerciseId: string) {
        return await http.delete(endPoints.deleteExercise(exerciseId));
    },
    async getExerciseDetail(exerciseId: string) {
        return await http.get<ExerciseDetailResponse>(
            endPoints.detailExercise(exerciseId),
        );
    },
};
