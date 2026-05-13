# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: 02-dashboard.spec.ts >> Dashboard >> fiados pendientes muestra datos correctos (balance > 0)
- Location: e2e/02-dashboard.spec.ts:32:7

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
  1  | import { Page } from '@playwright/test';
  2  | 
  3  | export const TEST_USER = {
  4  |   email: 'test@tucolmadord.com',
  5  |   password: 'Test1234!',
  6  | };
  7  | 
  8  | export async function login(page: Page) {
  9  |   await page.goto('/auth/login');
  10 |   await page.waitForSelector('[data-testid="login-form"]', { timeout: 10_000 });
  11 |   await page.fill('[data-testid="login-email"]', TEST_USER.email);
  12 |   await page.fill('[data-testid="login-password"]', TEST_USER.password);
  13 |   await page.click('[data-testid="login-submit-btn"]');
> 14 |   await page.waitForURL(/\/(portal|pos)/, { timeout: 20_000 });
     |              ^ TimeoutError: page.waitForURL: Timeout 20000ms exceeded.
  15 | }
  16 | 
  17 | export async function goToPOS(page: Page) {
  18 |   await page.goto('/pos');
  19 |   await page.waitForLoadState('networkidle');
  20 | }
  21 | 
```