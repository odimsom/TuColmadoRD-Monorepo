import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formats Dominican Republic phone numbers.
 *
 * Accepts digits-only or already-formatted strings:
 *   "8095551234"     → "(809) 555-1234"
 *   "18095551234"    → "(809) 555-1234"
 *   "809-555-1234"   → "(809) 555-1234"
 *   "(809) 555-1234" → "(809) 555-1234"
 *
 * Dominican carriers use area codes 809, 829, and 849 (all map to +1-809
 * in the NANP but are dialed with their own prefix locally).
 */
@Pipe({ name: 'rdPhone', standalone: true, pure: true })
export class RdPhonePipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) return '—';

    const digits = value.replace(/\D/g, '');

    // Strip leading country code "1" (NANP) if present and result is 10 digits
    const local = digits.length === 11 && digits.startsWith('1')
      ? digits.slice(1)
      : digits;

    if (local.length !== 10) return value; // not a recognizable format — pass through

    const area   = local.slice(0, 3);
    const middle = local.slice(3, 6);
    const last   = local.slice(6);

    return `(${area}) ${middle}-${last}`;
  }
}
