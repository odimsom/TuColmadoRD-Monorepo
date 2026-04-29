import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'rdCurrency', standalone: true, pure: true })
export class RdCurrencyPipe implements PipeTransform {
  /**
   * Formats a number as Dominican peso.
   * @param value  The amount.
   * @param signed When true, negative values render as  "-RD$ 1,234.56"
   *               and positive values render as "+RD$ 1,234.56".
   *               Useful for balance columns where sign matters.
   * @param empty  String to return for null/undefined values (default "—").
   */
  transform(
    value: number | null | undefined,
    signed = false,
    empty = '—',
  ): string {
    if (value == null) return empty;

    const abs = Math.abs(value);
    const formatted = abs.toLocaleString('es-DO', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    });

    if (!signed) return `RD$ ${formatted}`;

    if (value < 0) return `-RD$ ${formatted}`;
    if (value > 0) return `RD$ ${formatted}`;
    return `RD$ ${formatted}`;
  }
}
