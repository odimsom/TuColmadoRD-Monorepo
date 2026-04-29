export interface PaymentTokenResult {
  token: string;
  last4: string;
  brand: string;
}

/**
 * Contract every payment provider must fulfill.
 * mount()    — renders the card form into a container element
 * tokenize() — validates the form and returns a one-time token; rejects with a user-displayable message
 * destroy()  — tears down SDK resources; must be called on component destroy or provider change
 */
export interface PaymentProvider {
  readonly name: string;
  mount(container: HTMLElement): Promise<void>;
  tokenize(): Promise<PaymentTokenResult>;
  destroy(): void;
}
