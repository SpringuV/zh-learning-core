import {
    type UserListQueryParams,
    usersApi,
} from "@/modules/users/api/users.api";
import { useInfiniteQuery } from "@tanstack/react-query";

type UserListBaseQueryParams = Omit<UserListQueryParams, "searchAfterValues">;

export const useInfiniteUserList = (params: UserListBaseQueryParams = {}) => {
    return useInfiniteQuery({
        queryKey: ["users", params],
        initialPageParam: undefined as string | undefined,
        queryFn: async ({ pageParam }) => {
            return await usersApi.getListUsers({
                ...params,
                searchAfterValues: pageParam,
            });
        },
        getNextPageParam: (lastPage) => {
            const payload = lastPage.data;
            if (!payload.hasNextPage || !payload.nextCursor) {
                return undefined;
            }

            return payload.nextCursor;
        },
        retry: 2, // Thử lại tối đa 2 lần nếu có lỗi
        staleTime: 5 * 60 * 1000, // 5 phút
        gcTime: 10 * 60 * 1000, // 10 phút
        refetchOnWindowFocus: false, // không tự động refetch khi cửa sổ được focus lại
    });
};
