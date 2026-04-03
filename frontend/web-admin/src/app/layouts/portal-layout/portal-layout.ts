import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-portal-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './portal-layout.component.html',
  styleUrl: './portal-layout.component.scss',
})
export class PortalLayout {
  authService = inject(AuthService);
  isSidebarCollapsed = signal(false);
  connectionStatus = signal<'online' | 'offline'>('online');

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
