import {
    type UserListQueryParams,
    usersApi,
} from "@/modules/users/api/users.api";
import { useInfiniteQuery } from "@tanstack/react-query";

export const useUserList = (params: UserListQueryParams = {}) => {
    return useInfiniteQuery({
        queryKey: ["users", params],
        queryFn: async ({ pageParam }) => {
            return await usersApi.getListUsers({
                ...params,
                searchAfterCreatedAt: pageParam,
            });
        },
        initialPageParam: undefined as string | undefined,
        getNextPageParam: (lastPage) => {
            if (!lastPage.data.hasNextPage) {
                return undefined;
            }

            const nextCursor = lastPage.data.nextCursor;
            return nextCursor || undefined;
        },
        retry: 2, // Thử lại tối đa 2 lần nếu có lỗi
        staleTime: 5 * 60 * 1000, // 5 phút
        gcTime: 10 * 60 * 1000, // 10 phút
        refetchOnWindowFocus: false, // không tự động refetch khi cửa sổ được focus lại
    });
};
