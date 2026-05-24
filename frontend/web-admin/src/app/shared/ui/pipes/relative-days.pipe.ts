import { Pipe, PipeTransform } from '@angular/core';

/** Converts a day count to a human label: 0 → "hoy", 1 → "ayer", N → "hace Nd" */
@Pipe({ name: 'relativeDays', standalone: true })
export class RelativeDaysPipe implements PipeTransform {
  transform(days: number): string {
    if (days === 0) return 'hoy';
    if (days === 1) return 'ayer';
    return `hace ${days}d`;
  }
}
