export const environment = {
  production: true,
  gatewayUrl: 'https://api.tucolmadord.synsetsolutions.com',
  isLocalInstall: false,
  downloadUrl: 'https://github.com/synsetsolutions/TuColmadoRD-Monorepo/releases/latest/download/TuColmadoRD-Setup.exe',
  paymentProvider: 'dlocal' as 'mock' | 'stripe' | 'dlocal',
  stripePublishableKey: '',
  dlocalApiKey: '', // Set to dLocal live key before deploying
};
