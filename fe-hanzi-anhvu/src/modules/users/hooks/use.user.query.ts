import {
    type UserListQueryParams,
    usersApi,
} from "@/modules/users/api/users.api";
import { keepPreviousData, useQuery } from "@tanstack/react-query";

export const useUserList = (params: UserListQueryParams = {}) => {
    return useQuery({
        queryKey: ["users", params],
        queryFn: async () => {
            return await usersApi.getListUsers(params);
        },
        retry: 2, // Thử lại tối đa 2 lần nếu có lỗi
        placeholderData: keepPreviousData,
        staleTime: 5 * 60 * 1000, // 5 phút
        gcTime: 10 * 60 * 1000, // 10 phút
        refetchOnWindowFocus: false, // không tự động refetch khi cửa sổ được focus lại
    });
};
