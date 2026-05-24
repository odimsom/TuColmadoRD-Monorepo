import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';

type BadgeVariant = 'primary' | 'secondary' | 'accent' | 'neutral' | 'ghost' | 'info' | 'success' | 'warning' | 'error';
type BadgeSize = 'xs' | 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-badge',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<ng-content />`,
  host: { '[class]': '_class()' },
})
export class BadgeComponent {
  variant = input<BadgeVariant>('neutral');
  size = input<BadgeSize>('sm');

  _class = computed(() => {
    const c = ['tc-badge', `tc-badge-${this.variant()}`];
    // tc-badge style is naturally small, but we can add more if needed
    return c.join(' ');
  });
}
