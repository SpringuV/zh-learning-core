import { useState, useCallback } from "react";
import { useAuthResendActivation } from "@/modules/auth/hooks/use-auth-query";
import { UiStatus } from "@/shared/types/store.type";

export type UseResendOptions = {
    account: string;
    typeUsername: "Email" | "Phone" | "Username";
};

export function useResend(options: UseResendOptions) {
    const { account, typeUsername } = options;
    const [status, setStatus] = useState<UiStatus>("idle");
    const [message, setMessage] = useState("");

    const resendMutation = useAuthResendActivation();

    const canResend = Boolean(account.trim());

    const resend = useCallback(async () => {
        if (!canResend) {
            setStatus("warning");
            setMessage("Không thể gửi lại email. Vui lòng thử lại sau.");
            return;
        }

        setStatus("loading");
        setMessage("Đang gửi lại email kích hoạt...");

        try {
            await resendMutation.mutateAsync({
                Account: account,
                TypeUsername: typeUsername,
            });
            setStatus("success");
            setMessage("Email kích hoạt đã được gửi lại!");
        } catch (error: any) {
            setStatus("error");
            setMessage(
                error?.response?.data?.message ||
                    "Không thể gửi lại email kích hoạt. Vui lòng thử lại.",
            );
        }
    }, [canResend, account, typeUsername, resendMutation]);

    const reset = useCallback(() => {
        setStatus(canResend ? "idle" : "warning");
        setMessage(
            canResend
                ? "Nhấn 'Gửi lại email' để nhận liên kết kích hoạt mới."
                : "Không thể gửi lại email. Vui lòng thử lại sau.",
        );
    }, [canResend]);

    return {
        status,
        message,
        canResend,
        isLoading: status === "loading",
        resend,
        reset,
    };
}
