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
  private readonly toastService = inject(ToastService);

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  onSignUp(): void {
    this.errorMessage = '';

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
}
