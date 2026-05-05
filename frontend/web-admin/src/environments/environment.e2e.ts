export const environment = {
  production: false,
  gatewayUrl: 'http://127.0.0.1:5115',
  isLocalInstall: true,
  downloadUrl: '',
  paymentProvider: 'mock' as 'mock' | 'stripe' | 'dlocal',
  stripePublishableKey: '',
  dlocalApiKey: '',
};
