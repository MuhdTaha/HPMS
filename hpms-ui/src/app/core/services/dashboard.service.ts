import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { forkJoin, map, Observable, timer, switchMap } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface DashboardStats {
  totalCount: number;
  arrivedCount: number;
  pendingNotes: number;
  queue: any[];
  revenue: {
    today: number;
    chartLabels: string[];
    chartValues: number[];
  };
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private http = inject(HttpClient);
  private readonly baseUrl = API_BASE_URL;

  /**
   * Fetches all static dashboard data in parallel.
   * Uses forkJoin to ensure we get a single combined object back.
   */
  getInitialData(): Observable<DashboardStats> {
    const scheduling$ = this.http.get<any>(`${this.baseUrl}/scheduling/summary/today`);
    const billing$ = this.http.get<any>(`${this.baseUrl}/billing/summary/revenue`);

    return forkJoin([scheduling$, billing$]).pipe(
      map(([sched, bill]) => ({
        totalCount: sched.totalCount,
        arrivedCount: sched.arrivedCount,
        pendingNotes: sched.pendingNotes,
        queue: sched.queue,
        revenue: {
          today: bill.todayRevenue,
          chartLabels: bill.chartLabels,
          chartValues: bill.chartValues
        }
      }))
    );
  }

  /**
   * Creates a polling stream specifically for the Check-in Queue.
   * Useful for Front Desk users to see real-time patient arrivals.
   */
  getQueueUpdates(intervalMs: number = 30000): Observable<any[]> {
    return timer(0, intervalMs).pipe(
      switchMap(() => this.http.get<any>(`${this.baseUrl}/scheduling/summary/today`)),
      map(res => res.queue)
    );
  }
}
