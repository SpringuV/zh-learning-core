import {
    AuthSessionEvent,
    authSessionMachine,
} from "@/modules/auth/machines/auth-session/auth.session.machine";
import { createActor } from "xstate";

// Type alias cho actor của auth session machine
// Sử dụng ReturnType để lấy type chính xác từ createActor
type AuthSessionActor = ReturnType<
    typeof createActor<typeof authSessionMachine>
>;

// Biến global để lưu trữ actor instance (singleton pattern)
// Đảm bảo chỉ có 1 actor trong toàn bộ ứng dụng
let authSessionActor: AuthSessionActor | null = null;

// Hàm đảm bảo actor được tạo và khởi động
// Kiểm tra SSR-safe và lazy initialization
const ensureAuthSessionActor = () => {
    // Không tạo actor trên server (SSR) vì không có DOM
    if (typeof window === "undefined") {
        return null;
    }

    // Nếu chưa có actor, tạo mới và start
    // phải tạo mới mỗi lần vì createActor trả về một instance mới, nhưng đảm bảo chỉ có một instance được start
    // start() sẽ kích hoạt máy trạng thái và cho phép nó bắt đầu xử lý các sự kiện
    if (!authSessionActor) {
        authSessionActor = createActor(authSessionMachine);
        authSessionActor.start();
    }

    // Trả về actor để sử dụng
    return authSessionActor;
};

// Hàm public để khởi động auth session runtime
// Được gọi từ component listener để setup ban đầu
export const startAuthSessionRuntime = () => {
    ensureAuthSessionActor();
};

// Hàm gửi event đến auth session machine
// Được gọi từ HTTP interceptor khi refresh token
export const sendAuthSessionEvent = (event: AuthSessionEvent) => {
    // Lấy actor (hoặc tạo nếu chưa có)
    const actor = ensureAuthSessionActor();
    // Gửi event qua send() method, optional chaining vì actor có thể null trên server
    actor?.send(event);
};

// Hàm subscribe để lắng nghe state changes của machine
// Trả về cleanup function để unsubscribe
export const subscribeAuthSession = (
    listener: (snapshot: ReturnType<AuthSessionActor["getSnapshot"]>) => void,
) => {
    // Lấy actor
    const actor = ensureAuthSessionActor();
    // Nếu không có actor (server), trả về no-op function
    if (!actor) {
        return () => undefined;
    }

    // Subscribe listener, sẽ được gọi khi machine state thay đổi
    const subscription = actor.subscribe(listener);

    // Trả về cleanup function để unsubscribe khi component unmount
    return () => {
        subscription.unsubscribe();
    };
};
