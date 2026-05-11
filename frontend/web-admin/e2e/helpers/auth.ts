import { Page } from '@playwright/test';

export const TEST_USER = {
  email: 'test@tucolmadord.com',
  password: 'Test1234!',
};

export async function login(page: Page) {
  await page.goto('/auth/login');
  await page.fill('input[type="email"], input[formControlName="email"]', TEST_USER.email);
  await page.fill('input[type="password"], input[formControlName="password"]', TEST_USER.password);
  await page.click('button[type="submit"]');
  await page.waitForURL(/\/(portal|pos)/, { timeout: 10_000 });
}

export async function goToPOS(page: Page) {
  await page.goto('/pos');
  await page.waitForLoadState('networkidle');
}
