import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { LoginRequest, LoginResponse, UserRegistrationDto } from './auth.models';
import { jwtDecode } from 'jwt-decode';
import { IDENTITY_API_URL } from '../../config/api.config';

interface TenantResponse {
  id: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private readonly apiUrl = IDENTITY_API_URL;

  login(credentials: LoginRequest) {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => {
        // 1. Store the token
        localStorage.setItem('token', res.token);

        // 2. Extract TenantId from JWT Claims
        // Your .NET code adds: new Claim("TenantId", user.TenantId.ToString())
        const decoded: any = jwtDecode(res.token);
        const tenantId = decoded['TenantId'];

        if (tenantId) {
          localStorage.setItem('tenantId', tenantId);
        }
      })
    );
  }

  // Centralized logic to read the role from the stored JWT
  getRole(): string | null {
    // Get the token from localStorage, return null if not found
    const token = localStorage.getItem('token');
    if (!token) return null;

    // Decode the token and extract the role claim
    try {
      const decoded: any = jwtDecode(token);
      // Checks both standard JWT 'role' and the specific Microsoft Claim URI
      return decoded['role'] ||
        decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        null;
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  // Use this for the "Tenant Onboarding" endpoint
  createTenant(name: string) {
    return this.http.post<TenantResponse>(`${this.apiUrl}/tenants?name=${name}`, {});
  }

  // Use this for the "User Registration" endpoint
  registerUser(user: UserRegistrationDto) {
    return this.http.post(`${this.apiUrl}/users`, user);
  }
}
