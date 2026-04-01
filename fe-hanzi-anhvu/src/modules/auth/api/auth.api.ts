// Base URL for API, fallback to localhost if env not set

import {
    LoginRequest,
    LoginResponse,
    RegisterRequest,
    RegisterResponse,
} from "@/modules/auth/types/auth.inteface";
import http from "@/shared/utils/http";

const endpoints = {
    login: "/auth/v1/login",
    logout: "/auth/v1/logout",
    refresh: "/auth/v1/refresh-token",
    register: "/auth/v1/register",
    changeEmail: "/auth/v1/change-email",
    changePassword: "/auth/v1/change-password",
    forgotPassword: "/auth/v1/forgot-password",
    verifyEmail: "/auth/v1/verify-email",
    resendEmailActivation: "/auth/v1/activate/resend",
    activate: "/auth/v1/activate",
};

export const authApi = {
    async Login(payload: LoginRequest) {
        return await http.post<LoginResponse>(endpoints.login, payload);
    },
    async Register(payload: RegisterRequest) {
        return await http.post<RegisterResponse>(endpoints.register, payload);
    },
    async Logout() {
        return await http.post(endpoints.logout);
    },
    async Refresh() {
        return await http.post(endpoints.refresh);
    },
};
