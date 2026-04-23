import { exerciseApi } from "@/modules/lesson/api/exercise.api";
import {
    ExerciseCreateRequest,
    ExerciseListQueryParams,
    ExerciseReorderRequest,
    UpdateExerciseRequest,
} from "@/modules/lesson/types/exercise.type";
import {
    GcTime,
    StaleTime,
    TimeAwaitHandlerApi,
} from "@/shared/utils/contants";
import { wait } from "@/shared/utils/helper";
import {
    keepPreviousData,
    useMutation,
    useQuery,
    useQueryClient,
} from "@tanstack/react-query";

type ExerciseListBaseQueryParams = Partial<ExerciseListQueryParams>;

const invalidateExerciseQueries = (
    queryClient: ReturnType<typeof useQueryClient>,
) => {
    // Promise.all để thực hiện đồng thời việc invalidate nhiều query liên quan đến exercise, bao gồm cả danh sách bài tập và tổng quan bài tập theo chủ đề và khóa học
    // Invalidate tất cả các query liên quan đến exercise để đảm bảo dữ liệu luôn mới nhất sau khi có thay đổi
    return Promise.all([
        queryClient.invalidateQueries({ queryKey: ["list-exercises"] }),
        queryClient.invalidateQueries({
            queryKey: ["topic-exercises-overview"],
        }),
        queryClient.invalidateQueries({
            queryKey: ["course-topics-overview"],
        }),
    ]);
};

export const useGetExerciseDetail = (exerciseId: string) => {
    return useQuery({
        queryKey: ["exercise-detail", exerciseId],
        queryFn: async () => {
            return await exerciseApi.getExerciseDetail(exerciseId);
        },
        enabled: Boolean(exerciseId),
        refetchOnWindowFocus: false, // không tự động refetch khi cửa sổ được focus lại
        refetchOnMount: true,
        staleTime: StaleTime,
        gcTime: GcTime,
    });
};

export const useGetListExercises = (
    topicId: string,
    params: ExerciseListBaseQueryParams = {},
) => {
    return useQuery({
        queryKey: ["list-exercises", params, topicId],
        queryFn: async () => {
            return await exerciseApi.getListExercise(topicId, params);
        },
        placeholderData: keepPreviousData,
        staleTime: StaleTime,
        gcTime: GcTime,
        enabled: Boolean(topicId),
    });
};

export const useGetTopicExercisesOverview = (
    topicId: string,
    params: ExerciseListBaseQueryParams = {},
) => {
    return useQuery({
        queryKey: ["topic-exercises-overview", params, topicId],
        queryFn: async () => {
            return await exerciseApi.getTopicExercisesListOverview(
                topicId,
                params,
            );
        },
        // Giữ lại dữ liệu trang trước trong lúc tải trang mới để tránh giật/flicker UI.
        placeholderData: keepPreviousData,
        staleTime: StaleTime,
        gcTime: GcTime,
        enabled: Boolean(topicId),
    });
};

export const useCreateExercise = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: async (payload: ExerciseCreateRequest) => {
            const response = await exerciseApi.createExercise(payload);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateExerciseQueries(queryClient);
        },
    });
};

export const usePublishExercise = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (exerciseId: string) => {
            const response = await exerciseApi.publishExercise(exerciseId);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateExerciseQueries(queryClient);
        },
    });
};

export const useUnPublishExercise = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (exerciseId: string) => {
            const response = await exerciseApi.unPublishExercise(exerciseId);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateExerciseQueries(queryClient);
        },
    });
};

export const useReOrderExercise = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (payload: ExerciseReorderRequest) => {
            const response = await exerciseApi.reorderExercises(payload);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateExerciseQueries(queryClient);
        },
    });
};

export const useUpdateExercise = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (payload: UpdateExerciseRequest) => {
            const response = await exerciseApi.updateExercise(payload);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateExerciseQueries(queryClient);
        },
    });
};

export const useDeleteExercise = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (exerciseId: string) => {
            const response = await exerciseApi.deleteExercise(exerciseId);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateExerciseQueries(queryClient);
        },
    });
};
