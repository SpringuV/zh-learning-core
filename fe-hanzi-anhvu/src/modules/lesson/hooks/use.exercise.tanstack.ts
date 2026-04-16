import { exerciseApi } from "@/modules/lesson/api/exercise.api";
import {
    ExerciseCreateRequest,
    ExerciseListQueryParams,
    ExerciseReorderRequest,
    UpdateExerciseRequest,
} from "@/modules/lesson/types/exercise.type";
import { TimeAwaitHandlerApi } from "@/shared/utils/contants";
import { wait } from "@/shared/utils/helper";
import {
    useInfiniteQuery,
    useMutation,
    useQuery,
    useQueryClient,
} from "@tanstack/react-query";

type ExerciseListBaseQueryParams = Omit<
    ExerciseListQueryParams,
    "searchAfterValues"
>;

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
        staleTime: 5 * 60 * 1000, // 5 phút
        gcTime: 10 * 60 * 1000, // 10 phút
    });
};

export const useGetListExercises = (
    topicId: string,
    params: ExerciseListBaseQueryParams = {},
) => {
    return useInfiniteQuery({
        queryKey: ["list-exercises", params, topicId],
        initialPageParam: undefined as string | undefined,
        queryFn: async ({ pageParam }) => {
            return await exerciseApi.getListExercise(topicId, {
                ...params,
                searchAfterValues: pageParam,
            });
        },
        staleTime: 5 * 60 * 1000,
        gcTime: 10 * 60 * 1000,
        enabled: Boolean(topicId),
        getNextPageParam: (lastPage) => {
            const payload = lastPage.data;
            if (!payload.hasNextPage || !payload.nextCursor) {
                return undefined;
            }
            return payload.nextCursor;
        },
    });
};

export const useGetTopicExercisesOverview = (
    topicId: string,
    params: ExerciseListBaseQueryParams = {},
) => {
    return useInfiniteQuery({
        queryKey: ["topic-exercises-overview", params, topicId],
        initialPageParam: undefined as string | undefined,
        queryFn: async ({ pageParam }) => {
            return await exerciseApi.getTopicExercisesListOverview(topicId, {
                ...params,
                searchAfterValues: pageParam,
            });
        },
        staleTime: 5 * 60 * 1000,
        gcTime: 10 * 60 * 1000,
        enabled: Boolean(topicId),
        getNextPageParam: (lastPage) => {
            const payload = lastPage.data;
            if (!payload.hasNextPage || !payload.nextCursor) {
                return undefined;
            }
            return payload.nextCursor;
        },
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
