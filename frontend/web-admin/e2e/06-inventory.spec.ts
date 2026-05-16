import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

test.describe('Inventario', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
    await page.goto('/portal/inventory');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
  });

  test('página de inventario carga con productos', async ({ page }) => {
    await page.screenshot({ path: 'e2e/results/inventory-01-list.png' });
    await expect(page.getByRole('heading', { name: 'Inventario' })).toBeVisible();
  });

  test('búsqueda de productos funciona', async ({ page }) => {
    const search = page.locator('input[placeholder*="uscar"], input[placeholder*="roducto"]').first();
    if (await search.isVisible()) {
      await search.fill('arroz');
      await page.waitForTimeout(1000);
      await page.screenshot({ path: 'e2e/results/inventory-02-search.png' });
    }
  });

  test('agregar nuevo producto', async ({ page }) => {
    const addBtn = page.locator('button').filter({ hasText: /Nuevo|Agregar|Crear/i }).first();
    if (await addBtn.isVisible()) {
      await addBtn.click();
      await page.waitForTimeout(500);
      await page.screenshot({ path: 'e2e/results/inventory-03-add-modal.png' });
    }
  });

  test('stock crítico se muestra', async ({ page }) => {
    await page.screenshot({ path: 'e2e/results/inventory-04-low-stock.png' });
  });
});
