"use server";
import { cookies } from "next/headers";

// Server-only helper to parse and set cookies from Axios response headers.
export async function parseAndSetCookie(response: any): Promise<void> {
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

        cookieStore.set(name, value, options);
    }
}
