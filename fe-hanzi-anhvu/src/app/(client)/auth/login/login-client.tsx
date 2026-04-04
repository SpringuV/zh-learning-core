"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";

interface LoginPageClientProps {
    isAuthenticated: boolean;
}

export function LoginPageClient({ isAuthenticated }: LoginPageClientProps) {
    const router = useRouter();

    useEffect(() => {
        if (isAuthenticated) {
            router.push("/");
        }
    }, [isAuthenticated, router]);

    return (
        <div className="flex h-screen flex-col items-center justify-center gap-6 bg-linear-to-br from-white to-slate-50">
            {/* Spinner */}
            <div className="relative w-16 h-16">
                <div className="absolute inset-0 rounded-full border-4 border-slate-200"></div>
                <div className="absolute inset-0 rounded-full border-4 border-transparent border-t-primary border-r-primary animate-spin"></div>
            </div>

            {/* Text */}
            <p className="text-lg font-semibold text-slate-700">
                Đang chuyển hướng...
            </p>

            {/* Progress Bar */}
            <div className="w-48 h-1.5 bg-slate-200 rounded-full overflow-hidden shadow-sm">
                <div className="h-full bg-linear-to-r from-primary to-blue-500 rounded-full animate-pulse"></div>
            </div>
        </div>
    );
}
