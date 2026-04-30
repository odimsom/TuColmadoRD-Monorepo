export const environment = {
  production: false,
  gatewayUrl: 'http://localhost:5032',
  isLocalInstall: false,
  downloadUrl: 'https://github.com/odimsom/TuColmadoRD-Monorepo/releases/latest/download/TuColmadoRD-Setup.exe',
  // Payment provider: 'mock' | 'stripe' | 'dlocal'
  paymentProvider: 'mock' as 'mock' | 'stripe' | 'dlocal',
  stripePublishableKey: '',
  dlocalApiKey: '',
};
