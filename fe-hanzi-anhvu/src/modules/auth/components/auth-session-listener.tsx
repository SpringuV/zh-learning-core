"use client";

import { useEffect, useRef } from "react";
import { usePathname, useRouter } from "next/navigation";
import { signOut, useSession } from "next-auth/react";
import {
    sendAuthSessionEvent,
    startAuthSessionRuntime,
    subscribeAuthSession,
} from "@/modules/auth/machines/auth-session/auth.session.runtime";

// AuthSessionListener là một component React dùng để lắng nghe trạng thái phiên đăng nhập của người dùng và thực hiện các hành động tương ứng như xác nhận phiên đăng nhập, chuyển hướng đến trang đăng nhập nếu cần thiết, và quản lý các sự kiện liên quan đến phiên đăng nhập thông qua máy trạng thái đã được thiết lập trước đó. Component này sử dụng các hook của Next.js và NextAuth để quản lý trạng thái và điều hướng.
export const AuthSessionListener = () => {
    const router = useRouter();
    const pathname = usePathname();
    const { status } = useSession();
    const isSigningOutRef = useRef(false);

    useEffect(() => {
        startAuthSessionRuntime();
    }, []);

    useEffect(() => {
        if (status === "authenticated") {
            sendAuthSessionEvent({ type: "SESSION_CONFIRMED" });
        }
    }, [status]);

    useEffect(() => {
        return subscribeAuthSession((snapshot) => {
            if (!snapshot.context.shouldRedirectToLogin) {
                return;
            }

            if (isSigningOutRef.current) {
                return;
            }

            isSigningOutRef.current = true;

            (async () => {
                try {
                    await signOut({ redirect: false });
                } finally {
                    if (pathname !== "/auth/login") {
                        router.replace("/auth/login");
                    }

                    sendAuthSessionEvent({ type: "REDIRECT_HANDLED" });
                    isSigningOutRef.current = false;
                }
            })();
        });
    }, [pathname, router]);

    return null;
};
