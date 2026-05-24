import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';

@Component({
  selector: 'app-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ng-content select="[cardHeader]" />
    <div [class]="bodyClass()">
      <ng-content />
    </div>
  `,
  host: { '[class]': '_hostClass()' },
})
export class CardComponent {
  shadow = input(true);
  bordered = input(false);
  compact = input(false);

  _hostClass = computed(() => [
    'card bg-base-100',
    this.shadow() ? 'shadow' : '',
    this.bordered() ? 'border border-base-300' : '',
  ].filter(Boolean).join(' '));

  bodyClass = computed(() =>
    this.compact() ? 'card-body p-4' : 'card-body'
  );
}
