// Build "local": app corriendo en Docker local, gateway expuesto en :8081
export const environment = {
  production: true,
  gatewayUrl: 'http://localhost:8081',
};
