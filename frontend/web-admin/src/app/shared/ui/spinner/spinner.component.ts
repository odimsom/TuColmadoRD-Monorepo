import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';

@Component({
  selector: 'app-spinner',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span
      role="status"
      [attr.aria-label]="label()"
      [class]="'loading loading-spinner ' + _sizeClass()"
    ></span>
  `,
})
export class SpinnerComponent {
  size = input<'xs' | 'sm' | 'md' | 'lg'>('md');
  label = input('Cargando...');

  _sizeClass = computed(() => `loading-${this.size()}`);
}
