import { useCallback, useMemo, useRef, useState } from "react";
import { AxiosError } from "axios";
import {
    useAuthActivateAccount,
    useAuthResendActivation,
} from "@/modules/auth/hooks/use-auth-query";
import { useAuthStatus } from "@/modules/auth/hooks/use-auth-status";

type OrchestratorMode = "activate" | "resend";

type UseAuthActivationOrchestratorOptions = {
    mode: OrchestratorMode;
    email?: string;
    code?: string;
};

export function useAuthActivationOrchestrator({
    mode,
    email,
    code,
}: UseAuthActivationOrchestratorOptions) {
    const initialEmail = (email || "").trim();
    const normalizedCode = (code || "").trim();
    const [emailInput, setEmailInput] = useState(initialEmail);

    const activateMutation = useAuthActivateAccount();
    const resendMutation = useAuthResendActivation();
    const requestInFlightRef = useRef(false);

    const canActivate = useMemo(
        () => Boolean(initialEmail) && Boolean(normalizedCode),
        [initialEmail, normalizedCode],
    );

    const {
        status,
        message,
        setStatus,
        setLoading,
        setSuccess,
        setError,
        setWarning,
    } = useAuthStatus(
        mode === "activate" ? (canActivate ? "idle" : "warning") : "idle",
        mode === "activate"
            ? canActivate
                ? "Nhấn 'Kích hoạt ngay' để xác thực tài khoản của bạn."
                : "Liên kết kích hoạt không hợp lệ. Vui lòng kiểm tra lại email kích hoạt."
            : initialEmail
              ? "Nhấn 'Gửi lại email kích hoạt' để gửi email mới."
              : "Nhập email để nhận lại liên kết kích hoạt.",
    );

    const activate = useCallback(async () => {
        if (requestInFlightRef.current) {
            return;
        }

        if (!canActivate) {
            setWarning(
                "Liên kết kích hoạt không hợp lệ. Vui lòng kiểm tra lại email kích hoạt.",
            );
            return;
        }

        requestInFlightRef.current = true;
        setLoading("Đang xác thực mã kích hoạt...");

        try {
            const response = await activateMutation.mutateAsync({
                Email: initialEmail,
                Code: normalizedCode,
            });

            setSuccess(
                response.data?.message || "Kích hoạt tài khoản thành công.",
            );
        } catch (error) {
            const axiosError = error as AxiosError<{ message?: string }>;
            setError(
                axiosError.response?.data?.message ||
                    "Kích hoạt thất bại hoặc liên kết đã hết hạn.",
            );
        } finally {
            requestInFlightRef.current = false;
        }
    }, [
        activateMutation,
        canActivate,
        initialEmail,
        normalizedCode,
        setError,
        setLoading,
        setSuccess,
        setWarning,
    ]);

    const resend = useCallback(
        async (targetEmail?: string) => {
            if (requestInFlightRef.current) {
                return;
            }

            const normalizedEmail = (targetEmail ?? emailInput).trim();
            if (!normalizedEmail) {
                setError("Vui lòng nhập email đã đăng ký.");
                return;
            }

            requestInFlightRef.current = true;
            setLoading("Đang gửi lại email kích hoạt...");

            try {
                const response = await resendMutation.mutateAsync({
                    Email: normalizedEmail,
                });

                setSuccess(
                    response.data?.message ||
                        "Đã gửi lại email kích hoạt. Vui lòng kiểm tra hộp thư.",
                );
            } catch (error) {
                const axiosError = error as AxiosError<{ message?: string }>;
                setError(
                    axiosError.response?.data?.message ||
                        "Không thể gửi lại email kích hoạt. Vui lòng thử lại.",
                );
            } finally {
                requestInFlightRef.current = false;
            }
        },
        [emailInput, resendMutation, setError, setLoading, setSuccess],
    );

    const resetToIdle = useCallback(() => {
        setStatus("idle");
    }, [setStatus]);

    return {
        status,
        message,
        canActivate,
        isLoading: status === "loading",
        emailInput,
        setEmailInput,
        activate,
        resend,
        resetToIdle,
    };
}
