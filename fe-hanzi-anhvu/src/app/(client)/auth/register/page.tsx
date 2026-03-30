import { Metadata } from "next";
import { RegisterComponent } from "@/components/auth/register";

export const metadata: Metadata = {
    title: "Dang ky",
    description: "Tao tai khoan moi",
};

export default function RegisterPage() {
    return <RegisterComponent />;
}
