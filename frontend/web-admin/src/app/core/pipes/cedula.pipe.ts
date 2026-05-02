import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formats a Dominican Republic cédula de identidad.
 *
 * Format: 001-1234567-8  (3-7-1, 11 digits)
 * Issued by the Junta Central Electoral (JCE).
 *
 * The first three digits represent the municipality of birth:
 *   001 = Distrito Nacional
 *   002 = Azua … up to ~033
 *   400+ = foreign nationals
 *
 * Accepts raw digits or already-formatted strings.
 */
@Pipe({ name: 'cedula', standalone: true, pure: true })
export class CedulaPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) return '—';

    const digits = value.replace(/\D/g, '');

    if (digits.length === 11) {
      return `${digits.slice(0, 3)}-${digits.slice(3, 10)}-${digits.slice(10)}`;
    }

    return value; // unrecognised — pass through
  }
}
