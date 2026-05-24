import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'info' | 'warning';
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private _toasts = signal<ToastMessage[]>([]);
  readonly toasts = this._toasts.asReadonly();

  success(message: string): void { this.add('success', message); }
  error(message: string): void { this.add('error', message); }
  info(message: string): void { this.add('info', message); }
  warning(message: string): void { this.add('warning', message); }

  dismiss(id: string): void {
    this._toasts.update(ts => ts.filter(t => t.id !== id));
  }

  private add(type: ToastMessage['type'], message: string): void {
    const id = Math.random().toString(36).slice(2);
    this._toasts.update(ts => [...ts, { id, type, message }]);
    setTimeout(() => this.dismiss(id), 4500);
  }
}
