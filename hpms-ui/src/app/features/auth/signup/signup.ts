import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';

import { AuthService } from '../../../core/services/auth/auth';
import { ToastService } from '../../../core/services/toast/index';

interface TenantResponse {
  id: string;
}

@Component({
  selector: 'app-signup',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    RouterLinkActive,
    InputTextModule,
    PasswordModule,
    ButtonModule
  ],
  templateUrl: './signup.html',
  styleUrl: './signup.scss'
})

export class SignupComponent {
  username = '';
  firstName = '';
  lastName = '';
  email = '';
  password = '';
  confirmPassword = '';
  clinicName = '';

  loading = false;
  errorMessage = '';
  usernameError = '';
  emailError = '';
  passwordError = '';
  confirmPasswordError = '';
  usernameValid = false;
  emailValid = false;
  passwordValid = false;
  confirmPasswordValid = false;
  private readonly toastService = inject(ToastService);

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  onSignUp(): void {
    this.errorMessage = '';
    this.usernameError = '';
    this.emailError = '';
    this.passwordError = '';
    this.confirmPasswordError = '';

    if (!this.username || !this.firstName || !this.lastName || !this.email || !this.password || !this.confirmPassword || !this.clinicName) {
      this.errorMessage = 'Please fill in all required fields.';
      this.toastService.error('Missing details', this.errorMessage);
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      this.toastService.error('Signup failed', this.errorMessage);
      return;
    }

    if (!this.validateEmail()) {
      this.toastService.error('Invalid email', this.emailError || 'Email format is invalid.');
      return;
    }

    if (!this.validateUsername()) {
      this.toastService.error('Invalid username', this.usernameError || 'Username format is invalid.');
      return;
    }

    if (!this.validatePassword()) {
      this.toastService.error('Weak password', this.passwordError || 'Password does not meet safety requirements.');
      return;
    }

    this.loading = true;

    this.authService.createTenant(this.clinicName).subscribe({
      next: (tenant: TenantResponse) => {
        const userDto = {
          username: this.username,
          email: this.email,
          firstName: this.firstName,
          lastName: this.lastName,
          tenantId: tenant.id,
          roleId: 1,
          password: this.password
        };

        this.authService.registerUser(userDto).subscribe({
          next: () => this.router.navigate(['/login']),
          error: () => {
            this.loading = false;
            this.errorMessage = 'Clinic created, but user registration failed.';
            this.toastService.error('Signup failed', this.errorMessage);
          }
        });
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Failed to onboard clinic. Please check the name.';
        this.toastService.error('Signup failed', this.errorMessage);
      }
    });
  }

  validateUsername(): boolean {
    // One word, no spaces; lowercase letters, numbers, or punctuation
    if (!this.username) {
      this.usernameError = '';
      this.usernameValid = false;
      return false;
    }
    const re = /^[a-z0-9\-._!@#\$%\^&*()+={}\[\]:;"'<>.,?\/\\|`~^]+$/;
    if (!re.test(this.username)) {
      this.usernameError = 'Username must be one word (no spaces). Use lowercase letters, numbers, or punctuation.';
      this.usernameValid = false;
      return false;
    }
    // ensure lowercase letters only
    if (/[A-Z]/.test(this.username)) {
      this.usernameError = 'Username must be lowercase only.';
      this.usernameValid = false;
      return false;
    }
    this.usernameError = '';
    this.usernameValid = true;
    return true;
  }

  validateEmail(): boolean {
    if (!this.email) {
      this.emailError = '';
      this.emailValid = false;
      return false;
    }
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!re.test(this.email)) {
      this.emailError = 'Enter a valid email address.';
      this.emailValid = false;
      return false;
    }
    this.emailError = '';
    this.emailValid = true;
    return true;
  }

  validatePassword(): boolean {
    // Basic strong password check: min 8 chars, uppercase, lowercase, digit, special
    if (!this.password) {
      this.passwordError = '';
      this.passwordValid = false;
      return false;
    }
    if (this.password.length < 8) {
      this.passwordError = 'Password must be at least 8 characters.';
      this.passwordValid = false;
      return false;
    }
    if (!/[a-z]/.test(this.password) || !/[A-Z]/.test(this.password) || !/[0-9]/.test(this.password) || !/[!@#\$%\^&*()_+\-=[\]{};':"\\|,.<>\/?`~]/.test(this.password)) {
      this.passwordError = 'Password must include uppercase, lowercase, number, and special character.';
      this.passwordValid = false;
      return false;
    }
    this.passwordError = '';
    this.passwordValid = true;
    return true;
  }

  onUsernameChange(value: string): void {
    this.username = value;
    this.usernameError = '';
    this.validateUsername();
  }

  onEmailChange(value: string): void {
    this.email = value;
    this.emailError = '';
    this.validateEmail();
  }

  onPasswordChange(value: string): void {
    this.password = value;
    this.passwordError = '';
    this.validatePassword();
    if (this.confirmPassword && this.password !== this.confirmPassword) {
      this.confirmPasswordError = 'Passwords do not match.';
      this.confirmPasswordValid = false;
    } else {
      this.confirmPasswordError = '';
      this.confirmPasswordValid = !!this.confirmPassword;
    }
  }

  onConfirmPasswordChange(value: string): void {
    this.confirmPassword = value;
    this.confirmPasswordError = '';
    if (this.password && this.password !== this.confirmPassword) {
      this.confirmPasswordError = 'Passwords do not match.';
      this.confirmPasswordValid = false;
    } else {
      this.confirmPasswordError = '';
      this.confirmPasswordValid = !!this.confirmPassword && !!this.password;
    }
  }
}
