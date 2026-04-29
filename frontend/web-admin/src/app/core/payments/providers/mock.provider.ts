import { PaymentProvider, PaymentTokenResult } from '../payment-provider.interface';

/** Dev/test provider — renders a styled fake card form, never hits a real gateway. */
export class MockPaymentProvider implements PaymentProvider {
  readonly name = 'mock';

  private container: HTMLElement | null = null;
  private cardInput: HTMLInputElement | null = null;
  private expiryInput: HTMLInputElement | null = null;
  private cvvInput: HTMLInputElement | null = null;

  async mount(container: HTMLElement): Promise<void> {
    this.destroy();
    this.container = container;

    const wrap = this.el('div', `
      display:flex;flex-direction:column;gap:12px;
      padding:20px;
      background:rgba(255,255,255,0.03);
      border:1px solid rgba(255,255,255,0.08);
      border-radius:12px;
    `);

    const badge = this.el('div', 'font-size:11px;font-weight:700;color:#f59e0b;letter-spacing:0.08em;text-transform:uppercase;');
    badge.textContent = '⚠ Modo de prueba — usa 4242 4242 4242 4242';
    wrap.appendChild(badge);

    const sep = this.el('div', 'height:1px;background:rgba(255,255,255,0.05);margin:4px 0;');
    wrap.appendChild(sep);

    this.cardInput = this.createField(wrap, 'Número de tarjeta', '4242 4242 4242 4242', '100%');

    const row = this.el('div', 'display:grid;grid-template-columns:1fr 1fr;gap:12px;');
    this.expiryInput = this.createField(row, 'Vencimiento (MM/AA)', '12/26');
    this.cvvInput    = this.createField(row, 'CVV', '123');
    wrap.appendChild(row);

    container.appendChild(wrap);
  }

  async tokenize(): Promise<PaymentTokenResult> {
    const raw = this.cardInput?.value.replace(/\s/g, '') ?? '';

    if (raw === '0000000000000000') {
      throw new Error('Tarjeta rechazada. Usa la tarjeta de prueba: 4242 4242 4242 4242');
    }
    if (raw.length < 12) {
      throw new Error('Número de tarjeta inválido.');
    }

    await new Promise(r => setTimeout(r, 700)); // simulate round-trip

    return { token: `mock_tok_${Date.now()}`, last4: raw.slice(-4), brand: 'MOCK' };
  }

  destroy(): void {
    if (this.container) this.container.innerHTML = '';
    this.container = this.cardInput = this.expiryInput = this.cvvInput = null;
  }

  private createField(
    parent: HTMLElement,
    label: string,
    placeholder: string,
    width?: string,
  ): HTMLInputElement {
    const group = this.el('div', `display:flex;flex-direction:column;gap:4px;${width ? 'width:' + width : ''}`);

    const lbl = this.el('label', 'font-size:10px;font-weight:700;color:#64748b;text-transform:uppercase;letter-spacing:0.1em;');
    lbl.textContent = label;
    group.appendChild(lbl);

    const input = document.createElement('input');
    input.type = 'text';
    input.placeholder = placeholder;
    input.value = placeholder;
    input.style.cssText = `
      background:rgba(255,255,255,0.05);
      border:1px solid rgba(255,255,255,0.10);
      border-radius:8px;padding:10px 12px;
      font-size:14px;color:#fff;font-family:monospace;
      outline:none;transition:border-color 0.15s;
    `;
    input.addEventListener('focus', () => { input.style.borderColor = 'rgba(59,130,246,0.6)'; });
    input.addEventListener('blur',  () => { input.style.borderColor = 'rgba(255,255,255,0.10)'; });
    group.appendChild(input);

    parent.appendChild(group);
    return input;
  }

  private el(tag: string, css: string): HTMLElement {
    const e = document.createElement(tag);
    e.style.cssText = css;
    return e;
  }
}
