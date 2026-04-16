import { topicApi } from "@/modules/lesson/api/topic.api";
import {
    CourseTopicsOverviewResponse,
    TopicCreateRequest,
    TopicQueryParams,
    TopicReOrderRequest,
    UpdateTopicRequest,
} from "@/modules/lesson/types/topic.type";
import {
    useInfiniteQuery,
    useMutation,
    useQuery,
    useQueryClient,
} from "@tanstack/react-query";

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

export const useGetTopicDetail = (topicId: string) => {
    return useQuery({
        queryKey: ["topic-detail", topicId],
        queryFn: async () => {
            return await topicApi.getTopicDetail(topicId);
        },
        enabled: Boolean(topicId),
        refetchOnWindowFocus: false, // không tự động refetch khi cửa sổ được focus lại
        refetchOnMount: true,
        staleTime: 5 * 60 * 1000, // 5 phút
        gcTime: 10 * 60 * 1000, // 10 phút
    });
};

export const useGetListTopics = (
    courseId: string,
    params: TopicQueryParams = {},
) => {
    return useInfiniteQuery({
        queryKey: ["list-topics", params, courseId],
        initialPageParam: undefined as string | undefined,
        queryFn: async ({ pageParam }) => {
            // pageParam chính là giá trị của searchAfterValues được truyền vào hàm getListCourse để lấy trang tiếp theo
            return await topicApi.getListTopics(courseId!, {
                ...params,
                searchAfterValues: pageParam,
            });
        },
        refetchOnMount: true, // luôn refetch khi component mount để đảm bảo dữ liệu mới nhất, đặc biệt sau khi có thay đổi về chủ đề
        refetchOnWindowFocus: false, // không tự động refetch khi cửa sổ được focus lại,
        // nó sẽ chỉ refetch khi người dùng cuộn đến cuối danh sách hoặc gọi hàm refetch thủ công
        staleTime: 5 * 60 * 1000, // 5 phút
        gcTime: 10 * 60 * 1000, // 10 phút
        getNextPageParam: (lastPage) => {
            const payload = lastPage.data;
            if (!payload.hasNextPage || !payload.nextCursor) {
                return undefined;
            }
            return payload.nextCursor;
        },
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
    return useInfiniteQuery({
        queryKey: ["course-topics-overview", params, courseId],
        initialPageParam: undefined as string | undefined,
        queryFn: async ({ pageParam }) => {
            return await topicApi.getCourseTopicsOverview(courseId, {
                ...params,
                searchAfterValues: pageParam,
            });
        },
        staleTime: 5 * 60 * 1000,
        gcTime: 10 * 60 * 1000,
        refetchOnWindowFocus: false,
        refetchOnMount: true,
        enabled: Boolean(courseId),
        getNextPageParam: (lastPage) => {
            const payload: CourseTopicsOverviewResponse = lastPage.data;
            if (!payload.hasNextPage || !payload.nextCursor) {
                return undefined;
            }
            return payload.nextCursor;
        },
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
            await invalidateTopicQueries(queryClient);
        },
    });
};
