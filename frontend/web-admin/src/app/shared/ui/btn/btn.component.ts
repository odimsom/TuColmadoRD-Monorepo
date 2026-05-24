import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';

type BtnVariant = 'primary' | 'secondary' | 'accent' | 'ghost' | 'outline' | 'error' | 'warning' | 'success' | 'neutral';
type BtnSize = 'xs' | 'sm' | 'md' | 'lg';

@Component({
  selector: 'button[appBtn], a[appBtn]',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loading()) {
      <span class="loading loading-spinner loading-xs"></span>
    }
    <ng-content />
  `,
  host: {
    '[class]': '_classes()',
    '[attr.disabled]': 'disabled() || loading() ? true : null',
    '[attr.aria-busy]': 'loading()',
  },
})
export class BtnComponent {
  variant = input<BtnVariant>('primary');
  size = input<BtnSize>('md');
  loading = input(false);
  disabled = input(false);
  wide = input(false);

  _classes = computed(() => {
    const c = ['btn', `btn-${this.variant()}`];
    if (this.size() !== 'md') c.push(`btn-${this.size()}`);
    if (this.wide()) c.push('w-full');
    return c.join(' ');
  });
}
