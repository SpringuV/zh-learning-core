import { LoginRequest } from "@/modules/auth/types/auth.inteface";
import type { SignInResponse } from "next-auth/react";
import { assign, fromPromise, setup } from "xstate";

// event đại diện cho các hành động người dùng có thể thực hiện trên form login
type LoginEvents = { type: "SUBMIT"; input: LoginRequest } | { type: "RESET" };

// context đại diện cho state của form login, bao gồm request hiện tại
//  và lỗi nếu có
type LoginContext = {
    request: LoginRequest | null;
    errorMessage: string | null;
};

export const authLoginMachine = setup({
    types: {
        context: {} as LoginContext,
        events: {} as LoginEvents,
    },
    actors: {
        // Placeholder actor; sẽ được override từ useMachine(..., { actors })
        loginWithCredentials: fromPromise(
            async ({ input }: { input: LoginRequest }) => {
                void input;
                return {
                    error: undefined,
                    code: undefined,
                    status: 200,
                    ok: true,
                    url: null,
                } as SignInResponse;
            },
        ),
    },
    actions: {
        setRequest: assign(({ event }) => ({
            request: event.type === "SUBMIT" ? event.input : null,
            errorMessage: null,
        })),
        setErrorFromInvoke: assign(({ event }) => {
            const error = (event as { error?: unknown }).error;
            const message =
                error instanceof Error && error.message
                    ? error.message
                    : typeof error === "string"
                      ? error
                      : "Đăng nhập thất bại. Vui lòng thử lại.";

            return { errorMessage: message };
        }),
        setSuccess: assign({
            errorMessage: null,
        }),
        reset: assign({
            request: null,
            errorMessage: null,
        }),
    },
    guards: {
        isUnactiveError: ({ event }) => {
            const error = (event as { error?: unknown }).error;
            const message =
                error instanceof Error && error.message
                    ? error.message.toLowerCase()
                    : typeof error === "string"
                      ? error.toLowerCase()
                      : "";

            return message.includes("chưa được kích hoạt hoặc đã bị khóa");
        },
    },
}).createMachine({
    /** @xstate-layout N4IgpgJg5mDOIC5QEMCuAXAFgWgDYHsoBLAOwDoiJcwBiAZQFUAhAWQEkAVAbQAYBdRKAAO+WEXRF8JQSAAeiAEwB2BWQCsAFgCMCgBxqeGnsZ5KAzABoQAT0TYtGtWQCcSrT00A2M7vNq1AL4BVmhYeISkZLCoAEYAtuISJFA0EFJgFCQAbvgA1hmhOATE5NHxiaRQCKQ5AMbIElK8fM0yImKN0khyiDxWtgg8QSEYRRGlsQnoSSlgAE5z+HNkQrgNAGZLcWSF4SVRkxXJ1dn49Z3Nrd3t4pJdoPIIus5ORgpmygpaarq6jv2IMxqLRkBQ8ZyeBTOMzOb5mDQKDTDEC7YqRMpTGY0eaLZarDZbHajPbow7TSonOoNO6XLQCa6iW5SGSPfS6UE+HhaXSeTTPXkAhAaZzsj4fbk8XR6BT+ZGo8YHWq1OCwejMdjcfhtRmdFmIYFKMhaGGQrSw3RmbkaQXc1SecE8MxKBFKYEuuXEtETJUqmgAJQAonQA5r6cIdXc9QgdGRPJ4VP4NBaXi9JYKlIafpLPFpPBoYS8NJ4PWEvWR1sgiLhUHNaIxWJwruGOpHuo88zwyLpc+DLUXuz9BZbO9pHb4gQjfJoS2N9hWqzXaIHg6HtS3mW3EPnOzmPq6HL54zxPIKEaoM-GNI5PNCtEoZyTyKgSMhahIskugwGAHIAEQA+gAggAwhwbAAGqAWBADy35NiANy6pugyCkMwQop6CrPq+751uqjZagy673D0CBmJaXapgWbi6KYNpqJ4sbmMahh3le3IPmW2FvkQH7+l+q5EUyJGPF8ILfAYx5GDysLWjYihGEaSjdlCea-AYuZBOhJD4BAcAyPKJRrsJUbYEmgrYO8ZhkAi7jdvaxoyvmnEKpQ1DGUhDyKBmLgaK6rhGAYSaGIKniiu8OiIhmXzQi5+wYkcUAea2XlPAoqgfMKMndmYkLOBZkI2TekL2s4sIisKcWkj6sDwEJnmkX08kIAo9pkGKkV+SoZpmFV5DztWtbJRuqVNQMMIaF2xosY4xrmn1ZDcbhw0ib0oVuLG4JXkoxi+NNWkBEAA */
    id: "auth-login",
    initial: "idle",
    context: {
        request: null,
        errorMessage: null,
    },
    states: {
        idle: {
            on: {
                SUBMIT: {
                    target: "submitting",
                    actions: "setRequest",
                },
            },
        },
        submitting: {
            invoke: {
                src: "loginWithCredentials",
                input: ({ context }) => context.request as LoginRequest,
                onDone: {
                    target: "success",
                    actions: "setSuccess",
                },
                onError: [
                    {
                        guard: "isUnactiveError",
                        target: "unactive",
                        actions: "setErrorFromInvoke",
                    },
                    {
                        target: "failure",
                        actions: "setErrorFromInvoke",
                    },
                ],
            },
        },
        success: {
            on: {
                SUBMIT: {
                    target: "submitting",
                    actions: "setRequest",
                },
                RESET: {
                    target: "idle",
                    actions: "reset",
                },
            },
        },
        failure: {
            on: {
                SUBMIT: {
                    target: "submitting",
                    actions: "setRequest",
                },
                RESET: {
                    target: "idle",
                    actions: "reset",
                },
            },
        },
        unactive: {
            on: {
                SUBMIT: {
                    target: "submitting",
                    actions: "setRequest",
                },
                RESET: {
                    target: "idle",
                    actions: "reset",
                },
            },
        },
    },
});
