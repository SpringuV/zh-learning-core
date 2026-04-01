"use client";
import { AlertCircle, Eye, EyeOff, Lock, Mail, User } from "lucide-react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import React, { ChangeEvent } from "react";
import { Input } from "@/shared/components/ui/input";
import zod, * as z from "zod";
import { useRouter } from "next/navigation";
import { useErrorStore } from "@/store";
import { toast } from "sonner";
import { LoginRequest } from "@/modules/auth/types/auth.inteface";
import { signIn } from "next-auth/react";

// Determine login type based on input

function AuthLogo() {
    return (
        <div className="flex flex-col items-center gap-3 mb-8">
            <div className="size-16 rounded-2xl bg-primary flex items-center justify-center shadow-lg shadow-primary/20">
                <span className="text-primary-foreground font-bold text-3xl">
                    汉
                </span>
            </div>
            <div className="text-center">
                <h1 className="text-2xl font-bold">HànZì</h1>
            </div>
        </div>
    );
}
const loginSchema = z.object({
    Username: z
        .string()
        .min(1, "Vui lòng nhập email, tên đăng nhập hoặc số điện thoại")
        .min(3, "Phải ít nhất 3 ký tự")
        .max(100, "Tối đa 100 ký tự"),
    Password: z
        .string()
        .min(1, "Vui lòng nhập mật khẩu")
        .min(5, "Mật khẩu phải ít nhất 5 ký tự"),
});

