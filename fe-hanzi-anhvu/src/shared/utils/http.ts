// Client-side HTTP utility class for API calls with automatic token refresh
import axios, { AxiosInstance, AxiosResponse, AxiosError } from "axios";
import { sendAuthSessionEvent } from "@/modules/auth/machines/auth-session/auth.session.runtime";

export const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

let tlsBypassInitialized = false;

function enableDevSelfSignedTlsForLocalApi() {
    // Only run in Node runtime (server actions, NextAuth callbacks).
    if (typeof window !== "undefined" || tlsBypassInitialized) return;
    if (process.env.NODE_ENV === "production" || !API_BASE_URL) return;

    try {
        const parsed = new URL(API_BASE_URL);
        const isLocalHttps =
            parsed.protocol === "https:" &&
            (parsed.hostname === "localhost" ||
                parsed.hostname === "127.0.0.1");

        if (isLocalHttps) {
            process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
        }
    } catch {
        // Ignore invalid URL format, axios will surface actual request errors later.
    }

    tlsBypassInitialized = true;
}

// Extend InternalAxiosRequestConfig to include _retry property
declare module "axios" {
    interface InternalAxiosRequestConfig {
        _retry?: boolean;
    }
}

// Http class to manage axios instance with interceptors
class Http {
    instance: AxiosInstance;
    // Flag to prevent multiple refresh calls
    private isRefreshing = false;
    // Queue for failed requests during refresh
    private failedQueue: Array<{
        resolve: (config: any) => void;
        reject: (error?: any) => void;
        requestConfig: any;
    }> = [];

    private processQueue(error: any = null) {
        this.failedQueue.forEach(({ resolve, reject, requestConfig }) => {
            if (error) {
                reject(error);
                return;
            }
            resolve(requestConfig);
        });
        this.failedQueue = [];
    }

    // Constructor: Create axios instance and setup interceptors
    constructor() {
        enableDevSelfSignedTlsForLocalApi();

        const baseURL = API_BASE_URL;

        this.instance = axios.create({
            baseURL,
            timeout: 60 * 1000, // 60 second timeout
            withCredentials: true, // Send cookies with requests
        });

        // Setup response interceptor for handling 401 errors
        this.setupInterceptors();
    }

    // Private method to setup axios interceptors
    private setupInterceptors() {
        // ============= Response Interceptor =============
        // Handle 401 as fallback (reactive approach)
        this.instance.interceptors.response.use(
            // On success: return response as is
            (response: AxiosResponse) => response,
            // On error: handle auth failures for token refresh
            async (error: AxiosError) => {
                // Get original request config
                const originalRequest = error.config;
                const isRefreshEndpoint =
                    originalRequest?.url?.includes("/auth/v1/refresh-token") ??
                    false;
                // Check if error is auth-related, has config, and not already retried
                if (
                    (error.response?.status === 401 ||
                        error.response?.status === 403) &&
                    originalRequest &&
                    !isRefreshEndpoint &&
                    !originalRequest._retry
                ) {
                    // If already refreshing, queue this request
                    if (this.isRefreshing) {
                        // Return promise that resolves after refresh completes
                        return new Promise((resolve, reject) => {
                            this.failedQueue.push({
                                resolve,
                                reject,
                                requestConfig: originalRequest,
                            });
                        })
                            .then((requestConfig: any) => {
                                // Retry request after refresh
                                return this.instance(requestConfig);
                            })
                            .catch((err) => {
                                // Reject if queue processing fails
                                return Promise.reject(err);
                            });
                    }

                    // Mark request as retried to prevent infinite loop
                    originalRequest._retry = true;
                    // Set refreshing flag
                    this.isRefreshing = true;
                    sendAuthSessionEvent({ type: "TOKEN_REFRESH_STARTED" });

                    try {
                        // Refresh using HttpOnly cookies; backend will rotate cookies.
                        await this.instance.post(
                            "/auth/v1/refresh-token",
                            {},
                            {
                                withCredentials: true,
                            },
                        );

                        // Process queued requests after successful refresh
                        this.processQueue(null);
                        sendAuthSessionEvent({
                            type: "TOKEN_REFRESH_SUCCEEDED",
                        });

                        // Retry the original failed request
                        return this.instance(originalRequest);
                    } catch (refreshError) {
                        // If refresh fails, reject all queued requests
                        this.processQueue(refreshError);
                        sendAuthSessionEvent({
                            type: "TOKEN_REFRESH_FAILED",
                            reason:
                                refreshError instanceof Error
                                    ? refreshError.message
                                    : "Refresh token failed",
                        });
                        // Return 401 error to allow proper handling
                        return Promise.reject(refreshError);
                    } finally {
                        // Reset refreshing flag
                        this.isRefreshing = false;
                    }
                }

                // If not 401 or already handled, reject error
                return Promise.reject(error);
            },
        );
    }
}

// Export default instance
const http = new Http().instance;

export default http;
