export type ErrorState = {
    error: string | null;
    setError: (error: string | null) => void;
};

export type LoadingState = {
    loading: boolean;
    setLoading: (state: boolean) => void;
};
