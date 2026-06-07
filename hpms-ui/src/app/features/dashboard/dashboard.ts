import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../core/services/auth/auth';
import { DashboardService } from '../../core/services/dashboard.service';
import { SchedulingService, Appointment, Patient } from '../../core/services/scheduling.service';
import { Subscription } from 'rxjs';

interface QueueItem {
  id: string;
  patientId: string;
  startTime: string;
  status: number;
  patientName: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit, OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly dashboardService = inject(DashboardService);
  private readonly schedulingService = inject(SchedulingService);

  private queueSubscription?: Subscription;

  userRole = '';
  todayRevenue = 0;
  totalAppointments = 0;
  arrivedPatients: QueueItem[] = [];
  todaysAppointments: Appointment[] = [];
  patients: Patient[] = [];
  revenueRows: Array<{ date: string; amount: number }> = [];

  ngOnInit(): void {
    this.userRole = this.authService.getRole() || '';
    this.loadPatients();
    this.loadDashboardData();
    this.startQueuePolling();
  }

  ngOnDestroy(): void {
    this.queueSubscription?.unsubscribe();
  }

  loadPatients(): void {
    this.schedulingService.getPatients().subscribe({
      next: (patients) => {
        this.patients = patients;
        this.mapQueueNames();
        this.mapAppointmentNames();
      }
    });
  }

  loadDashboardData(): void {
    this.dashboardService.getInitialData().subscribe({
      next: (data) => {
        this.totalAppointments = data.totalCount;
        this.todayRevenue = data.revenue.today;
        this.arrivedPatients = data.queue.map((item) => ({
          ...item,
          patientName: this.resolvePatientName(item.patientId)
        }));
        this.revenueRows = data.revenue.chartLabels.map((date, index) => ({
          date,
          amount: data.revenue.chartValues[index] ?? 0
        }));
      }
    });

    const today = new Date();
    const start = new Date(today.getFullYear(), today.getMonth(), today.getDate());
    const end = new Date(today.getFullYear(), today.getMonth(), today.getDate(), 23, 59, 59);

    this.schedulingService.getAppointments({
      from: start.toISOString(),
      to: end.toISOString()
    }).subscribe({
      next: (appointments) => {
        this.todaysAppointments = appointments;
      }
    });
  }

  startQueuePolling(): void {
    this.queueSubscription = this.dashboardService.getQueueUpdates(30000).subscribe({
      next: (queue) => {
        this.arrivedPatients = queue.map((item) => ({
          ...item,
          patientName: this.resolvePatientName(item.patientId)
        }));
      }
    });
  }

  resolvePatientName(patientId: string): string {
    const patient = this.patients.find((p) => p.id === patientId);
    return patient ? `${patient.firstName} ${patient.lastName}` : patientId;
  }

  mapQueueNames(): void {
    this.arrivedPatients = this.arrivedPatients.map((item) => ({
      ...item,
      patientName: this.resolvePatientName(item.patientId)
    }));
  }

  mapAppointmentNames(): void {
    this.todaysAppointments = [...this.todaysAppointments];
  }

  patientName(patientId: string): string {
    return this.resolvePatientName(patientId);
  }

  showProvider(): boolean {
    return ['SystemAdmin', 'ClinicAdmin', 'Provider'].includes(this.userRole);
  }

  showFrontDesk(): boolean {
    return ['SystemAdmin', 'ClinicAdmin', 'FrontDesk'].includes(this.userRole);
  }

  showBilling(): boolean {
    return ['SystemAdmin', 'ClinicAdmin', 'BillingManager'].includes(this.userRole);
  }
}
