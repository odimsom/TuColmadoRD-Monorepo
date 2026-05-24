import { Component, ChangeDetectionStrategy, input, computed, forwardRef, signal, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-input',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => InputComponent),
    multi: true,
  }],
  template: `
    <label class="form-control w-full">
      @if (label()) {
        <div class="label pb-1">
          <span class="label-text font-medium text-base-content">
            {{ label() }}
            @if (required()) { <span class="text-error ml-0.5" aria-hidden="true">*</span> }
          </span>
        </div>
      }
      <div class="relative">
        <input
          [type]="_showPassword() && type() === 'password' ? 'text' : type()"
          [placeholder]="placeholder()"
          [disabled]="_disabled()"
          [class]="_inputClass()"
          [value]="_value()"
          [attr.autocomplete]="autocomplete()"
          [attr.required]="required() ? true : null"
          (input)="_handleInput($event)"
          (blur)="_onTouched()"
        />
        @if (type() === 'password') {
          <button
            type="button"
            class="absolute right-3 top-1/2 -translate-y-1/2 text-base-content/40 hover:text-base-content transition-colors flex items-center justify-center"
            (click)="_showPassword.set(!_showPassword())"
            [attr.aria-label]="_showPassword() ? 'Ocultar contraseña' : 'Mostrar contraseña'"
          >
            <iconify-icon [attr.icon]="_showPassword() ? 'lucide:eye-off' : 'lucide:eye'" class="text-xl"></iconify-icon>
          </button>
        }
      </div>
      <div class="label pt-1 min-h-[1.25rem]">
        @if (error()) {
          <span class="label-text-alt text-error" role="alert">{{ error() }}</span>
        } @else if (hint()) {
          <span class="label-text-alt text-base-content/40">{{ hint() }}</span>
        }
      </div>
    </label>
  `,
})
export class InputComponent implements ControlValueAccessor {
  label = input('');
  type = input<'text' | 'email' | 'password' | 'number' | 'tel' | 'search'>('text');
  placeholder = input('');
  required = input(false);
  error = input('');
  hint = input('');
  autocomplete = input('');

  _value = signal('');
  _disabled = signal(false);
  _showPassword = signal(false);

  _onChange: (_: string) => void = () => {};
  _onTouched: () => void = () => {};

  _inputClass = computed(() => [
    'input input-bordered w-full',
    this.type() === 'password' ? 'pr-10' : '',
    this.error() ? 'input-error' : '',
  ].filter(Boolean).join(' '));

  writeValue(v: unknown): void { this._value.set(String(v ?? '')); }
  registerOnChange(fn: (_: string) => void): void { this._onChange = fn; }
  registerOnTouched(fn: () => void): void { this._onTouched = fn; }
  setDisabledState(d: boolean): void { this._disabled.set(d); }

  _handleInput(e: Event): void {
    const v = (e.target as HTMLInputElement).value;
    this._value.set(v);
    this._onChange(v);
  }
}
