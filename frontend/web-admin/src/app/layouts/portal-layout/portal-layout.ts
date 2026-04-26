import { Component, inject, signal, OnInit, OnDestroy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { SaleService, ShiftDto } from '../../core/services/sale.service';
import { DownloadService, DownloadInfo } from '../../core/services/download.service';

const BANNER_DISMISSED_KEY = 'tc_download_banner_dismissed';

@Component({
  selector: 'app-portal-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './portal-layout.component.html',
  styleUrl: './portal-layout.component.scss',
})
export class PortalLayout implements OnInit, OnDestroy {
  authService     = inject(AuthService);
  private sales   = inject(SaleService);
  private downloads = inject(DownloadService);
  private destroyRef = inject(DestroyRef);

  isSidebarCollapsed = signal(false);
  connectionStatus   = signal<'online' | 'offline'>(navigator.onLine ? 'online' : 'offline');
  activeShift        = signal<ShiftDto | null>(null);
  shiftElapsed       = signal('--:--:--');
  downloadInfo       = signal<DownloadInfo | null>(null);
  showDownloadBanner = signal(
    localStorage.getItem(BANNER_DISMISSED_KEY) !== 'true'
  );
  isLicenseExpired = this.authService.isLicenseExpired;

  private readonly onOnline  = () => this.connectionStatus.set('online');
  private readonly onOffline = () => this.connectionStatus.set('offline');
  private shiftTimer?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    window.addEventListener('online',  this.onOnline);
    window.addEventListener('offline', this.onOffline);
    this.loadShift();
    this.loadDownloadInfo();
  }

  ngOnDestroy(): void {
    window.removeEventListener('online',  this.onOnline);
    window.removeEventListener('offline', this.onOffline);
    if (this.shiftTimer) clearInterval(this.shiftTimer);
  }

  loadDownloadInfo(): void {
    this.downloads.getLatestTestRelease()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(info => this.downloadInfo.set(info));
  }

  dismissBanner(): void {
    localStorage.setItem(BANNER_DISMISSED_KEY, 'true');
    this.showDownloadBanner.set(false);
  }

  downloadPOS(): void {
    const info = this.downloadInfo();
    if (!info?.downloadUrl) return;
    window.open(info.downloadUrl, '_blank');
    this.dismissBanner();
  }

  loadShift(): void {
    this.sales.getCurrentShift()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: shift => {
          this.activeShift.set(shift);
          this.startShiftTimer(shift.openedAt);
        },
        error: () => this.activeShift.set(null)
      });
  }

  private startShiftTimer(openedAt: string): void {
    if (this.shiftTimer) clearInterval(this.shiftTimer);
    const update = () => {
      const diff = Math.max(0, Math.floor((Date.now() - new Date(openedAt).getTime()) / 1000));
      const h = Math.floor(diff / 3600).toString().padStart(2, '0');
      const m = Math.floor((diff % 3600) / 60).toString().padStart(2, '0');
      const s = (diff % 60).toString().padStart(2, '0');
      this.shiftElapsed.set(`${h}:${m}:${s}`);
    };
    update();
    this.shiftTimer = setInterval(update, 1000);
  }

  toggleSidebar() {
    this.isSidebarCollapsed.set(!this.isSidebarCollapsed());
  }

  logout() {
    this.authService.logout();
  }

  getInitials(): string {
    const user = this.authService.currentUser();
    const fullName = `${user?.firstName || ''} ${user?.lastName || ''}`.trim();
    return (fullName || 'TC').split(' ').map(n => n[0]).join('').toUpperCase();
  }
}
