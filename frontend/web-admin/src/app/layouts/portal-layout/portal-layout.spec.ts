import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { vi } from 'vitest';

import { PortalLayout } from './portal-layout';
import { AuthService } from '../../core/services/auth.service';
import { DownloadService } from '../../core/services/download.service';

describe('PortalLayout', () => {
  let component: PortalLayout;
  let fixture: ComponentFixture<PortalLayout>;

  beforeEach(async () => {
    const authMock = { currentUser: () => null, logout: vi.fn() };
    const downloadMock = { getLatestTestRelease: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [PortalLayout],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        { provide: AuthService, useValue: authMock },
        { provide: DownloadService, useValue: downloadMock },
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
