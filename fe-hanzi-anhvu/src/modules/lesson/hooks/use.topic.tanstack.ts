import { topicApi } from "@/modules/lesson/api/topic.api";
import {
    CountinueLearningSessionResponse,
    StartLearningTopicResponse,
    TopicCreateRequest,
    TopicQueryParams,
    TopicReOrderRequest,
    UpdateTopicRequest,
} from "@/modules/lesson/types/topic.type";
import {
    GcTime,
    StaleTime,
    TimeAwaitHandlerApi,
} from "@/shared/utils/contants";
import { wait } from "@/shared/utils/helper";
import { BaseResponse } from "@/shared/types/store.type";
import {
    keepPreviousData,
    useMutation,
    useQuery,
    useQueryClient,
} from "@tanstack/react-query";

// cache key, from component A -> component B without props drilling, we can use this key to get data in cache and pass to component B, avoid refetching data again
export const startLearningTopicQueryKey = (slugTopic: string) =>
    ["start-learning-topic", slugTopic] as const;

// #region Invalidate topic queries
const invalidateTopicQueries = (
    queryClient: ReturnType<typeof useQueryClient>,
) => {
    return Promise.all([
        queryClient.invalidateQueries({ queryKey: ["list-topics"] }),
        queryClient.invalidateQueries({
            queryKey: ["course-topics-overview"],
        }),
        queryClient.invalidateQueries({
            queryKey: ["topic-detail"],
        }),
    ]);
};
// #endregion

// #region CountinueLearningTopic
export const useCountinueLearningTopic = (slugTopic: string) => {
    return useQuery({
        queryKey: ["countinue-learning-topic", slugTopic],
        queryFn: async (): Promise<
            BaseResponse<CountinueLearningSessionResponse>
        > => {
            const res =
                await topicApi.continueLearningSessionForTopicClient(slugTopic);
            return res.data;
        },
        enabled: Boolean(slugTopic),
        refetchOnWindowFocus: false, // không tự động refetch khi cửa sổ được focus lại
        refetchOnMount: true,
        staleTime: StaleTime,
        gcTime: GcTime,
    });
};
// #endregion

// #region StartLearningTopic
export const useStartLearningTopicMutation = () => {
    return useMutation({
        mutationFn: async (
            slugTopic: string,
        ): Promise<BaseResponse<StartLearningTopicResponse>> => {
            const response = await topicApi.startLearningTopic(slugTopic);
            return response.data;
        },
        onError: (err) => {
            console.error("Start learning topic error:", err);
        },
    });
};
// #endregion
// #region TopicsForClient
export const useGetTopicsForClient = (slug: string) => {
    return useQuery({
        queryKey: ["topics-for-client", slug],
        queryFn: async () => {
            return await topicApi.getTopicsForClient(slug);
        },
        refetchOnWindowFocus: false,
        refetchOnMount: true,
        staleTime: StaleTime,
        gcTime: GcTime,
        enabled: Boolean(slug), // Chỉ chạy query khi slug có giá trị hợp lệ
    });
};
// #endregion
export const useGetTopicDetail = (topicId: string) => {
    return useQuery({
        queryKey: ["topic-detail", topicId],
        queryFn: async () => {
            return await topicApi.getTopicDetail(topicId);
        },
        enabled: Boolean(topicId),
        refetchOnWindowFocus: false, // không tự động refetch khi cửa sổ được focus lại
        refetchOnMount: true,
        staleTime: StaleTime,
        gcTime: GcTime,
    });
};

export const useGetListTopics = (
    courseId: string,
    params: TopicQueryParams = {},
) => {
    return useQuery({
        queryKey: ["list-topics", params, courseId],
        queryFn: async () => {
            return await topicApi.getListTopics(courseId!, params);
        },
        placeholderData: keepPreviousData,
        refetchOnMount: true, // luôn refetch khi component mount để đảm bảo dữ liệu mới nhất, đặc biệt sau khi có thay đổi về chủ đề
        refetchOnWindowFocus: false, // không tự động refetch khi cửa sổ được focus lại,
        // nó sẽ chỉ refetch khi người dùng cuộn đến cuối danh sách hoặc gọi hàm refetch thủ công
        staleTime: StaleTime,
        gcTime: GcTime,
        enabled: Boolean(courseId),
    });
};

export const useCreateTopic = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: async (payload: TopicCreateRequest) => {
            const response = await topicApi.createTopic(payload);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateTopicQueries(queryClient);
        },
        onError: (err) => {
            console.error("Create topic error:", err);
        },
    });
};

export const useGetCourseTopicsOverview = (
    courseId: string,
    params: TopicQueryParams = {},
) => {
    return useQuery({
        queryKey: ["course-topics-overview", params, courseId],
        queryFn: async () => {
            return await topicApi.getCourseTopicsOverview(courseId, params);
        },
        placeholderData: keepPreviousData,
        staleTime: StaleTime,
        gcTime: GcTime,
        refetchOnWindowFocus: false,
        refetchOnMount: true,
        enabled: Boolean(courseId),
    });
};

export const usePublishTopic = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (topicId: string) => {
            const response = await topicApi.publishTopic(topicId);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateTopicQueries(queryClient);
        },
    });
};

export const useUnPublishTopic = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (topicId: string) => {
            const response = await topicApi.unPublishTopic(topicId);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateTopicQueries(queryClient);
        },
    });
};

export const useReOrderTopic = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (payload: TopicReOrderRequest) => {
            const response = await topicApi.reOrderTopic(payload);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateTopicQueries(queryClient);
        },
    });
};

export const useUpdateTopic = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (payload: UpdateTopicRequest) => {
            const response = await topicApi.updateTopic(payload);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateTopicQueries(queryClient);
        },
    });
};

export const useDeleteTopic = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (topicId: string) => {
            const response = await topicApi.deleteTopic(topicId);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateTopicQueries(queryClient);
        },
    });
};
