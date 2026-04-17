import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

// PrimeNG Imports
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';

// Services and Models
import { LoginRequest } from '../../../core/services/auth/auth.models';
import { AuthService } from '../../../core/services/auth/auth';
import { ToastService } from '../../../core/services/toast/index';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive, InputTextModule, PasswordModule, ButtonModule, CheckboxModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent {
  username = '';
  password = '';
  rememberMe = false;
  loading = false;
  private readonly toastService = inject(ToastService);

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onLogin() {
    if (!this.username || !this.password) {
      this.toastService.error('Missing details', 'Enter both username and password before signing in.');
      return;
    }

    this.loading = true;

    const request: LoginRequest = {
        username: this.username,
        password: this.password
    };

    this.authService.login(request).subscribe({
      next: () => {
        this.loading = false;
        console.log('Login successful');
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        console.error('Login failed', err);
        this.toastService.error('Login failed', 'Invalid username or password.');
      }
    });
  }
}
