import { assign, setup } from "xstate";

type ResendMailInput = {
    account: string;
    typeInput: "Email" | "Phone" | "Username";
};

type ResendMailEvents =
    | { type: "RESEND" }
    | { type: "RESEND_SUCCESS"; message?: string }
    | { type: "RESEND_ERROR"; message: string }
    | { type: "RESET" };

type ResendMailContext = {
    account: string;
    typeInput: "Email" | "Phone" | "Username";
    message: string | null;
};

const FALLBACK_ERROR_MESSAGE =
    "Không thể gửi lại email kích hoạt. Vui lòng thử lại.";

export const resendMailActiveMachine = setup({
    types: {
        input: {} as ResendMailInput,
        context: {} as ResendMailContext,
        events: {} as ResendMailEvents,
    },
    actions: {
        setLoadingMessage: assign({
            message: "Đang gửi lại email kích hoạt...",
        }),
        setSuccessMessage: assign(({ event }) => ({
            message:
                event.type === "RESEND_SUCCESS" && event.message
                    ? event.message
                    : "Đã gửi lại email kích hoạt. Vui lòng kiểm tra hộp thư.",
        })),
        setFailureMessage: assign(({ event }) => ({
            message:
                event.type === "RESEND_ERROR"
                    ? event.message
                    : FALLBACK_ERROR_MESSAGE,
        })),
        setMissingAccountMessage: assign({
            message: "Thiếu thông tin tài khoản để gửi lại kích hoạt.",
        }),
        resetMessage: assign({
            message: null,
        }),
    },
    guards: {
        canResend: ({ context }) => Boolean(context.account.trim()),
    },
}).createMachine({
    id: "resend-mail-active",
    initial: "idle",
    context: ({ input }) => ({
        account: input.account,
        typeInput: input.typeInput,
        message: null,
    }),
    states: {
        idle: {
            on: {
                RESEND: [
                    {
                        guard: "canResend",
                        target: "resending",
                        actions: "setLoadingMessage",
                    },
                    {
                        target: "failure",
                        actions: "setMissingAccountMessage",
                    },
                ],
            },
        },
        resending: {
            on: {
                RESEND_SUCCESS: {
                    target: "success",
                    actions: "setSuccessMessage",
                },
                RESEND_ERROR: {
                    target: "failure",
                    actions: "setFailureMessage",
                },
            },
        },
        success: {
            on: {
                RESEND: [
                    {
                        guard: "canResend",
                        target: "resending",
                        actions: "setLoadingMessage",
                    },
                    {
                        target: "failure",
                        actions: "setMissingAccountMessage",
                    },
                ],
                RESET: {
                    target: "idle",
                    actions: "resetMessage",
                },
            },
        },
        failure: {
            on: {
                RESEND: [
                    {
                        guard: "canResend",
                        target: "resending",
                        actions: "setLoadingMessage",
                    },
                    {
                        target: "failure",
                        actions: "setMissingAccountMessage",
                    },
                ],
                RESET: {
                    target: "idle",
                    actions: "resetMessage",
                },
            },
        },
    },
});
