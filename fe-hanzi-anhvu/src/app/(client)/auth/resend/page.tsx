import { ResendClient } from "@/app/(client)/auth/resend/resend-client";
import { Metadata } from "next";

type ResendActivationPageProps = {
    searchParams?: Promise<Record<string, string | string[] | undefined>>;
};

export const metadata: Metadata = {
    title: "Gửi lại email kích hoạt",
    description: "Gửi lại email kích hoạt tài khoản",
};

export default async function ResendActivationPage({
    searchParams,
}: ResendActivationPageProps) {
    const params = (await searchParams) ?? {};
    const accountParam = params.account;
    const initialAccount =
        typeof accountParam === "string"
            ? accountParam.trim()
            : Array.isArray(accountParam)
              ? (accountParam[0] ?? "").trim()
              : "";

    return <ResendClient initialAccount={initialAccount} />;
}
