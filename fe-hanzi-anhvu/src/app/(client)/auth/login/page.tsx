import { LoginComponent } from "@/components/auth/login";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Đăng nhập",
    description: "Đăng nhập vào hệ thống",
};

const LoginPage = () => {
    return <LoginComponent />;
};

export default LoginPage;
