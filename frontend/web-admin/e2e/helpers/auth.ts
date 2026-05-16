import { Page, expect } from '@playwright/test';

export const TEST_USER = {
  email: 'test@tucolmadord.com',
  password: 'Test1234!',
};

export async function login(page: Page) {
  await page.goto('/auth/login');
  await page.fill('input[type="email"], input[formControlName="email"]', TEST_USER.email);
  await page.fill('input[type="password"], input[formControlName="password"]', TEST_USER.password);
  await page.click('button[type="submit"]');
  await expect(page).toHaveURL(/\/(portal|pos)/, { timeout: 15_000 });
}

export async function goToPOS(page: Page) {
  await page.goto('/pos');
  await page.waitForLoadState('networkidle');
}
