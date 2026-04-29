import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { PaymentProvider, PaymentTokenResult } from './payment-provider.interface';
import { MockPaymentProvider } from './providers/mock.provider';
import { StripePaymentProvider } from './providers/stripe.provider';
import { DLocalPaymentProvider } from './providers/dlocal.provider';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private provider: PaymentProvider = this.build();

  /** Name of the active provider — useful for showing provider logos in the UI. */
  get providerName(): string {
    return this.provider.name;
  }

  mount(container: HTMLElement): Promise<void> {
    return this.provider.mount(container);
  }

  tokenize(): Promise<PaymentTokenResult> {
    return this.provider.tokenize();
  }

  destroy(): void {
    this.provider.destroy();
  }

  private build(): PaymentProvider {
    switch (environment.paymentProvider) {
      case 'stripe': return new StripePaymentProvider();
      case 'dlocal': return new DLocalPaymentProvider();
      default:       return new MockPaymentProvider();
    }
  }
}
