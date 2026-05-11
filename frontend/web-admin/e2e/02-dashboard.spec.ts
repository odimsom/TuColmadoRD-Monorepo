import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

test.describe('Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
    await page.goto('/portal/dashboard');
    await page.waitForLoadState('networkidle');
  });

  test('muestra los 5 stats cards', async ({ page }) => {
    await page.screenshot({ path: 'e2e/results/dashboard-01-stats.png' });

    const ingresos = page.getByText('Ingresos del Turno');
    const stockCritico = page.getByText('Stock Crítico');
    const fiados = page.getByText('Fiados Pendientes');
    const gastos = page.getByText('Gastos del Turno');
    const tiempo = page.getByText('Tiempo de Turno');

    await expect(ingresos).toBeVisible();
    await expect(stockCritico).toBeVisible();
    await expect(fiados).toBeVisible();
    await expect(gastos).toBeVisible();
    await expect(tiempo).toBeVisible();
  });

  test('tabla de últimas transacciones es visible', async ({ page }) => {
    await expect(page.getByText('Últimas Transacciones')).toBeVisible();
    await page.screenshot({ path: 'e2e/results/dashboard-02-transactions.png' });
  });

  test('fiados pendientes muestra datos correctos (balance > 0)', async ({ page }) => {
    // Espera que los datos de deuda carguen
    await page.waitForTimeout(2000);
    await page.screenshot({ path: 'e2e/results/dashboard-03-debt-stats.png' });
    // El stat de fiados NO debe mostrar "···" (loading infinito)
    const fiadosCard = page.locator('div').filter({ hasText: 'Fiados Pendientes' }).first();
    await expect(fiadosCard.locator('span.animate-pulse')).toHaveCount(0);
  });

  test('gastos del turno se carga (no queda en loading)', async ({ page }) => {
    await page.waitForTimeout(2000);
    await page.screenshot({ path: 'e2e/results/dashboard-04-expenses-stat.png' });
    const gastosCard = page.locator('div').filter({ hasText: 'Gastos del Turno' }).first();
    await expect(gastosCard.locator('span.animate-pulse')).toHaveCount(0);
  });
});
