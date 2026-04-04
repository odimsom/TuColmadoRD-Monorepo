import { Component, AfterViewInit, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { OnboardingWizard } from '../../../shared/components/onboarding-wizard/onboarding-wizard';
import { isDesktopApp } from '../../../core/utils/runtime';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, OnboardingWizard],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home implements AfterViewInit {
  private router = inject(Router);

  billingCycle = signal<'monthly' | 'annual'>('monthly');
  readonly desktopApp = isDesktopApp();

  stats = [
    { value: 200, label: 'COLMADOS ACTIVOS', current: 0, suffix: '+' },
    { value: 50, label: 'RD$ PROCESADOS', current: 0, suffix: 'M+' },
    { value: 4.9, label: 'VALORACIÓN', current: 0, suffix: '★' },
    { value: 24, label: 'SOPORTE DOMINICANO', current: 0, suffix: '/7' }
  ];

  ngAfterViewInit() {
    this.initCounters();
  }

  toggleBilling() {
    this.billingCycle.update(val => val === 'monthly' ? 'annual' : 'monthly');
  }

  goToRegister() {
    this.router.navigate(['/auth/register']);
  }

  goToLogin() {
    this.router.navigate(['/auth/login']);
  }

  private initCounters() {
    const duration = 1500;
    const steps = 60;
    const interval = duration / steps;

    const observer = new IntersectionObserver((entries) => {
      if (entries[0].isIntersecting) {
        this.stats.forEach(stat => {
          const increment = stat.value / steps;
          let current = 0;
          const timer = setInterval(() => {
            current += increment;
            if (current >= stat.value) {
              stat.current = stat.value;
              clearInterval(timer);
            } else {
              stat.current = Number(current.toFixed(1));
            }
          }, interval);
        });
        observer.disconnect();
      }
    }, { threshold: 0.1 });

    const el = document.querySelector('#stats-bar');
    if (el) observer.observe(el);
  }
}
