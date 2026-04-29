import { environment } from '../../../../environments/environment';
import { PaymentProvider, PaymentTokenResult } from '../payment-provider.interface';

declare const Stripe: any; // loaded via script tag

/** Stripe Elements provider — mounts a single CardElement iframe. */
export class StripePaymentProvider implements PaymentProvider {
  readonly name = 'stripe';

  private stripe: any = null;
  private elements: any = null;
  private cardElement: any = null;

  async mount(container: HTMLElement): Promise<void> {
    this.destroy();
    await loadScript('https://js.stripe.com/v3/');

    this.stripe = Stripe(environment.stripePublishableKey);
    this.elements = this.stripe.elements({
      appearance: {
        theme: 'night',
        variables: {
          colorPrimary: '#3b82f6',
          colorBackground: '#0f172a',
          colorText: '#f1f5f9',
          borderRadius: '8px',
        },
      },
    });
    this.cardElement = this.elements.create('card');
    this.cardElement.mount(container);
  }

  async tokenize(): Promise<PaymentTokenResult> {
    if (!this.stripe || !this.cardElement) {
      throw new Error('Formulario de pago no inicializado.');
    }
    const { token, error } = await this.stripe.createToken(this.cardElement);
    if (error) throw new Error(error.message ?? 'Error al procesar la tarjeta.');
    return {
      token: token.id,
      last4: token.card?.last4 ?? '????',
      brand: token.card?.brand ?? 'unknown',
    };
  }

  destroy(): void {
    this.cardElement?.unmount();
    this.cardElement = null;
    this.elements = null;
    this.stripe = null;
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