export const LoginComponent = () => {
    const redirectUrl = "/"; // URL to redirect after successful login
    const urlRegister = "/auth/register";
    const router = useRouter();
    const [loginRequest, setLoginRequest] = React.useState<LoginRequest>({
        Username: "",
        Password: "",
        TypeLogin: "Username",
    });
    const [showPwd, setShowPwd] = React.useState(false);

    // zustand store for error handling
    const error = useErrorStore((state) => state.error);
    const setError = useErrorStore((state) => state.setError);

    const onNavigate = (path: string) => {
        window.dispatchEvent(new CustomEvent("navigate", { detail: path }));
    };
    const onAuth = () => {};
    const handleSubmit = async (e: ChangeEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError(null);

        // Validate using Zod
        const validated = loginSchema.safeParse({
            Username: loginRequest.Username.trim(),
            Password: loginRequest.Password,
        });

        if (!validated.success) {
            // Use treeifyError instead of deprecated flatten
            const fieldErrors = zod.treeifyError(validated.error).errors;
            setError(fieldErrors?.[0] || "Vui lòng kiểm tra lại thông tin");
            return;
        }
        if (loginRequest.Username.includes("@gmail.com")) {
            loginRequest.TypeLogin = "Email";
        } else if (/^\d+$/.test(loginRequest.Username)) {
            loginRequest.TypeLogin = "Phone";
        } else {
            loginRequest.TypeLogin = "Username";
        }

        const value: LoginRequest = {
            Username: validated.data.Username,
            Password: validated.data.Password,
            TypeLogin: loginRequest.TypeLogin,
        };

        const result = await signIn("credentials", {
            nameAccount: value.Username,
            password: value.Password,
            typeLogin: value.TypeLogin,
            redirect: false,
        });

        if (!result?.error) {
            toast.success("Đăng nhập thành công! Đang chuyển hướng...");
            router.replace(redirectUrl);
            router.refresh();
        } else {
            const errorMessage = result.error || "Đăng nhập thất bại";
            console.error("Login failed:", errorMessage);
            toast.error(errorMessage);
        }
    };

    return (
        <div className="w-full max-w-md mx-auto h-full flex items-center justify-center">
            <Card className="bg-card border-border">
                <CardContent className="px-6 lg:px-8">
                    <div className="flex gap-3 items-center">
                        <div className="w-[30%]">
                            <AuthLogo />
                        </div>
                        <div className="flex items-center flex-col">
                            <h2 className="text-xl font-bold mb-1">
                                Chào mừng trở lại
                            </h2>
                            <p className="text-sm text-muted-foreground mb-6 text-center">
                                Đăng nhập để tiếp tục hành trình học tập của bạn
                            </p>
                        </div>
                    </div>

                    {error && (
                        <div className="flex items-center gap-2 bg-primary/10 border border-primary/20 text-primary rounded-lg p-3 mb-4 text-sm">
                            <AlertCircle className="size-4 shrink-0" />
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSubmit} className="space-y-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium">
                                Email | Tên tài khoản | Số điện thoại
                            </label>
                            <div className="relative mt-2">
                                <User className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
                                <Input
                                    type="text"
                                    placeholder="ban@vidu.com"
                                    className="pl-10 hover:bg-muted transition-colors duration-300 border-border"
                                    value={loginRequest.Username}
                                    onChange={(e) => {
                                        setLoginRequest({
                                            ...loginRequest,
                                            Username: e.target.value,
                                        });
                                        setError("");
                                    }}
                                />
                            </div>
                        </div>
                        <div className="space-y-2">
                            <div className="flex items-center justify-between">
                                <label className="text-sm font-medium">
                                    Password
                                </label>
                                <button
                                    type="button"
                                    onClick={() =>
                                        onNavigate("forgot-password")
                                    }
                                    tabIndex={-1} // Prevent focus on this button
                                    className="text-xs text-primary hover:underline"
                                >
                                    Quên mật khẩu?
                                </button>
                            </div>
                            <div className="relative">
                                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
                                <Input
                                    type={showPwd ? "text" : "password"}
                                    placeholder="Nhập mật khẩu"
                                    className="pl-10 pr-10 border-border hover:bg-muted transition-colors duration-300"
                                    value={loginRequest.Password}
                                    onChange={(e) => {
                                        setLoginRequest({
                                            ...loginRequest,
                                            Password: e.target.value,
                                        });
                                        setError("");
                                    }}
                                />
                                <button
                                    type="button"
                                    onClick={() => setShowPwd(!showPwd)}
                                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                                >
                                    {showPwd ? (
                                        <EyeOff className="size-4" />
                                    ) : (
                                        <Eye className="size-4" />
                                    )}
                                </button>
                            </div>
                        </div>
                        <Button type="submit" className="w-full" size="lg">
                            Đăng nhập
                        </Button>
                    </form>

                    <div className="mt-4 text-center">
                        <p className="text-sm text-muted-foreground">
                            {"Chưa có tài khoản? "}
                            <button
                                onClick={() => router.push(urlRegister)}
                                className="text-primary hover:underline font-medium"
                            >
                                Đăng ký
                            </button>
                        </p>
                    </div>

                    <div className="relative my-6">
                        <div className="absolute inset-0 flex items-center">
                            <div className="w-full border-t border-border" />
                        </div>
                        <div className="relative flex justify-center text-xs uppercase">
                            <span className="bg-card px-3 text-muted-foreground">
                                hoặc đăng nhập bằng
                            </span>
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-3">
                        <Button
                            variant="outline"
                            className="w-full"
                            onClick={onAuth}
                        >
                            <svg
                                className="size-4 mr-2"
                                viewBox="0 0 24 24"
                                fill="none"
                            >
                                <path
                                    d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                                    fill="#4285F4"
                                />
                                <path
                                    d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                                    fill="#34A853"
                                />
                                <path
                                    d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                                    fill="#FBBC05"
                                />
                                <path
                                    d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                                    fill="#EA4335"
                                />
                            </svg>
                            Google
                        </Button>
                        <Button
                            variant="outline"
                            className="w-full"
                            onClick={onAuth}
                        >
                            <svg
                                className="size-4 mr-2"
                                viewBox="0 0 24 24"
                                fill="currentColor"
                            >
                                <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
                            </svg>
                            Facebook
                        </Button>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
};
