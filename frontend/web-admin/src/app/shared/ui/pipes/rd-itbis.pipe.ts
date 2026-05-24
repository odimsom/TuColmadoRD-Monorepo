import { Pipe, PipeTransform } from '@angular/core';

/** Formats an ITBIS rate: 0.18 → "18% ITBIS" */
@Pipe({ name: 'rdItbis', standalone: true })
export class RdItbisPipe implements PipeTransform {
  transform(value: number | string | null | undefined): string {
    if (value === null || value === undefined || value === '') return '—';
    const num = typeof value === 'string' ? parseFloat(value) : value;
    if (isNaN(num)) return '—';
    if (num === 0) return 'Exento';
    return `${(num * 100).toFixed(0)}% ITBIS`;
  }
}
