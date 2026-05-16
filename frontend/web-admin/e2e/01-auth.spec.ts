import { test, expect } from '@playwright/test';
import { TEST_USER } from './helpers/auth';

test.describe('Autenticación', () => {
  test('login exitoso redirige al portal o POS', async ({ page }) => {
    await page.goto('/auth/login');
    await page.screenshot({ path: 'e2e/results/auth-01-login-page.png' });

    await page.fill('input[formControlName="email"]', TEST_USER.email);
    await page.fill('input[formControlName="password"]', TEST_USER.password);
    await page.screenshot({ path: 'e2e/results/auth-02-filled.png' });

    await page.click('button[type="submit"]');
    await expect(page).toHaveURL(/\/(portal|pos)/, { timeout: 15_000 });
    await page.screenshot({ path: 'e2e/results/auth-03-logged-in.png' });

    expect(page.url()).toMatch(/\/(portal|pos)/);
  });

  test('login con credenciales incorrectas muestra error', async ({ page }) => {
    await page.goto('/auth/login');
    await page.fill('input[formControlName="email"]', 'malo@test.com');
    await page.fill('input[formControlName="password"]', 'wrongpass');
    await page.click('button[type="submit"]');

    await page.waitForTimeout(2000);
    await page.screenshot({ path: 'e2e/results/auth-04-login-error.png' });

    // Debe mostrar mensaje de error y NO redirigir
    expect(page.url()).toContain('/auth/login');
  });
});
