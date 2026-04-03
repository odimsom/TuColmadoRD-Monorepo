import { Component, OnInit, AfterViewInit, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { OnboardingWizard } from '../../../shared/components/onboarding-wizard/onboarding-wizard';
import { DownloadService, DownloadInfo } from '../../../core/services/download.service';
import { isDesktopApp } from '../../../core/utils/runtime';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, OnboardingWizard],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home implements OnInit, AfterViewInit {
  private router = inject(Router);
  private downloadService = inject(DownloadService);

  billingCycle = signal<'monthly' | 'annual'>('monthly');
  
  // Registration Flow State
  registerState = signal<'form' | 'terms' | 'success'>('form');
  acceptedTerms = signal(false);
  downloadInfo = signal<DownloadInfo | null>(null);
  readonly desktopApp = isDesktopApp();

  stats = [
    { value: 200, label: 'COLMADOS ACTIVOS', current: 0, suffix: '+' },
    { value: 50, label: 'RD$ PROCESADOS', current: 0, suffix: 'M+' },
    { value: 4.9, label: 'VALORACIÓN', current: 0, suffix: '★' },
    { value: 24, label: 'SOPORTE DOMINICANO', current: 0, suffix: '/7' }
  ];

  ngOnInit() {
    this.downloadService.getLatestTestRelease().subscribe(info => {
      this.downloadInfo.set(info);
    });
  }

  ngAfterViewInit() {
    this.initCounters();
  }

  toggleBilling() {
    this.billingCycle.update(val => val === 'monthly' ? 'annual' : 'monthly');
  }

  enterPortal() {
    this.router.navigate(['/portal/dashboard']);
  }

  // Action to move from Form to Terms
  startRegistration() {
    this.registerState.set('terms');
  }

  // Action to move from Terms to Success
  finalizeRegistration() {
    if (this.acceptedTerms()) {
      this.registerState.set('success');
    }
  }

  scrollToHero(event: Event) {
    event.preventDefault();
    const el = document.getElementById('hero-form');
    if (el) {
      el.scrollIntoView({ behavior: 'smooth' });
      // Focus first input
      setTimeout(() => {
        const input = el.querySelector('input');
        if (input) (input as HTMLElement).focus();
      }, 500);
    }
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
