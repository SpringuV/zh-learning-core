import { auth } from "@/auth";
import type { Session } from "next-auth";
import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";

type AuthRequest = NextRequest & {
    auth: Session | null;
};

export default auth((req: AuthRequest) => {
    if (!req.auth) {
        const identityCookie = req.cookies.get("HanziAnhVu.Identity")?.value;
        const refreshCookie = req.cookies.get("HanziAnhVu.Refresh")?.value;

        // Nếu còn backend auth cookies nhưng mất NextAuth session, thử restore trước.
        if (identityCookie || refreshCookie) {
            const restoreUrl = new URL(
                "/api/auth/restore-session",
                req.nextUrl,
            );
            restoreUrl.searchParams.set(
                "callbackUrl",
                `${req.nextUrl.pathname}${req.nextUrl.search}`,
            );
            return Response.redirect(restoreUrl);
        }

        // Không có cookie backend -> về trang login.
        const loginUrl = new URL("/auth/login", req.nextUrl);
        loginUrl.searchParams.set(
            "callbackUrl",
            `${req.nextUrl.pathname}${req.nextUrl.search}`,
        );
        return Response.redirect(loginUrl);
    }

    const requestHeaders = new Headers(req.headers);
    // gắn thêm header để truyền pathname hiện tại vào API route, giúp API route xác định được context đang ở đâu
    // (vd: đang ở trang nào) để có thể đưa ra response phù hợp nếu cần, ví dụ: nếu đang ở trang CMS thì API
    //  có thể trả về thêm thông tin liên quan đến CMS, hoặc nếu đang ở trang client thì API có thể trả về
    // thông tin phù hợp với client. Đây là một cách để bridge thông tin giữa frontend và backend trong
    // trường hợp backend cần biết context của request để xử lý đúng.
    requestHeaders.set("x-pathname", req.nextUrl.pathname);

    return NextResponse.next({
        request: {
            headers: requestHeaders,
        },
    });
});

// Read more: https://nextjs.org/docs/app/building-your-application/routing/middleware#matcher
export const config = {
    matcher: [
        "/cms/:path*", // Only protect admin CMS routes
        "/client/:path*", // Optional: protect client routes if needed
    ],
};
