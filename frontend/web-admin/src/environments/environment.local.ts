export const environment = {
  production: true,
  gatewayUrl: 'http://localhost:5100',
  isLocalInstall: true,
  downloadUrl: '',
  paymentProvider: 'mock' as 'mock' | 'stripe' | 'dlocal',
  stripePublishableKey: '',
  dlocalApiKey: '',
};
