import { Component, inject, OnInit } from '@angular/core';
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
export class LoginComponent implements OnInit {
  // Form fields and state
  username = '';
  password = '';
  rememberMe = false;
  loading = false;
  usernameError = '';
  passwordError = '';
  usernameValid = false;
  passwordValid = false;
  private readonly toastService = inject(ToastService);

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    const savedUsername = localStorage.getItem('rememberedUsername');
    if (savedUsername) {
      this.username = savedUsername;
      this.rememberMe = true;
      this.validateUsername(); // Validate on load to show any errors if the saved username is invalid
    }
  }

  onLogin() {
    // 1. Initial Guards
    if (!this.username || !this.password) {
      this.toastService.error('Missing details', 'Enter both username and password before signing in.');
      return;
    }

    if (!this.validateUsername() || !this.validatePassword()) {
      return;
    }

    this.loading = true;

    // 2. Build the login request
    const request: LoginRequest = {
      username: this.username,
      password: this.password,
      rememberMe: this.rememberMe ? true : false
    };

    // 3. Execute the login request
    this.authService.login(request).subscribe({
      next: () => {
        // save username to localStorage if "Remember me" is checked, otherwise remove it
        if (this.rememberMe) {
          localStorage.setItem('rememberedUsername', this.username);
        } else {
          localStorage.removeItem('rememberedUsername');
        }

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

  /* --- Validation Methods --- */
  validateUsername(): boolean {
    if (!this.username) {
      this.usernameValid = false;
      this.usernameError = '';
      return false;
    }
    const re = /^[a-z0-9\-._!@#\$%\^&*()+={}\[\]:;"'<>.,?\/\\|`~^]+$/;
    if (!re.test(this.username) || /[A-Z]/.test(this.username) || /\s/.test(this.username)) {
      this.usernameValid = false;
      this.usernameError = 'Username must be one word, lowercase letters, numbers, or punctuation.';
      return false;
    }
    this.usernameError = '';
    this.usernameValid = true;
    return true;
  }

  validatePassword(): boolean {
    if (!this.password) {
      this.passwordValid = false;
      this.passwordError = '';
      return false;
    }

    if (this.password.length < 8) {
      this.passwordValid = false;
      this.passwordError = 'Password must be at least 8 characters.';
      return false;
    }
    this.passwordError = '';
    this.passwordValid = true;
    return true;
  }

  onUsernameChange(value: string): void {
    this.username = value;
    this.validateUsername();
  }

  onPasswordChange(value: string): void {
    this.password = value;
    this.validatePassword();
  }
}
