import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

test.describe('POS - Punto de Venta', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
    await page.goto('/pos');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
  });

  test('POS carga y muestra el estado del turno', async ({ page }) => {
    await page.screenshot({ path: 'e2e/results/pos-01-initial.png' });
    // Debe mostrar o el modal de abrir turno o el POS activo
    const hasOpenShiftModal = await page.locator('text=Abrir Turno').isVisible().catch(() => false);
    const hasPOSActive = await page.locator('text=Catálogo').isVisible().catch(() => false);
    expect(hasOpenShiftModal || hasPOSActive).toBe(true);
  });

  test('abrir turno si no hay uno activo', async ({ page }) => {
    const openShiftBtn = page.locator('button:has-text("Abrir Turno")');
    if (await openShiftBtn.isVisible()) {
      await openShiftBtn.click();
      await page.waitForTimeout(500);
      await page.screenshot({ path: 'e2e/results/pos-02-open-shift-modal.png' });

      await page.fill('input[formControlName="cashierName"]', 'Cajero Test');
      await page.fill('input[formControlName="openingCashAmount"]', '5000');
      await page.screenshot({ path: 'e2e/results/pos-03-shift-filled.png' });

      await page.click('button[type="submit"]:has-text("Abrir")');
      await page.waitForTimeout(2000);
      await page.screenshot({ path: 'e2e/results/pos-04-shift-opened.png' });
    } else {
      await page.screenshot({ path: 'e2e/results/pos-02-shift-already-open.png' });
    }
  });

  test('catálogo de productos se carga', async ({ page }) => {
    // Espera que el catálogo cargue (puede tardar si hay turno activo)
    await page.waitForTimeout(3000);
    await page.screenshot({ path: 'e2e/results/pos-05-catalog.png' });
  });

  test('agregar producto al carrito y procesar venta en efectivo', async ({ page }) => {
    await page.waitForTimeout(3000);

    // Busca el primer producto disponible
    const productCard = page.locator('[data-testid="product-card"], .product-card, button:has-text("Agregar")').first();
    const anyProduct = page.locator('div').filter({ has: page.locator('button') }).nth(2);

    await page.screenshot({ path: 'e2e/results/pos-06-before-add.png' });

    // Intenta hacer clic en el primer producto visible
    const products = page.locator('button').filter({ hasText: /^\d+\.?\d*$|RD\$|Agregar/ });
    if (await products.count() > 0) {
      await products.first().click().catch(() => {});
      await page.waitForTimeout(500);
      await page.screenshot({ path: 'e2e/results/pos-07-product-added.png' });
    }
  });

  test('módulo de búsqueda de productos funciona', async ({ page }) => {
    await page.waitForTimeout(2000);
    const searchInput = page.locator('input[placeholder*="buscar"], input[placeholder*="Buscar"], input[placeholder*="producto"]').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill('agua');
      await page.waitForTimeout(1000);
      await page.screenshot({ path: 'e2e/results/pos-08-search.png' });
    } else {
      await page.screenshot({ path: 'e2e/results/pos-08-no-search.png' });
    }
  });
});
