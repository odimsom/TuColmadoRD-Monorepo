import { environment } from '../../../../environments/environment';
import { PaymentProvider, PaymentTokenResult } from '../payment-provider.interface';

declare const dlocal: any; // loaded via script tag

/**
 * dLocal Smart Fields provider — recommended for Dominican Republic merchants.
 * Sandbox dashboard: https://dashboard.dlocal.com  (use sandbox API key in environment)
 * Test cards: https://docs.dlocal.com/guides/getting-started/sandbox-testing
 */
export class DLocalPaymentProvider implements PaymentProvider {
  readonly name = 'dlocal';

  private instance: any = null;
  private fields: any = null;
  private cardField: any = null;

  async mount(container: HTMLElement): Promise<void> {
    this.destroy();
    await loadScript('https://js.dlocal.com/');

    this.instance = dlocal(environment.dlocalApiKey);
    this.fields = this.instance.fields({
      locale: 'es',
      country: 'DO', // ISO 3166-1 alpha-2 for Dominican Republic
    });

    this.cardField = this.fields.create('card', {
      style: {
        base: {
          color: '#f1f5f9',
          fontFamily: 'ui-sans-serif, system-ui, sans-serif',
          fontSize: '14px',
          '::placeholder': { color: '#475569' },
        },
        invalid: { color: '#f87171' },
      },
    });
    this.cardField.mount(container);
  }

  async tokenize(): Promise<PaymentTokenResult> {
    if (!this.instance || !this.cardField) {
      throw new Error('Formulario de pago no inicializado.');
    }
    const result = await this.instance.createToken(this.cardField);
    if (result.error) throw new Error(result.error.message ?? 'Error al procesar la tarjeta.');
    return {
      token: result.token.id,
      last4: result.token.card?.last4 ?? '????',
      brand: result.token.card?.brand ?? 'unknown',
    };
  }

  destroy(): void {
    this.cardField?.unmount();
    this.cardField = null;
    this.fields = null;
    this.instance = null;
  }
}

function loadScript(src: string): Promise<void> {
  return new Promise((resolve, reject) => {
    if (document.querySelector(`script[src="${src}"]`)) { resolve(); return; }
    const s = document.createElement('script');
    s.src = src;
    s.onload = () => resolve();
    s.onerror = () => reject(new Error(`No se pudo cargar: ${src}`));
    document.head.appendChild(s);
  });
}
