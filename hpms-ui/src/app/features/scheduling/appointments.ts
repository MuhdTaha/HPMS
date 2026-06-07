import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import {
  Appointment,
  CreateAppointmentRequest,
  Patient,
  ProviderSummary,
  SchedulingService
} from '../../core/services/scheduling.service';
import { AuthService } from '../../core/services/auth/auth';
import { ToastService } from '../../core/services/toast/index';

const STATUS_LABELS: Record<number, string> = {
  1: 'Scheduled',
  2: 'Arrived',
  3: 'In Session',
  4: 'Completed',
  5: 'No Show',
  6: 'Canceled'
};

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, ButtonModule, InputTextModule, CheckboxModule],
  templateUrl: './appointments.html',
  styleUrl: './appointments.scss'
})
export class AppointmentsComponent implements OnInit {
  private readonly schedulingService = inject(SchedulingService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);

  appointments: Appointment[] = [];
  patients: Patient[] = [];
  providers: ProviderSummary[] = [];
  loading = false;

  canOverrideBooking = ['SystemAdmin', 'ClinicAdmin'].includes(this.authService.getRole() ?? '');

  form: CreateAppointmentRequest & { date: string; startTime: string; endTime: string } = {
    patientId: '',
    providerId: '',
    startTime: '',
    endTime: '',
    date: '',
    forceBooking: false
  };

  ngOnInit(): void {
    this.loadReferenceData();
    this.loadAppointments();
  }

  statusLabel(status: number): string {
    return STATUS_LABELS[status] ?? `Status ${status}`;
  }

  patientName(patientId: string): string {
    const patient = this.patients.find((p) => p.id === patientId);
    return patient ? `${patient.firstName} ${patient.lastName}` : patientId;
  }

  providerName(providerId: string): string {
    const provider = this.providers.find((p) => p.id === providerId);
    return provider ? `${provider.firstName} ${provider.lastName}` : providerId;
  }

  loadReferenceData(): void {
    this.schedulingService.getPatients().subscribe({
      next: (patients) => (this.patients = patients),
      error: () => this.toastService.error('Load failed', 'Could not load patients.')
    });

    this.schedulingService.getProviders().subscribe({
      next: (providers) => (this.providers = providers),
      error: () => this.toastService.error('Load failed', 'Could not load providers.')
    });
  }

  loadAppointments(): void {
    this.loading = true;

    const today = new Date();
    const from = new Date(today.getFullYear(), today.getMonth(), today.getMonth() - 1, today.getDate());
    const to = new Date(today.getFullYear(), today.getMonth(), today.getDate() + 31);

    this.schedulingService.getAppointments({
      from: from.toISOString(),
      to: to.toISOString()
    }).subscribe({
      next: (appointments) => {
        this.appointments = appointments;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastService.error('Load failed', 'Could not load appointments.');
      }
    });
  }

  bookAppointment(): void {
    if (!this.form.patientId || !this.form.providerId || !this.form.date || !this.form.startTime || !this.form.endTime) {
      this.toastService.error('Missing details', 'Select a patient, provider, date, and time range.');
      return;
    }

    const request: CreateAppointmentRequest = {
      patientId: this.form.patientId,
      providerId: this.form.providerId,
      startTime: new Date(`${this.form.date}T${this.form.startTime}`).toISOString(),
      endTime: new Date(`${this.form.date}T${this.form.endTime}`).toISOString(),
      forceBooking: this.form.forceBooking
    };

    this.schedulingService.createAppointment(request).subscribe({
      next: () => {
        this.toastService.success('Appointment booked', 'The appointment was created.');
        this.form.forceBooking = false;
        this.loadAppointments();
      },
      error: (err) => {
        const message = err?.error ?? 'Could not book the appointment.';
        this.toastService.error('Booking failed', typeof message === 'string' ? message : 'Could not book the appointment.');
      }
    });
  }

  advanceStatus(appointment: Appointment, newStatus: number): void {
    this.schedulingService.updateAppointmentStatus(appointment.id, newStatus).subscribe({
      next: () => {
        this.toastService.success('Status updated', 'The appointment status was updated.');
        this.loadAppointments();
      },
      error: () => this.toastService.error('Update failed', 'Could not update appointment status.')
    });
  }
}
