export function useWebAdmin() {
  const base = (import.meta.env.VITE_WEB_ADMIN_URL as string | undefined)?.replace(/\/$/, '') ?? 'https://app.tucolmadord.com'
  return {
    homeUrl: `${base}/`,
    registerUrl: `${base}/auth/register`,
    loginUrl: `${base}/auth/login`,
  }
}
