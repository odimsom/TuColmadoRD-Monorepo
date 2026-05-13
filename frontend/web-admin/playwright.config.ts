import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  retries: 1,
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
    actionTimeout: 10_000,
    navigationTimeout: 20_000,
  },
  outputDir: 'e2e/results',
  globalSetup: './e2e/global-setup.ts',
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: {
    command: 'ng serve --configuration=e2e --port 4200',
    url: 'http://localhost:4200',
    reuseExistingServer: true,
    timeout: 90_000,
  },
});
