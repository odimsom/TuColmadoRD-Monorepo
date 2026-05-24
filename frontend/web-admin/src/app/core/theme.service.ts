import { Injectable, signal, inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private doc = inject(DOCUMENT);
  private readonly STORAGE_KEY = 'tc_theme';

  readonly isDark = signal(false);

  constructor() {
    const saved = this.doc.defaultView?.localStorage.getItem(this.STORAGE_KEY);
    const prefersDark = this.doc.defaultView?.matchMedia('(prefers-color-scheme: dark)').matches ?? false;
    this.apply(saved ? saved === 'dark' : prefersDark);
  }

  toggle(): void {
    this.apply(!this.isDark());
  }

  private apply(dark: boolean): void {
    this.isDark.set(dark);
    this.doc.documentElement.setAttribute('data-theme', dark ? 'tucolmadord-dark' : 'tucolmadord-light');
    this.doc.defaultView?.localStorage.setItem(this.STORAGE_KEY, dark ? 'dark' : 'light');
  }
}
