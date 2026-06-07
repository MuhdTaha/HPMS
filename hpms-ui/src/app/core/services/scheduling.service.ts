import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL, IDENTITY_API_URL } from '../config/api.config';

export interface ProviderSummary {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  roleId: number;
  roleName: string;
}

export interface Patient {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
}

export interface Appointment {
  id: string;
  patientId: string;
  providerId: string;
  startTime: string;
  endTime: string;
  status: number;
}

export interface CreatePatientRequest {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  address: string;
  phoneNumber: string;
}

export interface CreateAppointmentRequest {
  patientId: string;
  providerId: string;
  startTime: string;
  endTime: string;
  forceBooking?: boolean;
}

@Injectable({ providedIn: 'root' })
export class SchedulingService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_BASE_URL;

  getProviders(): Observable<ProviderSummary[]> {
    return this.http.get<ProviderSummary[]>(`${IDENTITY_API_URL}/providers`);
  }

  getPatients(): Observable<Patient[]> {
    return this.http.get<Patient[]>(`${this.baseUrl}/scheduling/patients`);
  }

  createPatient(request: CreatePatientRequest): Observable<Patient> {
    return this.http.post<Patient>(`${this.baseUrl}/scheduling/patients`, {
      ...request,
      dateOfBirth: request.dateOfBirth
    });
  }

  getAppointments(filters?: {
    providerId?: string;
    from?: string;
    to?: string;
    status?: number;
  }): Observable<Appointment[]> {
    let params = new HttpParams();

    if (filters?.providerId) {
      params = params.set('providerId', filters.providerId);
    }
    if (filters?.from) {
      params = params.set('from', filters.from);
    }
    if (filters?.to) {
      params = params.set('to', filters.to);
    }
    if (filters?.status !== undefined) {
      params = params.set('status', filters.status.toString());
    }

    return this.http.get<Appointment[]>(`${this.baseUrl}/scheduling/appointments`, { params });
  }

  createAppointment(request: CreateAppointmentRequest): Observable<Appointment> {
    return this.http.post<Appointment>(`${this.baseUrl}/scheduling/appointments`, request);
  }

  updateAppointmentStatus(appointmentId: string, newStatus: number): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(
      `${this.baseUrl}/scheduling/appointments/${appointmentId}/status`,
      { appointmentId, newStatus }
    );
  }
}
