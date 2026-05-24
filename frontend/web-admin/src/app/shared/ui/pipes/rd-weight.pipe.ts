import { Pipe, PipeTransform } from '@angular/core';

/** Formats a weight value: 5, 'lb' → "5 lb" */
@Pipe({ name: 'rdWeight', standalone: true })
export class RdWeightPipe implements PipeTransform {
  transform(value: number | string | null | undefined, unit: string = 'lb'): string {
    if (value === null || value === undefined || value === '') return '—';
    return `${value} ${unit}`;
  }
}
