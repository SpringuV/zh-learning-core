"use client";

import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { RefreshCcw } from "lucide-react";
import { AuthStatusPanel } from "@/modules/auth/components/auth-status-panel";
import { useAuthActivationOrchestrator } from "@/modules/auth/hooks/use-auth-activation-orchestrator";
import { Button } from "@/shared/components/ui/button";
import {
    Card,
    CardContent,
    CardHeader,
    CardTitle,
} from "@/shared/components/ui/card";

export default function ActivatePage() {
    const searchParams = useSearchParams();
    const email = (searchParams.get("email") || "").trim();
    const code = (searchParams.get("code") || "").trim();
    const { status, message, canActivate, isLoading, activate } =
        useAuthActivationOrchestrator({
            mode: "activate",
            email,
            code,
        });

    return (
        <div className="mx-auto flex h-full w-full max-w-md items-center justify-center">
            <Card className="w-full border-border bg-card">
                <CardHeader className="pb-2">
                    <CardTitle className="text-center text-2xl">
                        Kích hoạt tài khoản
                    </CardTitle>
                </CardHeader>

                <CardContent className="space-y-5 px-6 pb-8 pt-2 lg:px-8">
                    <AuthStatusPanel status={status} message={message} />

                    <div className="grid grid-cols-1 gap-3">
                        {status !== "success" && (
                            <>
                                <Button
                                    onClick={activate}
                                    className="w-full"
                                    disabled={isLoading || !canActivate}
                                >
                                    <RefreshCcw className="mr-2 size-4" />
                                    {isLoading
                                        ? "Đang xác thực..."
                                        : "Kích hoạt ngay"}
                                </Button>

                                {(status === "error" ||
                                    status === "warning") && (
                                    <Button
                                        asChild
                                        variant="secondary"
                                        className="w-full"
                                    >
                                        <Link
                                            href={
                                                email
                                                    ? `/auth/resend?email=${encodeURIComponent(email)}`
                                                    : "/auth/resend"
                                            }
                                        >
                                            Gửi lại email kích hoạt
                                        </Link>
                                    </Button>
                                )}
                            </>
                        )}

                        <Button asChild className="w-full">
                            <Link href="/auth/login">Đi đến đăng nhập</Link>
                        </Button>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
