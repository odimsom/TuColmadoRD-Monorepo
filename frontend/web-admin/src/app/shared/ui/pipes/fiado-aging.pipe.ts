import { Pipe, PipeTransform } from '@angular/core';

export type FiadoStatus = 'overdue' | 'soon' | 'fresh';

/** Maps aging days to a status token. overdue ≥ 14d, soon 7–13d, fresh < 7d */
@Pipe({ name: 'fiadoStatus', standalone: true })
export class FiadoStatusPipe implements PipeTransform {
  transform(days: number): FiadoStatus {
    if (days >= 14) return 'overdue';
    if (days >= 7) return 'soon';
    return 'fresh';
  }
}

/** Returns the Tailwind badge classes for a fiado status */
@Pipe({ name: 'fiadoStatusClass', standalone: true })
export class FiadoStatusClassPipe implements PipeTransform {
  transform(status: FiadoStatus): string {
    switch (status) {
      case 'overdue': return 'badge badge-error badge-sm';
      case 'soon':    return 'badge badge-warning badge-sm';
      case 'fresh':   return 'badge badge-success badge-sm';
    }
  }
}
