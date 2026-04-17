export interface LoginRequest {
  username: string;
  password: string;
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
