import { test, expect } from '@playwright/test';
import { login } from './helpers/auth';

test.describe('Licencia y Suscripción', () => {
  test('página de suscripción carga y muestra los planes', async ({ page }) => {
    await login(page);
    await page.goto('/portal/subscription');
    await page.waitForLoadState('networkidle');
    await page.screenshot({ path: 'e2e/results/license-01-subscription-page.png' });

    await expect(page.getByRole('heading', { name: /Básico/i })).toBeVisible();
    await expect(page.getByRole('heading', { name: /Pro/i })).toBeVisible();
  });

  test('usuario activo accede al portal sin bloqueo de licencia', async ({ page }) => {
    await login(page);
    await page.goto('/portal/dashboard');
    await page.waitForLoadState('networkidle');
    await page.screenshot({ path: 'e2e/results/license-02-active-user-portal.png' });

    // Should not be redirected or show expired message
    expect(page.url()).toContain('/portal');
    const expiredBanner = page.getByText(/licencia expirada|suscripción expirada|expired/i);
    await expect(expiredBanner).not.toBeVisible();
  });

  test('usuario activo puede acceder al POS', async ({ page }) => {
    await login(page);
    await page.goto('/pos');
    await page.waitForLoadState('networkidle');
    await page.screenshot({ path: 'e2e/results/license-03-active-pos.png' });

    // POS should load (at minimum show "Abrir turno" or the catalog)
    const posContent = page.locator('text=Abrir turno').or(
      page.locator('text=Catálogo').or(page.locator('text=Buscar producto'))
    );
    await expect(posContent.first()).toBeVisible({ timeout: 10_000 });
  });

  test('estado de suscripción visible en el portal (active)', async ({ page }) => {
    await login(page);
    await page.goto('/portal/subscription');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await page.screenshot({ path: 'e2e/results/license-04-subscription-status.png' });

    // The page should show some subscription info or plan options
    const hasPlans = await page.getByText(/Básico|Pro|RD\$/i).count() > 0;
    expect(hasPlans).toBe(true);
  });

  test('botón de contacto WhatsApp existe en página de suscripción', async ({ page }) => {
    await login(page);
    await page.goto('/portal/subscription');
    await page.waitForLoadState('networkidle');
    await page.screenshot({ path: 'e2e/results/license-05-whatsapp-contact.png' });

    // Should have WhatsApp link for subscription upgrade
    const waLink = page.locator('a[href*="wa.me"], a[href*="whatsapp"]');
    await expect(waLink.first()).toBeVisible({ timeout: 5_000 });
  });
});
