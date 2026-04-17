import {
    type UserListQueryParams,
    usersApi,
} from "@/modules/users/api/users.api";
import { useQuery } from "@tanstack/react-query";

type UserListBaseQueryParams = Partial<UserListQueryParams>;

export const useUserList = (params: UserListBaseQueryParams = {}) => {
    return useQuery({
        queryKey: ["users", params],
        queryFn: async () => {
            return await usersApi.getListUsers(params);
        },
        retry: 2, // Thử lại tối đa 2 lần nếu có lỗi
        staleTime: 5 * 60 * 1000, // 5 phút
        gcTime: 10 * 60 * 1000, // 10 phút
        refetchOnWindowFocus: false, // không tự động refetch khi cửa sổ được focus lại,
        // nó sẽ chỉ refetch khi người dùng cuộn đến cuối danh sách hoặc gọi hàm refetch thủ công
    });
};
