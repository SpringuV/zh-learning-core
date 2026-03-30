// Client-side HTTP utility class for API calls with automatic token refresh
import { parseAndSetCookie } from "@/lib/server-cookies";
import axios, { AxiosInstance, AxiosResponse, AxiosError } from "axios";
import https from "https";

export const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

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
        // Create axios instance with base config
        this.instance = axios.create({
            baseURL: API_BASE_URL, // Set base URL
            timeout: 60 * 1000, // 60 second timeout
            withCredentials: true, // Send cookies with requests
            // For local development, disable SSL verification to allow self-signed certs
            httpsAgent:
                API_BASE_URL?.includes("localhost") ||
                API_BASE_URL?.includes("127.0.0.1")
                    ? new https.Agent({ rejectUnauthorized: false })
                    : undefined,
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
            // On error: handle 401 (unauthorized) for token refresh
            async (error: AxiosError) => {
                // Get original request config
                const originalRequest = error.config;
                // Check if error is 401, has config, and not already retried
                if (
                    error.response?.status === 401 &&
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
                        const refreshResponse = await axios.post(
                            `${API_BASE_URL}/auth/v1/refresh-token`, // Refresh API URL
                            {}, // Empty body
                            {
                                withCredentials: true, // Send refresh cookie
                            },
                        );
                        await parseAndSetCookie(refreshResponse); // Backend sets new access cookie, no need to store locally

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
                        window.location.href = "/auth/login";
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
