import { jwtDecode } from "jwt-decode";

interface JwtPayload {
    exp: number;
    iat: number;
    sub: string;
    name?: string;
    role?: string[];
    [key: string]: any;
}

/**
 * Decode JWT token và lấy expiry time
 * @param token JWT token string
 * @returns Expiry time in milliseconds, hoặc null nếu lỗi
 */
export function getTokenExpiry(token: string): number | null {
    try {
        const decoded = jwtDecode<JwtPayload>(token);
        // exp là Unix timestamp (seconds), convert to milliseconds
        return decoded.exp ? decoded.exp * 1000 : null;
    } catch (error) {
        console.error("[JWT] Failed to decode token:", error);
        return null;
    }
}

/**
 * Tính thời gian còn lại cho token
 * @param token JWT token string
 * @returns Milliseconds until expiry, 0 if expired or error
 */
export function getTimeUntilExpiry(token: string): number {
    const expiry = getTokenExpiry(token);
    if (!expiry) return 0;
    const remaining = expiry - Date.now();
    return remaining > 0 ? remaining : 0;
}

/**
 * Check xem token đã hết hạn chưa
 * @param token JWT token string
 * @returns true nếu token đã hết hạn
 */
export function isTokenExpired(token: string): boolean {
    return getTimeUntilExpiry(token) <= 0;
}

/**
 * Get decoded payload từ token
 * @param token JWT token string
 * @returns Decoded payload, hoặc null nếu lỗi
 */
export function getTokenPayload(token: string): JwtPayload | null {
    try {
        return jwtDecode<JwtPayload>(token);
    } catch (error) {
        console.error("[JWT] Failed to decode token:", error);
        return null;
    }
}
