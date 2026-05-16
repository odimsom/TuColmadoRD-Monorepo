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

  test('fiados pendientes muestra datos correctos (no loading infinito)', async ({ page }) => {
    // Card shows "clientes con deuda" subtitle when data has loaded
    await expect(page.getByText('clientes con deuda')).toBeVisible({ timeout: 10_000 });
    await page.screenshot({ path: 'e2e/results/dashboard-03-debt-stats.png' });
  });

  test('gastos del turno se carga (no queda en loading)', async ({ page }) => {
    // Card shows "registrados este turno" subtitle when data has loaded
    await expect(page.getByText('registrados este turno')).toBeVisible({ timeout: 10_000 });
    await page.screenshot({ path: 'e2e/results/dashboard-04-expenses-stat.png' });
  });
});
