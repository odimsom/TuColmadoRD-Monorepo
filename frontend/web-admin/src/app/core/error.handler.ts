import { ErrorHandler, Injectable, inject, NgZone } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastService } from '../shared/ui/toast/toast.service';

@Injectable()
export class AppErrorHandler implements ErrorHandler {
  private toast = inject(ToastService);
  private zone = inject(NgZone);

  handleError(error: unknown): void {
    this.zone.run(() => {
      if (error instanceof HttpErrorResponse) {
        const msg = (error.error as { message?: string } | undefined)?.message ?? error.message;
        this.toast.error(msg);
      } else {
        console.error(error);
      }
    });
  }
}
