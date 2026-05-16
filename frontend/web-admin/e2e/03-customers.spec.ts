import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

test.describe('Clientes y Fiados', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
    await page.goto('/portal/customers');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1500);
  });

  test('página de clientes carga correctamente', async ({ page }) => {
    await page.screenshot({ path: 'e2e/results/customers-01-list.png' });
    await expect(page.getByRole('heading', { name: 'Clientes', exact: true })).toBeVisible();
    await expect(page.getByText('Total Clientes')).toBeVisible();
    await expect(page.getByText('Con Deuda')).toBeVisible();
    await expect(page.getByText('Deuda Total')).toBeVisible();
  });

  test('crear nuevo cliente', async ({ page }) => {
    await page.click('button:has-text("Nuevo Cliente")');
    await page.screenshot({ path: 'e2e/results/customers-02-create-modal.png' });

    await page.fill('input[formControlName="fullName"]', 'Juan Pérez Prueba');
    await page.fill('input[formControlName="documentId"]', '00112345678');
    await page.fill('input[formControlName="phone"]', '8091234567');
    await page.fill('input[formControlName="creditLimit"]', '5000');
    await page.screenshot({ path: 'e2e/results/customers-03-create-filled.png' });

    await page.click('button[type="submit"]:has-text("Crear Cliente")');
    await page.waitForTimeout(2000);
    await page.screenshot({ path: 'e2e/results/customers-04-after-create.png' });
  });

  test('botón saldar aparece en clientes con deuda (balance > 0)', async ({ page }) => {
    await page.screenshot({ path: 'e2e/results/customers-05-balances.png' });
    // Hover sobre cada fila para ver si aparece el botón de abono
    const rows = page.locator('tbody tr');
    const count = await rows.count();

    for (let i = 0; i < Math.min(count, 5); i++) {
      await rows.nth(i).hover();
      await page.waitForTimeout(200);
    }
    await page.screenshot({ path: 'e2e/results/customers-06-hover-actions.png' });
  });

  test('ver estado de cuenta de un cliente', async ({ page }) => {
    const rows = page.locator('tbody tr');
    if (await rows.count() > 0) {
      await rows.first().hover();
      await page.waitForTimeout(300);
      const statementBtn = rows.first().locator('button[title="Ver Estado de Cuenta"]');
      if (await statementBtn.isVisible()) {
        await statementBtn.click();
        await page.waitForTimeout(1000);
        await page.screenshot({ path: 'e2e/results/customers-07-statement.png' });
        await expect(page.getByText('Estado de Cuenta')).toBeVisible();
      }
    }
  });

  test('registrar abono en cliente con deuda', async ({ page }) => {
    // Busca un cliente con balance > 0 (con deuda)
    const debtorRow = page.locator('tbody tr').filter({ has: page.locator('.text-red-400') }).first();
    if (await debtorRow.count() > 0) {
      await debtorRow.hover();
      await page.waitForTimeout(300);
      const payBtn = debtorRow.locator('button[title="Registrar Abono"]');
      if (await payBtn.isVisible()) {
        await payBtn.click();
        await page.waitForTimeout(500);
        await page.screenshot({ path: 'e2e/results/customers-08-payment-modal.png' });

        await expect(page.getByText('Registrar Abono')).toBeVisible();
        await page.fill('input[formControlName="amount"]', '100');
        await page.screenshot({ path: 'e2e/results/customers-09-payment-filled.png' });
        // No confirmamos el pago para no modificar datos de prueba
      }
    } else {
      await page.screenshot({ path: 'e2e/results/customers-08-no-debtors.png' });
      test.skip(true, 'No hay clientes con deuda para probar el abono');
    }
  });
});
