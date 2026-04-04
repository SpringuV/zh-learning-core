import { usersApi } from "@/modules/users/api/users.api";
import { useQuery } from "@tanstack/react-query";

export const useUserList = () => {
    return useQuery({
        queryKey: ["users"],
        queryFn: async () => {
            return await usersApi.getListUsers();
        },
        staleTime: 5 * 60 * 1000, // 5 phút
        gcTime: 10 * 60 * 1000, // 10 phút
        refetchOnWindowFocus: false, // không tự động refetch khi cửa sổ được focus lại
    });
};
