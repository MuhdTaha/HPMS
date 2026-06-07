import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SchedulingService, Patient, CreatePatientRequest } from '../../core/services/scheduling.service';
import { ToastService } from '../../core/services/toast/index';

@Component({
  selector: 'app-patients',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, ButtonModule, InputTextModule],
  templateUrl: './patients.html',
  styleUrl: './patients.scss'
})
export class PatientsComponent implements OnInit {
  private readonly schedulingService = inject(SchedulingService);
  private readonly toastService = inject(ToastService);

  patients: Patient[] = [];
  loading = false;

  form: CreatePatientRequest = {
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    email: '',
    address: '',
    phoneNumber: ''
  };

  ngOnInit(): void {
    this.loadPatients();
  }

  loadPatients(): void {
    this.loading = true;
    this.schedulingService.getPatients().subscribe({
      next: (patients) => {
        this.patients = patients;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastService.error('Load failed', 'Could not load patients.');
      }
    });
  }

  createPatient(): void {
    if (!this.form.firstName || !this.form.lastName || !this.form.dateOfBirth) {
      this.toastService.error('Missing details', 'First name, last name, and date of birth are required.');
      return;
    }

    this.schedulingService.createPatient(this.form).subscribe({
      next: () => {
        this.toastService.success('Patient created', 'The patient record was saved.');
        this.form = {
          firstName: '',
          lastName: '',
          dateOfBirth: '',
          email: '',
          address: '',
          phoneNumber: ''
        };
        this.loadPatients();
      },
      error: () => this.toastService.error('Create failed', 'Could not create the patient record.')
    });
  }
}
