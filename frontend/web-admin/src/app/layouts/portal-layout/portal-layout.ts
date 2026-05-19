import { Component, inject, signal, computed, OnInit, OnDestroy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { SaleService, ShiftDto } from '../../core/services/sale.service';
import { DownloadService, DownloadInfo } from '../../core/services/download.service';
import { LS_KEYS } from '../../core/constants';

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

  isSidebarCollapsed   = signal(false);
  isMobileSidebarOpen  = signal(false);
  connectionStatus     = signal<'online' | 'offline'>(navigator.onLine ? 'online' : 'offline');
  activeShift        = signal<ShiftDto | null>(null);
  shiftElapsed       = signal('--:--:--');
  downloadInfo       = signal<DownloadInfo | null>(null);
  showDownloadBanner = signal(
    localStorage.getItem(LS_KEYS.DOWNLOAD_BANNER_DISMISSED) !== 'true'
  );
  isLicenseExpired = this.authService.isLicenseExpired;
  private router = inject(Router);
  isOnSubscriptionPage = computed(() => this.router.url.startsWith('/portal/subscription'));

  private readonly onOnline  = () => this.connectionStatus.set('online');
  private readonly onOffline = () => this.connectionStatus.set('offline');
  private shiftTimer?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    window.addEventListener('online',  this.onOnline);
    window.addEventListener('offline', this.onOffline);
    this.loadShift();
    this.loadDownloadInfo();
    // Close mobile sidebar on route change
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => this.isMobileSidebarOpen.set(false));
  }

  ngOnDestroy(): void {
    window.removeEventListener('online',  this.onOnline);
    window.removeEventListener('offline', this.onOffline);
    if (this.shiftTimer) clearInterval(this.shiftTimer);
  }

  loadDownloadInfo(): void {
    this.downloads.getLatestRelease()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(info => this.downloadInfo.set(info));
  }

  dismissBanner(): void {
    localStorage.setItem(LS_KEYS.DOWNLOAD_BANNER_DISMISSED, 'true');
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

  toggleMobileSidebar() {
    this.isMobileSidebarOpen.set(!this.isMobileSidebarOpen());
  }

  closeMobileSidebar() {
    this.isMobileSidebarOpen.set(false);
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
