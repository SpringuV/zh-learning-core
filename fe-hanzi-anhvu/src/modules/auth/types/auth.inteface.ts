export type TypeLogin = "Email" | "Username" | "Phone";
export interface LoginRequest {
    Username: string;
    Password: string;
    TypeLogin: TypeLogin;
}

export interface LoginResponse {
    message: string;
    userId: string;
    userName: string;
    roles: string[];
}

export interface RegisterRequest {
    Email: string;
    Username: string;
    Password: string;
}

export interface RegisterResponse {
    message: string;
}

export interface ActivateAccountRequest {
    Email: string;
    Code: string;
}

export interface ActivateAccountResponse {
    message?: string;
}

export interface ResendActivationRequest {
    Email: string;
}

export interface ResendActivationResponse {
    message?: string;
}
