import { Component, ChangeDetectionStrategy, inject, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ToastService, ToastMessage } from './toast.service';

const ICONS: Record<string, string> = {
  success: 'lucide:check-circle',
  error: 'lucide:x-circle',
  info: 'lucide:info',
  warning: 'lucide:alert-triangle',
};

const ALERT_CLASSES: Record<string, string> = {
  success: 'alert alert-success',
  error: 'alert alert-error',
  info: 'alert alert-info',
  warning: 'alert alert-warning',
};

@Component({
  selector: 'app-toast',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div class="toast toast-end fixed bottom-4 right-4 z-50 max-w-sm">
      @for (toast of toastService.toasts(); track toast.id) {
        <div role="alert" [class]="alertClass(toast)">
          <iconify-icon [attr.icon]="icon(toast)" class="text-lg shrink-0"></iconify-icon>
          <span class="text-sm">{{ toast.message }}</span>
          <button
            class="btn btn-ghost btn-xs btn-circle shrink-0 flex items-center justify-center"
            (click)="toastService.dismiss(toast.id)"
            aria-label="Cerrar notificación"
          >
            <iconify-icon icon="lucide:x" class="text-sm"></iconify-icon>
          </button>
        </div>
      }
    </div>
  `,
})
export class ToastComponent {
  toastService = inject(ToastService);

  alertClass(toast: ToastMessage): string {
    return ALERT_CLASSES[toast.type] ?? 'alert';
  }

  icon(toast: ToastMessage): string {
    return ICONS[toast.type] ?? 'lucide:bell';
  }
}
