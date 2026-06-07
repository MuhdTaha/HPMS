import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { Dashboard } from './dashboard';
import { AuthService } from '../../core/services/auth/auth';
import { DashboardService } from '../../core/services/dashboard.service';
import { SchedulingService } from '../../core/services/scheduling.service';

describe('Dashboard', () => {
  let component: Dashboard;
  let fixture: ComponentFixture<Dashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
        {
          provide: AuthService,
          useValue: { getRole: () => 'ClinicAdmin' }
        },
        {
          provide: DashboardService,
          useValue: {
            getInitialData: () => of({
              totalCount: 0,
              arrivedCount: 0,
              inSessionCount: 0,
              queue: [],
              revenue: { today: 0, chartLabels: [], chartValues: [] }
            }),
            getQueueUpdates: () => of([])
          }
        },
        {
          provide: SchedulingService,
          useValue: {
            getPatients: () => of([]),
            getAppointments: () => of([])
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
