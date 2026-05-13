# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 01-auth.spec.ts >> Autenticación >> login exitoso redirige al portal o POS
- Location: e2e/01-auth.spec.ts:5:7

# Error details

```
TimeoutError: page.waitForURL: Timeout 20000ms exceeded.
=========================== logs ===========================
waiting for navigation until "load"
============================================================
```

# Page snapshot

```yaml
- generic [ref=e5]:
  - generic [ref=e6]:
    - generic:
      - img
    - generic [ref=e8]:
      - img [ref=e10]
      - generic [ref=e14]:
        - generic [ref=e15]: TuColmado
        - generic [ref=e16]: RD
    - generic [ref=e17]:
      - generic [ref=e18]: Sistema POS para Colmados
      - heading "Tu negocio bajo control total" [level=1] [ref=e20]:
        - text: Tu negocio
        - text: bajo
        - text: control total
      - paragraph [ref=e21]: Factura en segundos, controla tu inventario y cierra caja sin errores. Diseñado para colmados dominicanos.
      - generic [ref=e22]:
        - generic [ref=e23]: Sin internet
        - generic [ref=e24]: Soporte 24/7
        - generic [ref=e25]: 14 días gratis
        - generic [ref=e26]: Cancela cuando quieras
    - generic [ref=e27]:
      - generic [ref=e28]:
        - generic [ref=e29]:
          - generic [ref=e30]: JP
          - generic [ref=e31]:
            - paragraph [ref=e32]: Juan Pérez
            - paragraph [ref=e33]: Colmado El Buen Precio · SDO
        - paragraph [ref=e34]: "\"Antes perdía RD$3,000 al mes en errores de suma. Ahora todo me cuadra al centavo.\""
      - generic [ref=e41]:
        - paragraph [ref=e42]: Producto de
        - link "Synset Solutions" [ref=e43] [cursor=pointer]:
          - /url: https://synsetsolutions.com
          - img [ref=e44]
          - generic [ref=e47]: Synset Solutions
  - generic [ref=e48]:
    - generic [ref=e49]:
      - generic [ref=e50]:
        - heading "Bienvenido de vuelta" [level=2] [ref=e51]
        - paragraph [ref=e52]: Inicia sesión en tu cuenta para continuar.
      - generic [ref=e53]:
        - generic [ref=e54]:
          - generic [ref=e55]: Correo Electrónico
          - textbox "tu@colmado.com" [ref=e57]: test@tucolmadord.com
        - generic [ref=e58]:
          - generic [ref=e59]: Contraseña
          - textbox "••••••••" [ref=e61]: Test1234!
        - button "Verificando..." [disabled] [ref=e62]:
          - generic [ref=e64]: Verificando...
      - generic [ref=e65]:
        - paragraph [ref=e66]:
          - text: ¿No tienes cuenta?
          - link "Empieza gratis →" [ref=e67] [cursor=pointer]:
            - /url: /auth/register
        - link "Volver al inicio" [ref=e68] [cursor=pointer]:
          - /url: /
          - text: Volver al inicio
    - paragraph [ref=e70]:
      - text: TuColmadoRD ·
      - link "Synset Solutions" [ref=e71] [cursor=pointer]:
        - /url: https://synsetsolutions.com
```

# Test source

```ts
  1  | import { test, expect } from '@playwright/test';
  2  | import { TEST_USER } from './helpers/auth';
  3  | 
  4  | test.describe('Autenticación', () => {
  5  |   test('login exitoso redirige al portal o POS', async ({ page }) => {
  6  |     await page.goto('/auth/login');
  7  |     await page.waitForSelector('[data-testid="login-form"]');
  8  |     await page.screenshot({ path: 'e2e/results/auth-01-login-page.png' });
  9  | 
  10 |     await page.fill('[data-testid="login-email"]', TEST_USER.email);
  11 |     await page.fill('[data-testid="login-password"]', TEST_USER.password);
  12 |     await page.screenshot({ path: 'e2e/results/auth-02-filled.png' });
  13 | 
  14 |     await page.click('[data-testid="login-submit-btn"]');
> 15 |     await page.waitForURL(/\/(portal|pos)/, { timeout: 20_000 });
     |                ^ TimeoutError: page.waitForURL: Timeout 20000ms exceeded.
  16 |     await page.screenshot({ path: 'e2e/results/auth-03-logged-in.png' });
  17 | 
  18 |     expect(page.url()).toMatch(/\/(portal|pos)/);
  19 |   });
  20 | 
  21 |   test('login con credenciales incorrectas muestra error', async ({ page }) => {
  22 |     await page.goto('/auth/login');
  23 |     await page.waitForSelector('[data-testid="login-form"]');
  24 | 
  25 |     await page.fill('[data-testid="login-email"]', 'malo@test.com');
  26 |     await page.fill('[data-testid="login-password"]', 'wrongpass');
  27 |     await page.click('[data-testid="login-submit-btn"]');
  28 | 
  29 |     await expect(page.locator('[data-testid="login-error-alert"]')).toBeVisible({ timeout: 8_000 });
  30 |     await page.screenshot({ path: 'e2e/results/auth-04-login-error.png' });
  31 | 
  32 |     expect(page.url()).toContain('/auth/login');
  33 |   });
  34 | 
  35 |   test('usuario no verificado redirige a pantalla de verificación', async ({ page }) => {
  36 |     // Este test valida que el flujo de redirección existe aunque no haya usuario sin verificar
  37 |     await page.goto('/auth/login');
  38 |     await page.waitForSelector('[data-testid="login-form"]');
  39 |     await page.screenshot({ path: 'e2e/results/auth-05-verify-flow.png' });
  40 |     // Solo verificamos que el link de registro existe
  41 |     await expect(page.getByRole('link', { name: /Empieza gratis/i })).toBeVisible();
  42 |   });
  43 | });
  44 | 
```