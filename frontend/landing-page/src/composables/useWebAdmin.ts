export function useWebAdmin() {
  const base = (import.meta.env.VITE_WEB_ADMIN_URL as string | undefined)?.replace(/\/$/, '') || 'http://localhost:4200'
  return {
    homeUrl: `${base}/`,
    registerUrl: `${base}/auth/register`
  }
}
