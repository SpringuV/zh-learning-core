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
        "/cms/:path*", // Only protect admin CMS routes
        "/client/:path*", // Optional: protect client routes if needed
    ],
};
