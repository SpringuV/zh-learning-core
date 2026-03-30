import { auth } from "@/auth";

export default auth((req: any) => {
    if (!req.auth) {
        // Redirect về login
        return Response.redirect(new URL("/auth/login", req.nextUrl));
    }
});

// Read more: https://nextjs.org/docs/app/building-your-application/routing/middleware#matcher
export const config = {
    matcher: [
        "/((?!api|_next/static|_next/image|favicon.ico|auth).*)", // Exclude auth routes from middleware
        "/admin/:path*",
        "/client/:path*",
    ],
};
