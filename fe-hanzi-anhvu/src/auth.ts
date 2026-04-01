import NextAuth, { AuthError } from "next-auth";
import "next-auth/jwt";

import Credentials from "next-auth/providers/credentials";
import Facebook from "next-auth/providers/facebook";
import Google from "next-auth/providers/google";
import { AxiosError } from "axios";
import { authApi } from "@/modules/auth/api/auth.api";
import { TypeLogin } from "@/modules/auth/types/auth.inteface";

class customError extends AuthError {
    constructor(message: string) {
        super();
        this.message = message;
    }
}

// Server-side only: Parse and set cookies from Axios response
// Must be called only from authorize callback (server-side)
async function parseAndSetServerCookie(response: any): Promise<void> {
    // Dynamic import - only loads on server
    const { cookies } = await import("next/headers");

    const setCookie = response?.headers?.["set-cookie"];
    if (!setCookie) return;

    const cookieStrings = Array.isArray(setCookie) ? setCookie : [setCookie];
    const cookieStore = await cookies();

    for (const cookieStr of cookieStrings) {
        const parts = cookieStr.split(";").map((p: string) => p.trim());
        const nameValue = parts[0];
        const eq = nameValue.indexOf("=");
        if (eq === -1) continue;

        const name = nameValue.substring(0, eq).trim();
        const value = nameValue.substring(eq + 1).trim();

        const options: any = {};
        for (let i = 1; i < parts.length; i++) {
            const attr = parts[i];
            const [k, ...vParts] = attr.split("=");
            const key = k.trim().toLowerCase();
            const val = vParts.join("=").trim();

            if (key === "path") {
                options.path = val || "/";
            } else if (key === "expires") {
                const t = Date.parse(val);
                if (!Number.isNaN(t)) options.expires = new Date(t);
            } else if (key === "secure") {
                options.secure = true;
            } else if (key === "httponly") {
                options.httpOnly = true;
            } else if (key === "samesite") {
                const sameSite = val.toLowerCase();
                if (
                    sameSite === "none" ||
                    sameSite === "lax" ||
                    sameSite === "strict"
                ) {
                    options.sameSite = sameSite;
                }
            }
        }

        if (!options.path) {
            options.path = "/";
        }

        cookieStore.set(name, value, options);
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
                    // console.log("Attempting login with", nameAccount);
                    const res = await authApi.Login({
                        Username: nameAccount,
                        Password: password,
                        TypeLogin: typeLogin as TypeLogin,
                    });

                    await parseAndSetServerCookie(res); // Parse and set cookies to Next.js server (persist across refreshes)

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
            session.user.id = token.sub || "";
            session.user.name = token.name || "";
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
