"use client";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { RefreshCcw } from "lucide-react";
import { useActivateAccount } from "@/modules/auth/hooks/use-activate";
import { Button } from "@/shared/components/ui/button";
import {
    Card,
    CardContent,
    CardHeader,
    CardTitle,
} from "@/shared/components/ui/card";

export default function ActivatePage() {
    const searchParams = useSearchParams();
    const email = (searchParams.get("account") || "").trim();
    const code = (searchParams.get("code") || "").trim();
    const activeAccountHook = useActivateAccount({
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
                    <p className="mt-2 text-center text-sm text-muted-foreground">
                        Xác thực email để hoàn tất đăng ký và mở khóa đầy đủ
                        tính năng học tập.
                    </p>
                </CardHeader>

                <CardContent className="space-y-5 px-6 pb-8 pt-2 lg:px-8">
                    <div className="rounded-md border border-border/70 bg-muted/30 px-3 py-2 text-sm text-muted-foreground">
                        <p>
                            Email kich hoat:
                            <span className="ml-1 font-medium text-foreground">
                                {email || "Khong tim thay email"}
                            </span>
                        </p>
                        <p className="mt-1 text-xs">
                            Nếu liên kết hết hạn, bạn có thể gửi lại email kích
                            hoạt ở bên dưới.
                        </p>
                    </div>

                    {activeAccountHook.message ? (
                        <div
                            className={`rounded-md border px-3 py-2 text-sm ${
                                activeAccountHook.status === "success"
                                    ? "border-emerald-500/40 bg-emerald-500/10 text-emerald-700"
                                    : activeAccountHook.status === "error"
                                      ? "border-red-500/40 bg-red-500/10 text-red-700"
                                      : activeAccountHook.status === "warning"
                                        ? "border-amber-500/40 bg-amber-500/10 text-amber-700"
                                        : "border-slate-500/30 bg-slate-500/10 text-slate-700"
                            }`}
                        >
                            {activeAccountHook.message}
                        </div>
                    ) : null}

                    <p className="text-xs text-muted-foreground">
                        Bấm "Kích hoạt ngay" để xác thực mã trong liên kết kích
                        hoạt và hoàn tất quá trình đăng ký.
                    </p>

                    <div className="grid grid-cols-1 gap-3">
                        {activeAccountHook.status !== "success" && (
                            <>
                                <Button
                                    onClick={activeAccountHook.activate}
                                    className="w-full"
                                    disabled={
                                        activeAccountHook.isLoading ||
                                        !activeAccountHook.canActivate
                                    }
                                >
                                    <RefreshCcw className="mr-2 size-4" />
                                    {activeAccountHook.isLoading
                                        ? "Đang xác thực..."
                                        : "Kích hoạt ngay"}
                                </Button>

                                {(activeAccountHook.status === "error" ||
                                    activeAccountHook.status === "warning") && (
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
