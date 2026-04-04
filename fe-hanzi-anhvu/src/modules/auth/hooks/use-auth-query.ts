import {
    ActivateAccountRequest,
    LoginRequest,
    RegisterRequest,
    ResendActivationRequest,
} from "@/modules/auth/types/auth.inteface";
import { useMutation } from "@tanstack/react-query";
import { authApi } from "@/modules/auth/api/auth.api";
import { signIn } from "next-auth/react";

function decodeErrorCode(value?: string | null) {
    if (!value) return null;

    try {
        return decodeURIComponent(value);
    } catch {
        return value;
    }
}

function getLoginErrorMessage(errorCode?: string | null, code?: string | null) {
    const backendMessage = decodeErrorCode(code);
    if (backendMessage && backendMessage !== "credentials") {
        return backendMessage;
    }

    if (errorCode === "CredentialsSignin" || errorCode === "Configuration") {
        return "Tên đăng nhập hoặc mật khẩu không hợp lệ.";
    }

    return "Đăng nhập thất bại. Vui lòng thử lại.";
}

export const useAuthCredentialSignIn = () => {
    return useMutation({
        mutationFn: async (payload: LoginRequest) => {
            const result = await signIn("credentials", {
                nameAccount: payload.Username,
                password: payload.Password,
                typeLogin: payload.TypeLogin,
                redirect: false,
            });

            if (!result) {
                throw new Error("Đăng nhập thất bại. Vui lòng thử lại.");
            }

            if (result.error) {
                throw new Error(
                    getLoginErrorMessage(result.error, result.code),
                );
            }

            return result;
        },
        onError: (err) => {
            console.error("Credential sign-in error:", err);
        },
    });
};

export const useAuthActivateAccount = () => {
    return useMutation({
        mutationFn: async (payload: ActivateAccountRequest) =>
            await authApi.Activate(payload),
        onError: (err) => {
            console.error("Activate account error:", err);
        },
    });
};

export const useAuthResendActivation = () => {
    return useMutation({
        mutationFn: async (payload: ResendActivationRequest) =>
            await authApi.ResendActivation(payload),
        onError: (err) => {
            console.error("Resend activation error:", err);
        },
    });
};

export const useAuthLogout = () => {
    return useMutation({
        mutationFn: async () => await authApi.Logout(),
        onError: (err) => {
            console.error("Logout error:", err);
        },
    });
};

export const useAuthRegister = () => {
    return useMutation({
        mutationFn: async (payload: RegisterRequest) =>
            await authApi.Register(payload),
        onError: (err) => {
            console.error("Register error:", err);
        },
    });
};
