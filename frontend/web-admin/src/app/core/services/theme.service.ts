import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  currentTheme = signal<'tucolmadodark' | 'tucolmadolight'>('tucolmadodark');

  initTheme() {
    const savedTheme = localStorage.getItem('tucolmado-theme') as 'tucolmadodark' | 'tucolmadolight';
    const themeToApply = savedTheme || 'tucolmadodark';
    this.setTheme(themeToApply);
  }

  toggleTheme() {
    const newTheme = this.currentTheme() === 'tucolmadodark' ? 'tucolmadolight' : 'tucolmadodark';
    this.setTheme(newTheme);
  }

  private setTheme(theme: 'tucolmadodark' | 'tucolmadolight') {
    this.currentTheme.set(theme);
    localStorage.setItem('tucolmado-theme', theme);
    document.documentElement.setAttribute('data-theme', theme);
  }
}
