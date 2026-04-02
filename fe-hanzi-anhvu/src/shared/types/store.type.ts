export type ErrorState = {
    error: string | null;
    setError: (error: string | null) => void;
};

export type LoadingState = {
    loading: boolean;
    setLoading: (state: boolean) => void;
};

export type MessageState = {
    message: string | null;
    setMessage: (state: string | null) => void;
};

export type AuthUiStatus =
    | "idle"
    | "loading"
    | "success"
    | "error"
    | "warning";

export type AuthStatusState = {
    status: AuthUiStatus;
    message: string;
    loading: boolean;
    setStatus: (status: AuthUiStatus) => void;
    setMessage: (message: string) => void;
    setLoading: (loading: boolean) => void;
    setStatusMessage: (status: AuthUiStatus, message: string) => void;
    reset: () => void;
};
