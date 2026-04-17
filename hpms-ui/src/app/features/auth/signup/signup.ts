import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';

import { AuthService } from '../../../core/services/auth';

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
  fullName = '';
  email = '';
  password = '';
  confirmPassword = '';
  loading = false;
  errorMessage = '';

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  onSignUp(): void {
    this.errorMessage = '';

    if (!this.fullName || !this.email || !this.password || !this.confirmPassword) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.loading = true;

    this.authService
      .signup({
        fullName: this.fullName,
        email: this.email,
        password: this.password,
        confirmPassword: this.confirmPassword
      })
      .subscribe({
        next: () => {
          this.router.navigate(['/login']);
        },
        error: () => {
          this.loading = false;
          this.errorMessage = 'Unable to create account. Please try again.';
        },
        complete: () => {
          this.loading = false;
        }
      });
  }
}
