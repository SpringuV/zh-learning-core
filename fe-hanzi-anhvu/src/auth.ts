import NextAuth, { AuthError } from "next-auth";
import "next-auth/jwt";

import Credentials from "next-auth/providers/credentials";
import Facebook from "next-auth/providers/facebook";
import Google from "next-auth/providers/google";
import { AxiosError } from "axios";
import { parseAndSetCookie } from "@/lib/server-cookies";
import { TypeLogin } from "@/types/auth.inteface";
class customError extends AuthError {
    constructor(message: string) {
        super();
        this.message = message;
    }
}

export const { handlers, auth, signIn, signOut } = NextAuth({
    trustHost: true,
    // debug: !!process.env.AUTH_DEBUG,
    // basePath: "/api/auth", // Đảm bảo khớp với đường dẫn API của bạn
    providers: [
        Credentials({
            name: "credentials",
            credentials: {
                nameAccount: { label: "NameAccount", type: "text" },
                password: { label: "Password", type: "password" },
                typeLogin: { label: "TypeLogin", type: "text" },
            },
            async authorize(credentials) {
                // insure credentials exist
                if (!credentials) return null;

                const nameAccount = credentials.nameAccount;
                const password = credentials.password;
                const typeLogin = credentials.typeLogin;

                if (
                    typeof nameAccount !== "string" ||
                    typeof password !== "string" ||
                    typeof typeLogin !== "string"
                )
                    return null;

                // Call backend .NET login API
                try {
                    const { authApi } = await import("@/utils/auth.api");
                    // console.log("Attempting login with", nameAccount);
                    const res = await authApi.Login({
                        Username: nameAccount,
                        Password: password,
                        TypeLogin: typeLogin as TypeLogin,
                    });

                    await parseAndSetCookie(res); // Parse and set cookies from backend response

                    return {
                        id: res.data.userId,
                        name: res.data.userName,
                        roles: res.data.roles,
                    };
                } catch (err) {
                    console.error("Login error:", err);
                    if (err instanceof AxiosError) {
                        console.error("Login API error:", {
                            status: err.response?.status,
                            statusText: err.response?.statusText,
                            data: err.response?.data,
                        });
                        throw new customError(
                            err.response?.data?.message || "Đăng nhập thất bại",
                        );
                    }
                }

                return null; // Login failed
            },
        }),
        Facebook({
            clientId: process.env.AUTH_FACEBOOK_ID!,
            clientSecret: process.env.AUTH_FACEBOOK_SECRET!,
        }),
        Google({
            clientId: process.env.AUTH_GOOGLE_ID!,
            clientSecret: process.env.AUTH_GOOGLE_SECRET!,
        }),
    ],
    session: { strategy: "jwt" },
    callbacks: {
        jwt({ token, user, account }) {
            if (user) {
                token.roles = user.roles;
                token.sub = user.id;
            }
            // For OAuth providers, use account.access_token as accessToken
            if (account?.access_token) {
                token.accessToken = account.access_token;
            }
            return token;
        },
        async session({ session, token }) {
            session.user.roles = token.roles;
            session.user.userId = token.sub || "";
            session.user.userName = token.name || "";
            return session;
        },
    },
    secret: process.env.NEXTAUTH_SECRET,
});

/*
1. httpOnly
Ý nghĩa: Cookie chỉ có thể được truy cập bởi server (backend), không thể đọc hoặc sửa đổi qua JavaScript trên client-side.
Bảo mật: Ngăn chặn tấn công XSS (Cross-Site Scripting) vì attacker không thể steal cookie qua script.
Khi nào set true: Luôn set true cho authentication cookies để bảo mật. Backend thường set HttpOnly cho JWT cookies.
2. secure
Ý nghĩa: Cookie chỉ được gửi qua kết nối HTTPS an toàn, không gửi qua HTTP.
Bảo mật: Ngăn chặn man-in-the-middle attacks có thể intercept cookie trên HTTP.
Khi nào set true:
Trong production (khi NODE_ENV === "production")
Khi backend set attribute Secure trong Set-Cookie header
Nếu API dùng HTTPS (như https://localhost:1907/api)
Trong code hiện tại, secure được set true nếu backend có attribute Secure, hoặc có thể thêm logic để luôn true trong production.

3. sameSite
Ý nghĩa: Kiểm soát khi cookie được gửi trong cross-site requests.
Các giá trị:
"strict": Chỉ gửi cookie trong same-site requests (an toàn nhất, nhưng có thể block một số legitimate requests)
"lax": Gửi cookie trong top-level navigation (như click link từ site khác) và same-site requests
"none": Luôn gửi cookie, nhưng phải có secure: true (cho third-party cookies)
Khi nào dùng gì:
"lax" là mặc định an toàn cho authentication
"strict" nếu muốn cực kỳ bảo mật
"none" hiếm khi dùng, chỉ cho cross-site needs
Trong code, sameSite được parse từ backend, mặc định "lax" nếu không có.
*/
