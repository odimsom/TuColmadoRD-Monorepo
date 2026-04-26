import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { vi } from 'vitest';

import { PortalLayout } from './portal-layout';
import { AuthService } from '../../core/services/auth.service';
import { DownloadService } from '../../core/services/download.service';
import { SaleService } from '../../core/services/sale.service';
import { throwError } from 'rxjs';

describe('PortalLayout', () => {
  let component: PortalLayout;
  let fixture: ComponentFixture<PortalLayout>;

  beforeEach(async () => {
    const authMock = { currentUser: () => null, logout: vi.fn(), isLicenseExpired: () => false };
    const downloadMock = { getLatestTestRelease: vi.fn() };
    const saleMock = { getCurrentShift: vi.fn().mockReturnValue(throwError(() => new Error('no shift'))) };

    await TestBed.configureTestingModule({
      imports: [PortalLayout],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        { provide: AuthService, useValue: authMock },
        { provide: DownloadService, useValue: downloadMock },
        { provide: SaleService, useValue: saleMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PortalLayout);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
