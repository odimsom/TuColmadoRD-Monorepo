import { Pipe, PipeTransform } from '@angular/core';

/**
 * Displays a DGII Número de Comprobante Fiscal (NCF).
 *
 * NCF structure (Norma 06-18):
 *   B + 2-digit series + "-" + 8-digit sequential
 *   e.g. "B01-00000001"
 *
 * Series reference:
 *   B01 — Factura de Crédito Fiscal
 *   B02 — Factura de Consumo
 *   B03 — Nota de Débito
 *   B04 — Nota de Crédito
 *   B11 — Comprobante de Compras
 *   B12 — Registro de Gastos Menores
 *   B13 — Registro Especial de Gastos Gubernamentales
 *   B14 — Comprobante Gubernamentales
 *   B15 — Comprobante de Exportaciones
 *   B16 — Comprobante para Pagos al Exterior
 *
 * If the value is already formatted it is returned as-is.
 * Raw digits "B0100000001" are formatted to "B01-00000001".
 */
@Pipe({ name: 'ncf', standalone: true, pure: true })
export class NcfPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) return '—';

    const v = value.trim().toUpperCase();

    // Already formatted
    if (/^B\d{2}-\d{8}$/.test(v)) return v;

    // Raw: B + 2 + 8 = 11 chars without dash
    if (/^B\d{10}$/.test(v)) {
      return `${v.slice(0, 3)}-${v.slice(3)}`;
    }

    return v; // unrecognised — pass through
  }
}
