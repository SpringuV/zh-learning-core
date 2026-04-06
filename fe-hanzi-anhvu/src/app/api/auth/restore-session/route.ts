import { signIn } from "@/auth";
import { NextRequest, NextResponse } from "next/server";
import axios from "axios";
import { cookies } from "next/headers";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
const CookieNameIdentity = "HanziAnhVu.Identity";
const CookieNameRefresh = "HanziAnhVu.Refresh";
function normalizeCallbackUrl(rawCallbackUrl: string, requestUrl: URL): string {
    try {
        const parsed = new URL(rawCallbackUrl, requestUrl);
        if (parsed.origin !== requestUrl.origin) {
            return "/";
        }
        return `${parsed.pathname}${parsed.search}`;
    } catch {
        return "/";
    }
}

function getCookieValueFromHeader(
    cookieHeader: string | null | undefined,
    cookieName: string,
): string | null {
    if (!cookieHeader) return null;

    const cookiePairs = cookieHeader.split(";");
    for (const pair of cookiePairs) {
        const trimmed = pair.trim();
        if (trimmed.startsWith(`${cookieName}=`)) {
            return trimmed.substring(cookieName.length + 1);
        }
    }
    return null;
}

async function applySetCookieHeaders(
    setCookieHeader: string | string[] | undefined,
) {
    if (!setCookieHeader) return;

    const cookieStore = await cookies();
    const cookieStrings = Array.isArray(setCookieHeader)
        ? setCookieHeader
        : [setCookieHeader];

    for (const cookieStr of cookieStrings) {
        const parts = cookieStr.split(";").map((p) => p.trim());
        const nameValue = parts[0];
        const eq = nameValue.indexOf("=");
        if (eq === -1) continue;

        const name = nameValue.substring(0, eq).trim();
        const value = nameValue.substring(eq + 1).trim();

        const options: Record<string, any> = {};
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

export async function GET(request: NextRequest) {
    const callbackFromQuery = request.nextUrl.searchParams.get("callbackUrl");
    const referer = request.headers.get("referer");

    let fallbackCallbackUrl = "/";
    if (referer) {
        try {
            const refererUrl = new URL(referer);
            if (refererUrl.origin === request.nextUrl.origin) {
                fallbackCallbackUrl = `${refererUrl.pathname}${refererUrl.search}`;
            }
        } catch {
            // Ignore invalid referer value and keep default fallback.
        }
    }

    const callbackUrl = normalizeCallbackUrl(
        callbackFromQuery || fallbackCallbackUrl,
        request.nextUrl,
    );
    const cookieHeader = request.headers.get("cookie");
    // Đầu tiên thử lấy access token từ cookie, nếu có thì restore session ngay mà không cần gọi API refresh.
    let identityToken = getCookieValueFromHeader(
        cookieHeader,
        CookieNameIdentity,
    );

    // Nếu chưa có access cookie nhưng còn refresh cookie, thử refresh server-side.
    if (!identityToken) {
        const refreshToken = getCookieValueFromHeader(
            cookieHeader,
            // hardcode tên cookie ở đây để tránh case sensitivity issue khi đọc từ header, vì cookie có thể được set với tên khác nhau (vd: HanziAnhVu.Identity vs hanzianhvu.identity) nhưng vẫn trùng khi đọc từ document.cookie ở client.
            CookieNameRefresh,
        );

        // Nếu có refresh token, gọi API refresh để lấy lại access token mới, đồng thời cập nhật
        // cookie trên response.
        if (refreshToken && API_BASE_URL) {
            try {
                const refreshResponse = await axios.post(
                    `${API_BASE_URL}/auth/v1/refresh-token`,
                    {},
                    {
                        headers: {
                            Cookie: cookieHeader ?? "",
                        },
                        withCredentials: true,
                    },
                );

                const setCookie = refreshResponse.headers?.["set-cookie"] as
                    | string
                    | string[]
                    | undefined;
                await applySetCookieHeaders(setCookie);

                const cookieStrings = Array.isArray(setCookie)
                    ? setCookie
                    : setCookie
                      ? [setCookie]
                      : [];
                const identitySetCookie = cookieStrings.find((c) =>
                    c.startsWith(CookieNameIdentity),
                );

                if (identitySetCookie) {
                    const firstPart = identitySetCookie.split(";")[0];
                    identityToken = firstPart.substring(
                        `${CookieNameIdentity}=`.length,
                    );
                }
            } catch {
                // Ignore refresh errors here, flow will fall back to login.
            }
        }
    }

    if (!identityToken) {
        const loginUrl = new URL("/auth/login", request.url);
        loginUrl.searchParams.set("callbackUrl", callbackUrl);
        return NextResponse.redirect(loginUrl);
    }

    try {
        await signIn("restore-session", {
            redirect: false,
            identityToken,
        });

        return NextResponse.redirect(new URL(callbackUrl, request.url));
    } catch {
        const loginUrl = new URL("/auth/login", request.url);
        loginUrl.searchParams.set("callbackUrl", callbackUrl);
        return NextResponse.redirect(loginUrl);
    }
}
