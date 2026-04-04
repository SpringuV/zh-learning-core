"use client";

import { useAuthLogout } from "@/modules/auth/hooks/use-auth-query";
import { LogOutIcon } from "lucide-react";
import { signOut } from "next-auth/react";
import { useRouter } from "next/navigation";

const LogoutComponent = () => {
    const authLogout = useAuthLogout();
    const router = useRouter();

    const handleLogout = async () => {
        try {
            await authLogout.mutateAsync();
        } catch (error) {
            if (error instanceof Error) {
                console.error("Backend logout failed", error.message);
            } else {
                console.error("Backend logout failed", error);
            }
        } finally {
            // Always clear NextAuth session cookie even if backend logout fails.
            // call logout api first to ensure backend session is cleared, then call NextAuth signOut to clear client session and cookie
            await signOut({
                redirect: false,
                callbackUrl: "/",
            });
            router.replace("/");
            router.refresh();
        }
    };

    return (
        <button
            className="flex items-center gap-2"
            type="button"
            onClick={handleLogout}
            disabled={authLogout.isPending}
        >
            <LogOutIcon />
            {authLogout.isPending ? "Đang đăng xuất..." : "Đăng xuất"}
        </button>
    );
};

export default LogoutComponent;
