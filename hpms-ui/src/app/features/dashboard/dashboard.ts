import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChartModule } from 'primeng/chart';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../core/services/auth/auth';
import { forkJoin, timer, switchMap } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ChartModule, TableModule, ButtonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private authService = inject(AuthService);

  userRole: string = '';

  // Data for Widgets
  todaysAppointments: any[] = [];
  arrivedPatients: any[] = [];
  revenueData: any;

  ngOnInit() {
    // Simple and clean extraction using the auth service helper
    this.userRole = this.authService.getRole() || '';

    console.log('Dashboard initialized for role:', this.userRole);
    this.loadDashboardData();
  }

  loadDashboardData() {
    // Call services to fetch data for widgets based on user role
  }

  // Check visibility helpers
  showProvider() { return ['SystemAdmin', 'ClinicAdmin', 'Provider'].includes(this.userRole); }
  showFrontDesk() { return ['SystemAdmin', 'ClinicAdmin', 'FrontDesk'].includes(this.userRole); }
  showBilling() { return ['SystemAdmin', 'ClinicAdmin', 'BillingManager'].includes(this.userRole); }
}
