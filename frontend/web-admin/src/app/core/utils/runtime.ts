export function isDesktopApp(): boolean {
  return typeof window !== 'undefined' && Boolean((window as Window & { process?: { versions?: { electron?: string } } }).process?.versions?.electron);
}