export function useWebAdmin() {
  const base = (import.meta.env.VITE_WEB_ADMIN_URL as string | undefined)?.replace(/\/$/, '') ?? 'https://app.tucolmadord.com'
  const whatsapp = (import.meta.env.VITE_WHATSAPP_URL as string | undefined) ?? 'https://wa.me/18296932458?text=Hola%2C+quiero+solicitar+mi+cupo+en+TuColmadoRD'

  return {
    homeUrl: `${base}/`,
    registerUrl: `${base}/auth/register`,
    loginUrl: `${base}/auth/login`,
    whatsappUrl: whatsapp,
  }
}
