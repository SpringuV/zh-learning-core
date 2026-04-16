import { courseApi } from "@/modules/lesson/api/course.api";
import {
    CourseCreateRequest,
    CourseListQueryParams,
    CourseReOrderRequest,
    UpdateCourseRequest,
} from "@/modules/lesson/types/coure.type";
import {
    useInfiniteQuery,
    useMutation,
    useQueryClient,
} from "@tanstack/react-query";

type CourseListBaseQueryParams = Omit<
    CourseListQueryParams,
    "searchAfterValues"
>;

const invalidateCourseQueries = (queryClient: ReturnType<typeof useQueryClient>) => {
    void queryClient.invalidateQueries({ queryKey: ["list-course"] });
};

export const useGetListCourse = (params: CourseListBaseQueryParams = {}) => {
    return useInfiniteQuery({
        queryKey: ["list-course", params],
        initialPageParam: undefined as string | undefined,
        queryFn: async ({ pageParam }) => {
            // pageParam chính là giá trị của searchAfterValues được truyền vào hàm getListCourse để lấy trang tiếp theo
            return await courseApi.getListCourse({
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

export const useCreateCourse = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: async (payload: CourseCreateRequest) => {
            const response = await courseApi.createCourse(payload);
            return response.data;
        },
        onSuccess: () => {
            // delay một chút để đảm bảo rằng dữ liệu đã được cập nhật trên server trước khi refetch
            setTimeout(() => {
                invalidateCourseQueries(queryClient);
            }, 1500);
        },
        onError: (err) => {
            console.error("Create course error:", err);
        },
    });
};

export const usePublishCourse = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (courseId: string) => {
            const response = await courseApi.publishCourse(courseId);
            return response.data;
        },
        onSuccess: () => {
            invalidateCourseQueries(queryClient);
        },
    });
};

export const useUnPublishCourse = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (courseId: string) => {
            const response = await courseApi.unPublishCourse(courseId);
            return response.data;
        },
        onSuccess: () => {
            invalidateCourseQueries(queryClient);
        },
    });
};

export const useReOrderCourse = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (payload: CourseReOrderRequest) => {
            const response = await courseApi.reOrderCourse(payload);
            return response.data;
        },
        onSuccess: () => {
            invalidateCourseQueries(queryClient);
        },
    });
};

export const useUpdateCourse = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (payload: UpdateCourseRequest) => {
            const response = await courseApi.updateCourse(payload);
            return response.data;
        },
        onSuccess: () => {
            invalidateCourseQueries(queryClient);
        },
    });
};

export const useDeleteCourse = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (courseId: string) => {
            const response = await courseApi.deleteCourse(courseId);
            return response.data;
        },
        onSuccess: () => {
            invalidateCourseQueries(queryClient);
        },
    });
};
