import { courseApi } from "@/modules/lesson/api/course.api";
import {
    CourseCreateRequest,
    CourseListQueryParams,
    CourseReOrderRequest,
    UpdateCourseRequest,
} from "@/modules/lesson/types/coure.type";
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

type CourseListBaseQueryParams = Partial<CourseListQueryParams>;

const invalidateCourseQueries = (
    queryClient: ReturnType<typeof useQueryClient>,
) => {
    return queryClient.invalidateQueries({ queryKey: ["list-course"] });
};

// #region ListCourse
export const useGetListCourse = (params: CourseListBaseQueryParams = {}) => {
    return useQuery({
        queryKey: ["list-course", params],
        queryFn: async () => {
            return await courseApi.getListCourse(params);
        },
        placeholderData: keepPreviousData,
        refetchOnMount: false, //
        staleTime: StaleTime,
        gcTime: GcTime,
    });
};

// #endregion
// #region Create
export const useCreateCourse = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: async (payload: CourseCreateRequest) => {
            const response = await courseApi.createCourse(payload);
            return response.data;
        },
        onSuccess: async () => {
            await invalidateCourseQueries(queryClient);
        },
        onError: (err) => {
            console.error("Create course error:", err);
        },
    });
};
// #endregion
// #region Publish
export const usePublishCourse = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (courseId: string) => {
            const response = await courseApi.publishCourse(courseId);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateCourseQueries(queryClient);
        },
    });
};
// #endregion
// #region UnPublish
export const useUnPublishCourse = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (courseId: string) => {
            const response = await courseApi.unPublishCourse(courseId);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateCourseQueries(queryClient);
        },
    });
};
// #endregion
// #region Reorder
export const useReOrderCourse = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (payload: CourseReOrderRequest) => {
            const response = await courseApi.reOrderCourse(payload);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateCourseQueries(queryClient);
        },
    });
};
// #endregion
// #region GetDashboardCourses
export const useGetCourseForDashboard = () => {
    return useQuery({
        queryKey: ["dashboard-courses"],
        queryFn: async () => {
            return (await courseApi.loadCoursesForDashboard()).data;
        },
        placeholderData: keepPreviousData,
        refetchOnMount: false, //
        staleTime: StaleTime,
        gcTime: GcTime,
    });
};
// #endregion
// #region Update
export const useUpdateCourse = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (payload: UpdateCourseRequest) => {
            const response = await courseApi.updateCourse(payload);
            return response.data;
        },
        onSuccess: async () => {
            await wait(TimeAwaitHandlerApi);
            await invalidateCourseQueries(queryClient);
        },
    });
};
// #endregion
// #region Delete
export const useDeleteCourse = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (courseId: string) => {
            const response = await courseApi.deleteCourse(courseId);
            return response.data;
        },
        onSuccess: async () => {
            // Dữ liệu list-course đọc từ search index qua outbox/event nên có thể cập nhật chậm hơn API lesson.
            // Refetch thêm một nhịp ngắn để giảm khả năng item cũ còn xuất hiện tạm thời.
            await wait(TimeAwaitHandlerApi);
            await invalidateCourseQueries(queryClient);
        },
    });
};
