import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { LoginRequest, LoginResponse, UserRegistrationDto } from './auth.models';
import { jwtDecode } from 'jwt-decode';

interface TenantResponse {
  id: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5260/identity';

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

  // Use this for the "Tenant Onboarding" endpoint
  createTenant(name: string) {
    return this.http.post<TenantResponse>(`${this.apiUrl}/tenants?name=${name}`, {});
  }

  // Use this for the "User Registration" endpoint
  registerUser(user: UserRegistrationDto) {
    return this.http.post(`${this.apiUrl}/users`, user);
  }
}
