"use client";

import React from "react";
import Link from "next/link";
import { Eye, EyeOff, Lock, Mail, User } from "lucide-react";
import * as z from "zod";
import { AxiosError } from "axios";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { authApi } from "@/utils/auth.api";
import { RegisterRequest } from "@/types/auth.inteface";

const registerSchema = z
    .object({
        Email: z.email("Email không hợp lệ"),
        Username: z
            .string()
            .min(3, "Username phải ít nhất 3 ký tự")
            .max(50, "Username tối đa 50 ký tự"),
        Password: z
            .string()
            .min(6, "Password phải ít nhất 6 ký tự")
            .max(100, "Password tối đa 100 ký tự"),
        ConfirmPassword: z
            .string()
            .min(6, "Nhập lại mật khẩu phải ít nhất 6 ký tự")
            .max(100, "Nhập lại mật khẩu tối đa 100 ký tự"),
    })
    .refine((data) => data.Password === data.ConfirmPassword, {
        message: "Mật khẩu nhập lại không khớp",
        path: ["ConfirmPassword"],
    });

export const RegisterComponent = () => {
    const router = useRouter();
    const [showPwd, setShowPwd] = React.useState(false);
    const [showConfirmPwd, setShowConfirmPwd] = React.useState(false);
    const [isLoading, setIsLoading] = React.useState(false);
    const [confirmPassword, setConfirmPassword] = React.useState("");
    const [payload, setPayload] = React.useState<RegisterRequest>({
        Email: "",
        Username: "",
        Password: "",
    });

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const validated = registerSchema.safeParse({
            Email: payload.Email.trim(),
            Username: payload.Username.trim(),
            Password: payload.Password,
            ConfirmPassword: confirmPassword,
        });

        if (!validated.success) {
            const issue = validated.error.issues[0];
            toast.error(issue?.message || "Vui lòng kiểm tra thông tin");
            return;
        }

        setIsLoading(true);
        try {
            await authApi.Register({
                Email: validated.data.Email,
                Username: validated.data.Username,
                Password: validated.data.Password,
            });
            toast.success("Đăng ký thành công. Vui lòng đăng nhập.");
            setTimeout(() => {
                router.push("/auth/login");
            }, 3000);
        } catch (error) {
            if (error instanceof AxiosError) {
                const message =
                    error.response?.data?.message ||
                    error.response?.data?.error ||
                    "Đăng ký thất bại";
                toast.error(message);
            } else {
                toast.error("Đăng ký thất bại");
            }
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="mx-auto flex h-full w-full max-w-md items-center justify-center">
            <Card className="w-full border-border bg-card">
                <CardContent className="px-6 py-8 lg:px-8">
                    <div className="mb-6 text-center">
                        <div className="mx-auto mb-3 flex size-14 items-center justify-center rounded-2xl bg-primary text-2xl font-bold text-primary-foreground shadow-lg shadow-primary/20">
                            汉
                        </div>
                        <h1 className="text-2xl font-bold">Tạo tài khoản</h1>
                        <p className="mt-1 text-sm text-muted-foreground">
                            Bắt đầu lộ trình học tiếng Trung ngay hôm nay
                        </p>
                    </div>

                    <form onSubmit={handleSubmit} className="space-y-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium">Email</label>
                            <div className="relative">
                                <Mail className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                                <Input
                                    type="email"
                                    placeholder="ban@vidu.com"
                                    className="pl-10"
                                    value={payload.Email}
                                    onChange={(e) =>
                                        setPayload((prev) => ({
                                            ...prev,
                                            Email: e.target.value,
                                        }))
                                    }
                                    required
                                />
                            </div>
                        </div>

                        <div className="space-y-2">
                            <label className="text-sm font-medium">
                                Username
                            </label>
                            <div className="relative">
                                <User className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                                <Input
                                    type="text"
                                    placeholder="Tên đăng nhập"
                                    className="pl-10"
                                    value={payload.Username}
                                    onChange={(e) =>
                                        setPayload((prev) => ({
                                            ...prev,
                                            Username: e.target.value,
                                        }))
                                    }
                                    required
                                />
                            </div>
                        </div>

                        <div className="space-y-2">
                            <label className="text-sm font-medium">
                                Password
                            </label>
                            <div className="relative">
                                <Lock className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                                <Input
                                    type={showPwd ? "text" : "password"}
                                    placeholder="Nhập mật khẩu"
                                    className="pl-10 pr-10"
                                    value={payload.Password}
                                    onChange={(e) =>
                                        setPayload((prev) => ({
                                            ...prev,
                                            Password: e.target.value,
                                        }))
                                    }
                                    required
                                />
                                <button
                                    type="button"
                                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground"
                                    onClick={() => setShowPwd((prev) => !prev)}
                                >
                                    {showPwd ? (
                                        <EyeOff className="size-4" />
                                    ) : (
                                        <Eye className="size-4" />
                                    )}
                                </button>
                            </div>
                        </div>

                        <div className="space-y-2">
                            <label className="text-sm font-medium">
                                Nhập lại mật khẩu
                            </label>
                            <div className="relative">
                                <Lock className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                                <Input
                                    type={showConfirmPwd ? "text" : "password"}
                                    placeholder="Nhập lại mật khẩu"
                                    className="pl-10 pr-10"
                                    value={confirmPassword}
                                    onChange={(e) =>
                                        setConfirmPassword(e.target.value)
                                    }
                                    required
                                />
                                <button
                                    type="button"
                                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground"
                                    onClick={() =>
                                        setShowConfirmPwd((prev) => !prev)
                                    }
                                >
                                    {showConfirmPwd ? (
                                        <EyeOff className="size-4" />
                                    ) : (
                                        <Eye className="size-4" />
                                    )}
                                </button>
                            </div>
                        </div>

                        <Button
                            type="submit"
                            className="w-full"
                            size="lg"
                            disabled={isLoading}
                        >
                            {isLoading ? "Đang xử lý..." : "Đăng ký"}
                        </Button>
                    </form>

                    <p className="mt-5 text-center text-sm text-muted-foreground">
                        Đã có tài khoản?{" "}
                        <Link
                            href="/auth/login"
                            className="font-medium text-primary hover:underline"
                        >
                            Đăng nhập
                        </Link>
                    </p>
                </CardContent>
            </Card>
        </div>
    );
};
