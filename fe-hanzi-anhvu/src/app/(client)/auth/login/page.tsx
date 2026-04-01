import { LoginComponent } from "@/modules/auth/components/login";
import { Metadata } from "next";
import { auth } from "@/auth";
import { LoginPageClient } from "@/app/(client)/auth/login/login-client";

export const metadata: Metadata = {
    title: "Đăng nhập",
    description: "Đăng nhập vào hệ thống",
};

export default async function LoginPage() {
    const session = await auth();

    if (session) {
        // Nếu đã đăng nhập, delegate redirect logic to client component
        return <LoginPageClient isAuthenticated={true} />;
    }

    return <LoginComponent />;
}
