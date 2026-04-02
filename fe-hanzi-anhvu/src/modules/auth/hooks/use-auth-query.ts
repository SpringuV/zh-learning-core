import { authApi } from "@/modules/auth/api/auth.api";
import {
    ActivateAccountRequest,
    ResendActivationRequest,
} from "@/modules/auth/types/auth.inteface";
import { useMutation } from "@tanstack/react-query";

export const useAuthLogout = () => {
    return useMutation({
        mutationFn: async () => await authApi.Logout(),
        onError: (err) => {
            console.error("Logout error:", err);
        },
    });
};

export const useAuthActivateAccount = () => {
    return useMutation({
        mutationFn: async (payload: ActivateAccountRequest) => {
            return await authApi.Activate(payload);
        },
        onError: (err) => {
            console.error("Activate account error:", err);
        },
    });
};

export const useAuthResendActivation = () => {
    return useMutation({
        mutationFn: async (payload: ResendActivationRequest) => {
            return await authApi.ResendActivation(payload);
        },
        onError: (err) => {
            console.error("Resend activation error:", err);
        },
    });
};
