import { assign, setup } from "xstate";

// auth session context là nơi lưu trữ trạng thái liên quan đến phiên đăng nhập của người dùng,
// bao gồm thông tin về việc có cần chuyển hướng đến trang đăng nhập hay không
// và lỗi gần nhất khi làm mới token (nếu có).
type AuthSessionContext = {
    shouldRedirectToLogin: boolean;
    lastRefreshError: string | null;
};

export type AuthSessionEvent =
    | { type: "TOKEN_REFRESH_STARTED" }
    | { type: "TOKEN_REFRESH_SUCCEEDED" }
    | { type: "TOKEN_REFRESH_FAILED"; reason?: string }
    | { type: "SESSION_CONFIRMED" }
    | { type: "REDIRECT_HANDLED" };

// set up máy trạng thái cho phiên đăng nhập, định nghĩa các trạng thái và cách chuyển đổi giữa chúng dựa trên các sự kiện
// liên quan đến việc làm mới token và xác nhận phiên đăng nhập. Máy trạng thái này giúp quản lý logic liên quan đến việc giữ
// cho người dùng luôn được xác thực hoặc chuyển hướng họ đến trang đăng nhập khi cần thiết.
export const authSessionMachine = setup({
    actors: {}, // actors là các tác nhân phụ trợ có thể được sử dụng trong máy trạng thái,
    // ở đây chúng ta không sử dụng actor nào nên để trống
    delays: {}, // delays là các khoảng thời gian trì hoãn có thể được sử dụng trong máy trạng thái,
    guards: {}, // guards là các điều kiện ràng buộc có thể được sử dụng để kiểm soát việc chuyển đổi giữa
    // các trạng thái, ở đây chúng ta không sử dụng guard nào nên để trống
    types: {
        context: {} as AuthSessionContext, // Định nghĩa kiểu cho context của máy trạng thái
        events: {} as AuthSessionEvent, // Định nghĩa kiểu cho các sự kiện của state machine
    },
    // Định nghĩa các action để cập nhật context dựa trên các sự kiện
    actions: {
        // Khi token refresh thất bại, đánh dấu rằng cần chuyển hướng đến trang đăng nhập và lưu lỗi
        markRefreshFailed: assign(({ event }) => ({
            shouldRedirectToLogin: true,
            lastRefreshError:
                event.type === "TOKEN_REFRESH_FAILED"
                    ? (event.reason ?? "Refresh token failed")
                    : null,
        })),
        clearRefreshError: assign({
            shouldRedirectToLogin: false,
            lastRefreshError: null,
        }),
        clearRedirectFlag: assign({
            shouldRedirectToLogin: false,
        }),
    },
}).createMachine({
    /** @xstate-layout N4IgpgJg5mDOIC5QEMCuAXAFgZTrAlgPYB2AxAEoCiAIgJJUDCAKgPoASAggHLUAyNAbQAMAXUSgADoQLoixcSAAeiAIxCA7ADoATAE4ArOu0AOAwGZduoWYA0IAJ6IAtCt1nNZ9QDZ9u9f90AFmCVYwBfMLs0LFxYAhJNaMwwYlkAY2R0SFImAHkAaUouFioAMSpsNhZsJg5yJkFRBSkZOQVlBCcDTUDXbXV9My9jY2HjbTtHBG1QnS8hUyHjMyF9FRVAiKiMHDw5RJ2U9MzsvMLisoqq0o5afmphMSQQFvxZEnbnXWNNFXVdbT6QJeFSAkzGQKTRD9QKaEb6IReYJmYIIrzqLYgJKxeLETQAJzAADNCbBMPhiFAcgUiiVKOVKJVqgBVBgMSg0RpPSTSN5tZ4dLo-P4AoEgsEjSEOVTabSaLzaIRWCyGUIjMyY7F7BKEklwcmU6nnOkMpk3O5c5q897yAXQ4wqDzGDSuZ2BPRqLxQhAqMyO8ZotTaQLqYGrTU7HH7VDEJJHfAZLIQUjYRnYWi5YoMTOlegAWUtz1eNs+CBWXg8gKMQiE2nm+hG3qcM00+hBvhV4rcgQ1mOIhAgcAUWri-J5rQ+ds6ft0v2CgJG6j+wK90s6alnQnWvmBbaGIfCkSxke1eLjqQTJwgVonttAgv6jo27obxiXoa8q6mvp6sqsfwGXpdAAiMYlPAliVJA0oBvPlJ3vVR0U0JUNE8WtQS8Ax9G9FQgXlQIa28UM61cCxQN2UcEhjc9jiTWCSynfRFSdQwVkCfRfA43QcMMOdCPRd0QTcXQIgiIA */
    id: "authSession", // Đặt tên cho máy trạng thái
    initial: "authenticated", // Trạng thái ban đầu là "authenticated"
    // trạng thái context ban đầu với shouldRedirectToLogin là false và lastRefreshError là null
    context: {
        shouldRedirectToLogin: false,
        lastRefreshError: null,
    },

    // Định nghĩa các trạng thái và cách chuyển đổi giữa chúng dựa trên các sự kiện
    states: {
        // Khi ở trạng thái authenticated, nếu bắt đầu làm mới token thì chuyển sang trạng thái refreshing
        // Nếu làm mới token thành công thì vẫn ở trạng thái authenticated, nếu thất bại thì chuyển sang trạng thái unauthenticated
        // tự đặt tên cho các sự kiện để dễ hiểu hơn, ví dụ TOKEN_REFRESH_STARTED, TOKEN_REFRESH_SUCCEEDED, TOKEN_REFRESH_FAILED, SESSION_CONFIRMED
        authenticated: {
            // từ khóa on để định nghĩa các sự kiện và cách chúng ảnh hưởng đến trạng thái
            on: {
                TOKEN_REFRESH_STARTED: {
                    // target để xác định trạng thái tiếp theo khi sự kiện xảy ra
                    target: "refreshing",
                },
                TOKEN_REFRESH_FAILED: {
                    target: "unauthenticated",
                    // actions để thực hiện khi sự kiện xảy ra, ở đây là đánh dấu rằng refresh đã thất bại
                    actions: "markRefreshFailed",
                },
            },
        },
        refreshing: {
            // từ khóa on để định nghĩa các sự kiện và cách chúng ảnh hưởng đến trạng thái
            on: {
                TOKEN_REFRESH_SUCCEEDED: {
                    target: "authenticated",
                    // actions để thực hiện khi sự kiện xảy ra, ở đây là xóa lỗi refresh
                    actions: "clearRefreshError",
                },
                TOKEN_REFRESH_FAILED: {
                    target: "unauthenticated",
                    actions: "markRefreshFailed",
                },
            },
        },
        unauthenticated: {
            on: {
                SESSION_CONFIRMED: {
                    target: "authenticated",
                    actions: "clearRefreshError",
                },
            },
        },
    },
    // từ khóa on để định nghĩa các sự kiện toàn cục, ở đây là REDIRECT_HANDLED để xóa cờ chuyển hướng khi đã xử lý xong
    on: {
        REDIRECT_HANDLED: {
            actions: "clearRedirectFlag",
        },
    },
});
