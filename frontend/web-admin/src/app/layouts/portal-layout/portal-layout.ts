import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { DownloadService } from '../../core/services/download.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-portal-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './portal-layout.component.html',
  styleUrl: './portal-layout.component.scss',
})
export class PortalLayout {
  authService = inject(AuthService);
  private readonly downloadService = inject(DownloadService);
  isSidebarCollapsed = signal(false);
  connectionStatus = signal<'online' | 'offline'>('online');
  isCheckingUpdate = signal(false);

  toggleSidebar() {
    this.isSidebarCollapsed.set(!this.isSidebarCollapsed());
  }

  logout() {
    this.authService.logout();
  }

  async updateDesktopApp() {
    if (this.isCheckingUpdate()) {
      return;
    }

    this.isCheckingUpdate.set(true);

    try {
      const downloadInfo = await firstValueFrom(this.downloadService.getLatestTestRelease());
      if (!downloadInfo?.downloadUrl) {
        window.alert('No se pudo obtener el instalador en este momento.');
        return;
      }

      window.open(downloadInfo.downloadUrl, '_blank', 'noopener,noreferrer');
    } catch {
      window.alert('No se pudo verificar actualizaciones ahora mismo.');
    } finally {
      this.isCheckingUpdate.set(false);
    }
  }

  getInitials(): string {
    const user = this.authService.currentUser();
    const fullName = `${user?.firstName || ''} ${user?.lastName || ''}`.trim();
    return (fullName || 'TC').split(' ').map(n => n[0]).join('').toUpperCase();
  }
}
