import { useEffect, useRef } from "react";
import { useAuthStatusStore } from "@/store";
import { AuthUiStatus } from "@/shared/types/store.type";

export function useAuthStatus(
    initialStatus: AuthUiStatus,
    initialMessage: string,
) {
    const initializedRef = useRef(false);
    const status = useAuthStatusStore((state) => state.status);
    const message = useAuthStatusStore((state) => state.message);
    const setStatus = useAuthStatusStore((state) => state.setStatus);
    const setMessage = useAuthStatusStore((state) => state.setMessage);
    const setStatusMessage = useAuthStatusStore(
        (state) => state.setStatusMessage,
    );
    const reset = useAuthStatusStore((state) => state.reset);

    useEffect(() => {
        if (initializedRef.current) {
            return;
        }

        initializedRef.current = true;
        setStatusMessage(initialStatus, initialMessage);
    }, [initialMessage, initialStatus, setStatusMessage]);

    const setLoading = (nextMessage: string) => {
        setStatusMessage("loading", nextMessage);
    };

    const setSuccess = (nextMessage: string) => {
        setStatusMessage("success", nextMessage);
    };

    const setError = (nextMessage: string) => {
        setStatusMessage("error", nextMessage);
    };

    const setWarning = (nextMessage: string) => {
        setStatusMessage("warning", nextMessage);
    };

    return {
        status,
        message,
        setStatus,
        setMessage,
        setLoading,
        setSuccess,
        setError,
        setWarning,
        reset,
    };
}
