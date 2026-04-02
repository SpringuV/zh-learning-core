"use client";

import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useState } from "react";
import { Loader2, Mail, RefreshCcw } from "lucide-react";
import { AuthStatusPanel } from "@/modules/auth/components/auth-status-panel";
import { Button } from "@/shared/components/ui/button";
import {
    Card,
    CardContent,
    CardHeader,
    CardTitle,
} from "@/shared/components/ui/card";
import { Input } from "@/shared/components/ui/input";
import { useAuthActivationOrchestrator } from "@/modules/auth/hooks/use-auth-activation-orchestrator";

export default function ResendActivationPage() {
    const searchParams = useSearchParams();
    const queryEmail = (searchParams.get("email") || "").trim();
    const [email, setEmail] = useState(queryEmail);
    const { status, message, isLoading, resend, resetToIdle } =
        useAuthActivationOrchestrator({
            mode: "resend",
            email: queryEmail,
        });

    return (
        <div className="mx-auto flex h-full w-full max-w-md items-center justify-center">
            <Card className="w-full border-border bg-card">
                <CardHeader className="pb-2">
                    <CardTitle className="text-center text-2xl">
                        Gửi lại email kích hoạt
                    </CardTitle>
                </CardHeader>

                <CardContent className="space-y-5 px-6 pb-8 pt-2 lg:px-8">
                    <div className="space-y-2">
                        <label className="text-sm font-medium">Email</label>
                        <div className="relative">
                            <Mail className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                            <Input
                                type="email"
                                placeholder="ban@vidu.com"
                                className="pl-10"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                            />
                        </div>
                    </div>

                    <AuthStatusPanel status={status} message={message} />

                    <div className="grid grid-cols-1 gap-3">
                        <Button
                            onClick={() => resend(email)}
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
                                onClick={resetToIdle}
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
