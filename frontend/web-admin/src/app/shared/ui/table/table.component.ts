import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="overflow-x-auto w-full">
      <table class="table table-zebra w-full">
        <ng-content />
      </table>
    </div>
  `,
})
export class TableComponent {}
