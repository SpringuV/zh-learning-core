// B1: Phân tích vấn đề

import { UiStatus } from "@/shared/types/store.type";
import { assign, setup } from "xstate";

// 1.2: xác định event handler để kích hoạt tài khoản, bao gồm cả việc gọi API và xử lý kết quả trả về
type UserActivateEvents =
    | { type: "ACTIVATE"; email: string; code: string }
    | { type: "SUCCESS" }
    | { type: "ERROR"; message: string }
    | { type: "RESET" };
// 1.3: xác định context để lưu trữ trạng thái và event handler của quá trình kích hoạt tài khoản,
//  giúp các component khác có thể truy cập và sử dụng
type Context = {
    message: string;
    status: UiStatus;
};
// 1.4: Xác định actions để cập nhật trạng thái và message dựa trên
// kết quả của quá trình kích hoạt tài khoản,
// B2: tạo machine
export const machineActivate = setup({
    types: {
        context: {} as Context,
        events: {} as UserActivateEvents,
    },
    actions: {
        setLoading: assign({
            status: "loading",
            message: "Đang xác thực mã kích hoạt...",
        }),
        setSuccess: assign({
            status: "success",
            message: "Tài khoản đã được kích hoạt thành công!",
        }),
        setError: assign(({ event }) => ({
            status: "error",
            message:
                event.type === "ERROR"
                    ? event.message
                    : "Kích hoạt thất bại hoặc liên kết đã hết hạn.",
        })),
        setInvalidActivateParams: assign({
            status: "error",
            message:
                "Liên kết kích hoạt không hợp lệ. Vui lòng kiểm tra lại email kích hoạt.",
        }),
        reset: assign({ status: "idle", message: "" }),
    },
    guards: {
        hasValidActivateParams: ({ event }) =>
            event.type === "ACTIVATE" &&
            Boolean(event.email.trim()) &&
            Boolean(event.code.trim()),
    },
}).createMachine({
    /** @xstate-layout N4IgpgJg5mDOIC5QEMDGAXAlgN2esAtGqgPYCuAdugHSYQA2YAxAIIDCAKgJIBqLHAUQDaABgC6iUAAcSsTFhIVJIAB6IAjAHYATNU0AWESPXaAnAA4AbNs1XLAGhABPRAGZzI6pfOaArCMt9axFXWwBfMMc0LFx8IlRSSho6RlZOXn5hdQkkEBk5BSVctQQtXQMjEwtrW0sHZ0RtbV8vV2aQ0PNTP18IqIwcPEJicipqaMGsCigmAGUAVTY2AVnZ0RzpWXlMRWUSy3VTahFTf1cTn1MRTXVHFwRfQOpffX91S00dD8s+kAnY4YJUY0f54TDTJgCABKUIA8lD1sp8ttdsVEAcjiczhdNFcbnc3JYjuZ1P4AiJfJ9ST9In8BgD4okxrAyAk4LAmFCVgIOIjcsjCntEOYms9DCEQr5TK5AvoCQg6upqKZ1EYmupztTtL9QXERklqGAAE5GkhGtLcPiCPmbAo7IqgfaHY6nCUWXHXW4NBBBTTUdQeYzqfSkw6+cw6+lDRnAw0ms0WjLW7JIraCtEIaX6ZVkkO+Q7mEmueWk3S+F5GIy+VyuQ6nSMxaP6sbG03mrmzHk2vJp+1ChAisvi86uKUy-Ry70k6ivSvXNWj-Ta34UEgQODKXWApnoVN21GOxAEer3Y-HOcXueaBuTbexlJgPcoh2qRBLkvvGeuWfncv5z4RrSW4xgaurglAT7poePrmK4zyutoo4Bs0ZjyoY2bWGSsFtBqo43gyzY0CybKwPA-K9ger6Zt0XiUi8AafN077elK2YBCG5jlrY1Q0v0jZ6kCBqtmakF9hmpg0ZYdEcYxBjaPKzTZiqVwUiGIiBsGEQREAA */
    id: "activate-account",
    initial: "idle",
    // khởi tạo context với giá trị mặc định, có thể được cập nhật khi gọi machine
    context: {
        message: "",
        status: "idle",
    },
    states: {
        idle: {
            on: {
                ACTIVATE: [
                    {
                        guard: "hasValidActivateParams",
                        target: "activating",
                        actions: "setLoading",
                    },
                    {
                        target: "error",
                        actions: "setInvalidActivateParams",
                    },
                ],
            },
        },
        activating: {
            on: {
                SUCCESS: {
                    target: "success",
                    actions: "setSuccess",
                },
                ERROR: {
                    target: "error",
                    actions: "setError",
                },
            },
        },
        success: {
            on: {
                RESET: {
                    target: "idle",
                    actions: "reset",
                },
            },
        },
        error: {
            on: {
                ACTIVATE: [
                    {
                        guard: "hasValidActivateParams",
                        target: "activating",
                        actions: "setLoading",
                    },
                    {
                        target: "error",
                        actions: "setInvalidActivateParams",
                    },
                ],
                RESET: {
                    target: "idle",
                    actions: "reset",
                },
            },
        },
    },
});
