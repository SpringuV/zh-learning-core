// Client-side HTTP utility class for API calls with automatic token refresh
import axios, { AxiosInstance, AxiosResponse, AxiosError } from "axios";

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
        resolve: (value?: any) => void; // tạo resolve và reject để xử lý các request bị lỗi trong khi đang refresh token
        reject: (error?: any) => void;
    }> = [];

    // Constructor: Create axios instance and setup interceptors
    constructor() {
        enableDevSelfSignedTlsForLocalApi();

        // Create axios instance with base config
        this.instance = axios.create({
            baseURL: API_BASE_URL, // Set base URL
            timeout: 60 * 1000, // 60 second timeout
            withCredentials: true, // Send cookies with requests
        });

        // Setup response interceptor for handling 401 errors
        this.setupInterceptors();
    }

    // Private method to setup axios interceptors
    private setupInterceptors() {
        // Response interceptor to handle successful and error responses
        this.instance.interceptors.response.use(
            // On success: return response as is
            (response: AxiosResponse) => response,
            // On error: handle auth failures for token refresh
            async (error: AxiosError) => {
                // Get original request config
                const originalRequest = error.config;
                // Check if error is auth-related, has config, and not already retried
                if (
                    (error.response?.status === 401 ||
                        error.response?.status === 403) &&
                    originalRequest &&
                    !originalRequest._retry
                ) {
                    // If already refreshing, queue this request
                    if (this.isRefreshing) {
                        // Return promise that resolves after refresh completes
                        return new Promise((resolve, reject) => {
                            this.failedQueue.push({ resolve, reject });
                        })
                            .then(() => {
                                // Retry original request after refresh
                                return this.instance(originalRequest);
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

                    try {
                        // Call backend refresh endpoint
                        await axios.post(
                            `${API_BASE_URL}/auth/v1/refresh-token`, // Refresh API URL
                            {}, // Empty body
                            {
                                withCredentials: true, // Send refresh cookie
                            },
                        );

                        // In browser runtime, Set-Cookie is applied by the browser automatically
                        // when CORS + credentials are configured correctly.

                        // Process queued requests
                        this.failedQueue.forEach(({ resolve }) => {
                            // Resolve each queued request with retried call
                            resolve(this.instance(originalRequest));
                        });
                        // Clear queue
                        this.failedQueue = [];

                        // Retry the original failed request
                        return this.instance(originalRequest);
                    } catch (refreshError) {
                        // If refresh fails, reject all queued requests
                        this.failedQueue.forEach(({ reject }) => {
                            reject(refreshError);
                        });
                        // Clear queue
                        this.failedQueue = [];
                        // Redirect to login page
                        if (typeof window !== "undefined") {
                            window.location.href = "/auth/login";
                        }
                        // Reject the error
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
