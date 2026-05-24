import { Component, ChangeDetectionStrategy, input, output, viewChild, ElementRef, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'app-modal',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <dialog #dialog class="modal">
      <div class="modal-box">
        @if (title()) {
          <h3 class="font-bold text-lg mb-4 text-base-content">{{ title() }}</h3>
        }
        <button
          class="btn btn-sm btn-circle btn-ghost absolute right-4 top-4 flex items-center justify-center"
          (click)="close()"
          aria-label="Cerrar"
        >
          <iconify-icon icon="lucide:x" class="text-xl"></iconify-icon>
        </button>
        <ng-content />
        <div class="modal-action">
          <ng-content select="[modalActions]" />
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button (click)="close()">cerrar</button>
      </form>
    </dialog>
  `,
})
export class ModalComponent {
  title = input('');
  closed = output<void>();

  private dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  open(): void {
    this.dialog().nativeElement.showModal();
  }

  close(): void {
    this.dialog().nativeElement.close();
    this.closed.emit();
  }
}
