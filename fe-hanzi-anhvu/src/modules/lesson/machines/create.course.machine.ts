import {
    CourseCreateRequest,
    CourseCreateResponseApi,
} from "@/modules/lesson/types/coure.type";
import axios from "axios";
import { assign, fromPromise, setup } from "xstate";

type CourseCreateEvents =
    | { type: "SUBMIT"; input: CourseCreateRequest }
    | { type: "RESET" };

type CourseCreateContext = {
    request: CourseCreateRequest | null;
    error: string | null;
    messageResponse: CourseCreateResponseApi | null;
};

const getErrorMessage = (error: unknown): string => {
    if (axios.isAxiosError<CourseCreateResponseApi>(error)) {
        return error.response?.data?.message ?? error.message;
    }

    if (error instanceof Error) {
        return error.message;
    }

    if (typeof error === "string") {
        return error;
    }

    return "Đã xảy ra lỗi không xác định. Vui lòng thử lại.";
};

export const createCourseMachine = setup({
    types: {
        events: {} as CourseCreateEvents,
        context: {} as CourseCreateContext,
    },
    actions: {
        setRequest: assign(({ event }) => ({
            request: event.type === "SUBMIT" ? event.input : null,
            error: null,
            messageResponse: null,
        })),
        reset: assign({
            request: null,
            error: null,
            messageResponse: null,
        }),
    },
    actors: {
        // Placeholder actor; sẽ được override từ useMachine(..., { actors })
        createCourse: fromPromise(
            async ({ input }: { input: CourseCreateRequest }) => {
                void input; // tránh lỗi unused variable
                return {
                    success: true,
                    message: "Khóa học đã được tạo thành công.",
                } as CourseCreateResponseApi;
            },
        ),
    },
    guards: {
        hasValidRequest: ({ event }) =>
            event.type === "SUBMIT" &&
            Boolean(event.input.Title.trim()) &&
            event.input.HskLevel >= 0,
    },
}).createMachine({
    /** @xstate-layout N4IgpgJg5mDOIC5QGMBOYCGAXMBaZA9gK6qxgB0AlhADZgDEAygKoBCAsgJIAqA2gAwBdRKAAOBWJSyUCAOxEgAHogCsAFgDM5AGwBGFdoAch-gHYVGk2oA0IAJ6Jda-uVO7TATg8aATB4saboYAvsG2aJg4+MSkFNR0TGxcfLrCSCDiktJyCsoI6lp6BsZmFla2Dgi4PpbkatpqaoZqpkba-qHh6Nh4hCRk5LBEAEYAtlLSslD0EHJxsgBuBADWFBE90f0UQ2MTlFMI+0vI2DKyAoIXCplSZ7mI3mrkvobuDdpu2vweFYi4-uR3GYNGpdB41CoPIZ1J0QOson1YoMRuMsJNpmBUKgCKhyKIaNgAGY40bkeG9GIDHao9GHRYEE7Zc5CK7pG5M+4IXQglSAyy6ZpBQyBFS-KqeQHgszOUwtEHC2HkzZIobIZBwWCJDg8VliCS3HLpPKPZ4+V6teqfb5i6rkFTmB0+UyGcweM0aFSK7oIynbIhqjX0ABKAFFGCG+EJrvqOUbEKZ+LzfBo3n4vipEzbdC5tOo3bLAoYPAnTD4vZEKVtyISMJQaCQGCxtZG0nqsnc41yNB4XPwmgK-NCzaYxdy6vxvvwzaDs40fNpyxtEQMa3WG8GwxHdRkYx3QHknD5eW5+B9S872qL7H9+a4VIYahY1E6PE5TKEwiBZAQIHAFErlzAaN20NfdEA9FwoQMV8e20HwWkaG16g8HRTF8CErG5FRdEXH0q3iIC2V3UClHAqFyCg9owVPeDZRsa8uQzcgalTEwjF8KdcMrFUUT2KZgINeRO10XQjx0I8Pn0CFiz8G0XDcMEXVPZ8LHBIwuOVKl-XVWB4CIkChLAhATBQ9pXhMQcp0MUcfHHblRNzF1DC+ecNMA6ta3rdABNjIypN5fhoSMUS2NLejKlwOCKNeSFzOLES-A-YIgA */
    id: "create-course",
    initial: "idle",
    context: {
        request: null,
        error: null,
        messageResponse: null,
    },
    states: {
        idle: {
            on: {
                SUBMIT: [
                    {
                        guard: "hasValidRequest",
                        target: "submitting",
                        actions: "setRequest",
                    },
                    {
                        target: "failure",
                        actions: assign({
                            error: "Vui lòng điền đầy đủ thông tin hợp lệ.",
                            request: null,
                            messageResponse: null,
                        }),
                    },
                ],
            },
        },
        submitting: {
            invoke: {
                src: "createCourse",
                input: ({ context }) => context.request as CourseCreateRequest,
                onDone: {
                    target: "success",
                    actions: assign(({ event }) => ({
                        messageResponse: event.output,
                        error: null,
                        request: null,
                    })),
                },
                onError: {
                    target: "failure",
                    actions: assign(({ event }) => ({
                        error: getErrorMessage(event.error),
                        messageResponse: null,
                        request: null,
                    })),
                },
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
    },
});
