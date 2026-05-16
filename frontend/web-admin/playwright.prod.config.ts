import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  retries: 1,
  workers: 1,
  timeout: 60_000,
  reporter: [
    ['list'],
    ['html', { outputFolder: 'e2e/reports', open: 'never' }],
    ['json', { outputFile: 'e2e/results/test-results.json' }],
  ],
  use: {
    baseURL: 'https://app.tucolmadord.com',
    trace: 'off',
    video: 'off',
    screenshot: 'only-on-failure',
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
});
