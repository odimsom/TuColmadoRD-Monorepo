import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastComponent } from './shared/ui/toast/toast.component';

@Component({
  selector: 'app-root',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, ToastComponent],
  template: `
    <router-outlet />
    <app-toast />
  `,
})
export class App {}
