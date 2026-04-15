import {
    CourseTopicsOverviewResponse,
    topicApi,
    TopicCreateRequest,
    TopicQueryParams,
} from "@/modules/lesson/api/topic.api";
import {
    useInfiniteQuery,
    useMutation,
    useQueryClient,
} from "@tanstack/react-query";

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
        onSuccess: (_) => {
            // delay một chút để đảm bảo rằng dữ liệu đã được cập nhật trên server trước khi refetch
            setTimeout(() => {
                queryClient.invalidateQueries({ queryKey: ["list-topics"] });
                queryClient.invalidateQueries({
                    queryKey: ["course-topics-overview"],
                });
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
