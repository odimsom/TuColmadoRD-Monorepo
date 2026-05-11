import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  retries: 0,
  workers: 1,
  reporter: [
    ['list'],
    ['html', { outputFolder: 'e2e/reports', open: 'never' }],
  ],
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on',
    video: 'on',
    screenshot: 'on',
    headless: true,
    locale: 'es-DO',
  },
  outputDir: 'e2e/results',
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  // Levanta el dev server si no está corriendo
  webServer: {
    command: 'npm run start',
    url: 'http://localhost:4200',
    reuseExistingServer: true,
    timeout: 60_000,
  },
});
