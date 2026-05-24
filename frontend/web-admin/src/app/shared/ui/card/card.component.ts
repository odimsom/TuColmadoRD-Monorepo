import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';

@Component({
  selector: 'app-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ng-content select="[cardHeader]" />
    <ng-content />
  `,
  host: { '[class]': '_hostClass()' },
})
export class CardComponent {
  shadow = input(true);
  compact = input(false);
  tight = input(false);
  variant = input<'default' | 'primary'>('default');

  _hostClass = computed(() => [
    'tc-card',
    this.shadow() ? 'shadow' : '',
    this.compact() ? 'tc-card-compact' : '',
    this.tight() ? 'tc-card-tight' : '',
    this.variant() === 'primary' ? 'tc-card-primary' : '',
  ].filter(Boolean).join(' '));

  bodyClass = computed(() => '');
}
