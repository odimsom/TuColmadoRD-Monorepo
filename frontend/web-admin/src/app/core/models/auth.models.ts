export interface AuthUser {
  id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  role: string | null;
  tenantId: string | null;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  tenantName: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  token?: string;
  accessToken?: string;
  tenantId?: string;
  user?: AuthUser | null;
}