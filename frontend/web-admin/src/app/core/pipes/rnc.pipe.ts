import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formats a Dominican Republic RNC (Registro Nacional del Contribuyente).
 *
 * Persons (cédula as RNC, 11 digits):  001-1234567-8
 * Companies (9 digits):                131-12345-6
 *
 * Both formats are used as RNC on fiscal documents.
 * Accepts raw digits or already-formatted strings.
 */
@Pipe({ name: 'rnc', standalone: true, pure: true })
export class RncPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) return '—';

    const digits = value.replace(/\D/g, '');

    if (digits.length === 9) {
      // Company RNC: 3-5-1
      return `${digits.slice(0, 3)}-${digits.slice(3, 8)}-${digits.slice(8)}`;
    }

    if (digits.length === 11) {
      // Person RNC (cédula): 3-7-1
      return `${digits.slice(0, 3)}-${digits.slice(3, 10)}-${digits.slice(10)}`;
    }

    return value; // unrecognised — pass through
  }
}
