import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-onboarding-wizard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './onboarding-wizard.html',
  styleUrl: './onboarding-wizard.scss',
})
export class OnboardingWizard implements OnInit {
  visible = signal(false);
  step = signal(1);

  // States for step 1
  typeSelected = signal<string | null>(null);
  empCount = signal<string | null>(null);
  useFiados = signal(false);

  stepInfo = computed(() => {
    switch (this.step()) {
      case 1: return 'Perfil del Negocio';
      case 2: return 'Inventario Inicial';
      case 3: return 'Bienvenida';
      default: return '';
    }
  });

  ngOnInit() {
    const skipOnboarding = localStorage.getItem('skip_onboarding');
    // For demo purposes, we show it if it's not set
    if (!skipOnboarding) {
      setTimeout(() => this.visible.set(true), 1500);
    }
  }

  next() {
    if (this.step() < 3) {
      this.step.update(v => v + 1);
    } else {
      this.close();
    }
  }

  prev() {
    if (this.step() > 1) {
      this.step.update(v => v - 1);
    }
  }

  close() {
    this.visible.set(false);
    localStorage.setItem('skip_onboarding', 'true');
  }
}
