import { Pipe, PipeTransform } from '@angular/core';

/** Formats a number as Dominican peso: 1200 → "RD$ 1,200" */
@Pipe({ name: 'rdCurrency', standalone: true })
export class RdCurrencyPipe implements PipeTransform {
  transform(value: number | string | null | undefined, decimals = 0): string {
    if (value === null || value === undefined || value === '') return '—';
    const num = typeof value === 'string' ? parseFloat(value.replace(/,/g, '')) : value;
    if (isNaN(num)) return '—';
    return 'RD$ ' + num.toLocaleString('es-DO', {
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals,
    });
  }
}
