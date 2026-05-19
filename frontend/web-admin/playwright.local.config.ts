import { defineConfig } from '@playwright/test';
export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  retries: 0,
  workers: 1,
  timeout: 60_000,
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'off',
    video: 'off',
    screenshot: 'only-on-failure',
    headless: true,
    locale: 'es-DO',
  },
});
