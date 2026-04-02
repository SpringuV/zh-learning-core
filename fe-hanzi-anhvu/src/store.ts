import {
    AuthStatusState,
    ErrorState,
    LoadingState,
    MessageState,
} from "@/shared/types/store.type";
import { create } from "zustand";

export const useErrorStore = create<ErrorState>((set) => ({
    // get chủ yếu dùng để truy cập nội bộ của store, set để cập nhật state của store
    error: null,
    setError: (state: string | null) => set({ error: state }),
}));

export const useLoadingStore = create<LoadingState>((set) => ({
    loading: false,
    setLoading: (state: boolean) => set({ loading: state }),
}));

export const useMessageStore = create<MessageState>((set) => ({
    message: null,
    setMessage: (state: string | null) => set({ message: state }),
}));

export const useAuthStatusStore = create<AuthStatusState>((set) => ({
    status: "idle",
    message: "",
    loading: false,
    setStatus: (status) =>
        set({
            status,
            loading: status === "loading",
        }),
    setMessage: (message) => set({ message }),
    setLoading: (loading) =>
        set((state) => ({
            loading,
            status: loading ? "loading" : state.status,
        })),
    setStatusMessage: (status, message) =>
        set({
            status,
            message,
            loading: status === "loading",
        }),
    reset: () =>
        set({
            status: "idle",
            message: "",
            loading: false,
        }),
}));
