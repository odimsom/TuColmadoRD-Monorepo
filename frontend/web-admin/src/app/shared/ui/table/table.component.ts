import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="overflow-x-auto w-full">
      <table class="tc-table">
        <ng-content />
      </table>
    </div>
  `,
  host: { 'class': 'block w-full overflow-hidden' }
})
export class TableComponent {}
