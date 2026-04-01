import { authApi } from "@/modules/auth/api/auth.api";
import { useMutation } from "@tanstack/react-query";

export const useAuthLogout = () => {
    return useMutation({
        mutationFn: async () => await authApi.Logout(),
        onError: (err) => {
            console.error("Logout error:", err);
        },
    });
};
