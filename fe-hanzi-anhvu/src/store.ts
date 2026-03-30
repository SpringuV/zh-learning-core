import { ErrorState } from "@/types/store.type";
import { create } from "zustand";

export const useErrorStore = create<ErrorState>((set) => ({
    // get chủ yếu dùng để truy cập nội bộ của store, set để cập nhật state của store
    error: null,
    setError: (state: string | null) => set({ error: state }),
}));
