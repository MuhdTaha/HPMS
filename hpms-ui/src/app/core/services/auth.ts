import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = 'https://localhost:5260/identity';

  constructor(private http: HttpClient, private router: Router) {}

  login(credentials: any) {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => {
        // Sakai works best when we store the user state
        localStorage.setItem('token', res.token);
        localStorage.setItem('tenantId', res.tenantId);
        localStorage.setItem('user', JSON.stringify(res.user));
      })
    );
  }

  signup(payload: { fullName: string; email: string; password: string; confirmPassword: string }) {
    return this.http.post<any>(`${this.apiUrl}/register`, payload);
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/auth/login']);
  }

  get isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }
}
