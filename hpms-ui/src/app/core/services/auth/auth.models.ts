export interface LoginRequest {
  username: string;
  password: string;
  rememberMe?: boolean;
}

export interface LoginResponse {
  token: string;
}

export interface UserRegistrationDto {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  tenantId: string;
  roleId: number;
  password: string;
}
