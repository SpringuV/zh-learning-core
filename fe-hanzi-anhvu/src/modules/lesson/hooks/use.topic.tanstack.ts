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
    useQueryClient,
} from "@tanstack/react-query";

const invalidateTopicQueries = (queryClient: ReturnType<typeof useQueryClient>) => {
    void queryClient.invalidateQueries({ queryKey: ["list-topics"] });
    void queryClient.invalidateQueries({ queryKey: ["course-topics-overview"] });
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
        staleTime: 5 * 60 * 1000, // 5 phút
        gcTime: 10 * 60 * 1000, // 10 phút
        refetchOnWindowFocus: false, // không tự động refetch khi cửa sổ được focus lại,
        // nó sẽ chỉ refetch khi người dùng cuộn đến cuối danh sách hoặc gọi hàm refetch thủ công
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
        onSuccess: () => {
            // delay một chút để đảm bảo rằng dữ liệu đã được cập nhật trên server trước khi refetch
            setTimeout(() => {
                invalidateTopicQueries(queryClient);
            }, 1500);
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
        onSuccess: () => {
            invalidateTopicQueries(queryClient);
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
        onSuccess: () => {
            invalidateTopicQueries(queryClient);
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
        onSuccess: () => {
            invalidateTopicQueries(queryClient);
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
        onSuccess: () => {
            invalidateTopicQueries(queryClient);
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
        onSuccess: () => {
            invalidateTopicQueries(queryClient);
        },
    });
};
