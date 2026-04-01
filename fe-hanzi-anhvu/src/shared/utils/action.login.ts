/* eslint-disable @typescript-eslint/no-explicit-any */
"use server";

import { signIn } from "@/auth";
import { LoginRequest } from "@/modules/auth/types/auth.inteface";

// sửa đăng nhập, hiển thị lỗi tường minh hơn
export async function authenticate(payload: LoginRequest) {
    try {
        const response = await signIn("credentials", {
            nameAccount: payload.Username,
            password: payload.Password,
            typeLogin: payload.TypeLogin,
            redirect: false,
        });
        // console.log("Authentication response:", response);
        // Đăng nhập thành công
        return { success: true, message: "Đăng nhập thành công" };
    } catch (err: any) {
        console.error("Authentication error:", err);

        let errorMessage = "Đăng nhập thất bại";

        // Try to extract message from error
        if (typeof err === "string") {
            errorMessage = err;
        } else if (typeof err === "object") {
            // If err.message is a JSON string, try to parse it
            if (typeof err.message === "string") {
                try {
                    const parsed = JSON.parse(err.message);
                    // Try to get message from parsed object
                    const msg =
                        parsed?.message?.message?.[0] ||
                        parsed?.message ||
                        err.message;
                    errorMessage =
                        typeof msg === "string" ? msg : "Đăng nhập thất bại";
                } catch {
                    // Not JSON, use the message as is
                    errorMessage = err.message;
                }
            } else if (err.message?.message?.[0]) {
                // Direct nested structure
                errorMessage = err.message.message[0];
            }
        }

        return {
            success: false,
            message: errorMessage,
        };
    }
}
