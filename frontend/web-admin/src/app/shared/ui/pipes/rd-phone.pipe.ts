import { Pipe, PipeTransform } from '@angular/core';

/** Formats a phone number: 8095551234 → "(809) 555-1234" */
@Pipe({ name: 'rdPhone', standalone: true })
export class RdPhonePipe implements PipeTransform {
  transform(value: string | number | null | undefined): string {
    if (!value) return '—';
    const s = value.toString().replace(/\D/g, '');
    if (s.length !== 10) return value.toString();
    return `(${s.slice(0, 3)}) ${s.slice(3, 6)}-${s.slice(6)}`;
  }
}
