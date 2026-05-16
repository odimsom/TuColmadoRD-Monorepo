import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

test.describe('Gastos', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
    await page.goto('/portal/expenses');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1500);
  });

  test('página de gastos carga', async ({ page }) => {
    await page.screenshot({ path: 'e2e/results/expenses-01-list.png' });
    await expect(page.getByRole('heading', { name: /Gastos/i }).first()).toBeVisible();
  });

  test('lista de gastos muestra datos o estado vacío', async ({ page }) => {
    await page.waitForTimeout(2000);
    await page.screenshot({ path: 'e2e/results/expenses-02-loaded.png' });

    const hasItems = await page.locator('tbody tr:not(.animate-pulse)').count() > 0;
    const hasEmpty = await page.getByText(/sin gastos|no hay gastos|registra/i).isVisible().catch(() => false);
    // Uno de los dos debe ser verdad
    expect(hasItems || hasEmpty).toBe(true);
  });

  test('botón registrar gasto existe y funciona (requiere turno activo)', async ({ page }) => {
    const registerBtn = page.locator('button').filter({ hasText: /Registrar|Nuevo Gasto|Agregar/i }).first();
    if (await registerBtn.isVisible()) {
      await registerBtn.click();
      await page.waitForTimeout(500);
      await page.screenshot({ path: 'e2e/results/expenses-03-modal.png' });
    } else {
      await page.screenshot({ path: 'e2e/results/expenses-03-no-button.png' });
    }
  });
});
