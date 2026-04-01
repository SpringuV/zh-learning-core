import { Metadata } from "next";
import { RegisterComponent } from "@/modules/auth/components/register";

export const metadata: Metadata = {
    title: "Dang ky",
    description: "Tao tai khoan moi",
};

export default function RegisterPage() {
    return <RegisterComponent />;
}
