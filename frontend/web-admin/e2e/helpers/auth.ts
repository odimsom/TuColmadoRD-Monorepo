import { Page } from '@playwright/test';

export const TEST_USER = {
  email: 'test@tucolmadord.com',
  password: 'Test1234!',
};

export async function login(page: Page) {
  await page.goto('/auth/login');
  await page.waitForSelector('[data-testid="login-form"]', { timeout: 10_000 });
  await page.fill('[data-testid="login-email"]', TEST_USER.email);
  await page.fill('[data-testid="login-password"]', TEST_USER.password);
  await page.click('[data-testid="login-submit-btn"]');
  await page.waitForURL(/\/(portal|pos)/, { timeout: 20_000 });
}

export async function goToPOS(page: Page) {
  await page.goto('/pos');
  await page.waitForLoadState('networkidle');
}
