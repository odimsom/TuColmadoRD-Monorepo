export const environment = {
  production: true,
  gatewayUrl: 'https://api.tucolmadord.com',
  isLocalInstall: false,
  downloadUrl: 'https://github.com/odimsom/TuColmadoRD-Monorepo/releases/latest/download/TuColmadoRD-Setup.exe',
  paymentProvider: 'dlocal' as 'mock' | 'stripe' | 'dlocal',
  stripePublishableKey: '',
  dlocalApiKey: '', // Set to dLocal live key before deploying
};
