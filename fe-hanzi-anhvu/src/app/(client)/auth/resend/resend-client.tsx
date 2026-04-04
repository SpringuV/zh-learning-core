"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { Loader2, Mail, RefreshCcw } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import {
    Card,
    CardContent,
    CardHeader,
    CardTitle,
} from "@/shared/components/ui/card";
import { Input } from "@/shared/components/ui/input";
import { useResend, UseResendOptions } from "@/modules/auth/hooks/use-resend";

type ResendClientProps = {
    initialAccount?: string;
};

export function ResendClient({ initialAccount = "" }: ResendClientProps) {
    const [accountName, setAccountName] = useState(initialAccount);

    const userResentRequest: UseResendOptions = useMemo(() => {
        const typeUsername = accountName.includes("@")
            ? "Email"
            : /^\d+$/.test(accountName)
              ? "Phone"
              : "Username";

        return {
            account: accountName,
            typeUsername,
        };
    }, [accountName]);

    const { status, message, isLoading, resend, reset } =
        useResend(userResentRequest);

    return (
        <div className="mx-auto flex h-full w-full max-w-md items-center justify-center">
            <Card className="w-full border-border bg-card gap-2">
                <CardHeader className="space-y-3 pb-2">
                    <CardTitle className="text-center text-2xl">
                        Gửi lại email kích hoạt
                    </CardTitle>
                    <p className="text-center text-sm leading-relaxed text-muted-foreground">
                        Nhập tài khoản đã đăng ký để nhận lại liên kết kích
                        hoat. Nếu bạn không tìm thấy email, hãy kiểm tra thư mục
                        spam.
                    </p>
                </CardHeader>

                <CardContent className="space-y-5 px-6 pb-8 pt-2 lg:px-8">
                    <div className="space-y-2">
                        <label className="text-sm font-medium">Tài khoản</label>
                        <div className="relative">
                            <Mail className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                            <Input
                                type="text"
                                placeholder="Nhập email | Số điện thoại | Tên đăng nhập đăng ký"
                                className="pl-10 placeholder:text-muted-foreground/55"
                                value={accountName}
                                onChange={(e) => setAccountName(e.target.value)}
                            />
                        </div>
                    </div>

                    {message ? (
                        <div
                            className={`rounded-md border px-3 py-2 text-sm ${
                                status === "success"
                                    ? "border-emerald-500/40 bg-emerald-500/10 text-emerald-700"
                                    : status === "error"
                                      ? "border-red-500/40 bg-red-500/10 text-red-700"
                                      : status === "warning"
                                        ? "border-amber-500/40 bg-amber-500/10 text-amber-700"
                                        : "border-slate-500/30 bg-slate-500/10 text-slate-700"
                            }`}
                        >
                            {message}
                        </div>
                    ) : null}

                    <div className="grid grid-cols-2 gap-3">
                        <Button
                            onClick={() => resend()}
                            disabled={isLoading}
                            className="w-full"
                        >
                            {isLoading ? (
                                <>
                                    <Loader2 className="mr-2 size-4 animate-spin" />
                                    Đang gửi...
                                </>
                            ) : (
                                <>
                                    <RefreshCcw className="mr-2 size-4" />
                                    Gửi lại email kích hoạt
                                </>
                            )}
                        </Button>

                        {status === "success" && (
                            <Button
                                variant="secondary"
                                className="w-full"
                                onClick={reset}
                            >
                                Gửi email khác
                            </Button>
                        )}

                        <Button asChild variant="outline" className="w-full">
                            <Link href="/auth/login">Quay về đăng nhập</Link>
                        </Button>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
