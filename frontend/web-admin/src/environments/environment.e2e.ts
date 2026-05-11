export const environment = {
  production: false,
  gatewayUrl: 'http://localhost:8081',
  isLocalInstall: false,
  downloadUrl: '',
  paymentProvider: 'mock' as 'mock' | 'stripe' | 'dlocal',
  stripePublishableKey: '',
  dlocalApiKey: '',
};
