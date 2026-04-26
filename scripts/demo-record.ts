/**
 * TuColmadoRD — Script de Grabacion Demostrativa v0.0.1-test.3
 *
 * Flujo del video:
 *   1. Registro de nuevo colmado (form → terminos → bienvenida)
 *   2. Entrar al portal → Dashboard con stats y tabla
 *   3. Explorar sidebar (colapsar, expandir, hover en links)
 *   4. RF-04: simular caida de red → indicador cambia a OFFLINE
 *            navegar entre paginas via Angular router sin recargar
 *   5. Reconexion → indicador vuelve a ONLINE
 *   6. Login independiente (RF-03) desde cero
 *
 * Auto-gestiona mock API y Angular dev server.
 * Produce un unico archivo: video_demostracion/demo-tucolmadord.webm
 *
 * Uso:
 *   npx ts-node --skipProject --compiler-options '{"module":"commonjs","esModuleInterop":true,"lib":["es2020"]}' scripts/demo-record.ts
 */

import { chromium, BrowserContext, Page } from 'playwright';
import { createServer, IncomingMessage, ServerResponse } from 'http';
import { spawn, spawnSync, ChildProcess } from 'child_process';
import * as path from 'path';
import * as fs from 'fs';

// ─── Config ──────────────────────────────────────────────────────────────────

const APP_PORT   = 4210;
const API_PORT   = 5115;
const APP_URL    = `http://127.0.0.1:${APP_PORT}`;
const VIDEO_DIR  = path.resolve(__dirname, '../video_demostracion');
const SLOW_MO    = 900;
const TIMEOUT_MS = 30_000;

// ─── Utilidades ──────────────────────────────────────────────────────────────

function log(msg: string): void {
  console.log(`[${new Date().toISOString().substring(11, 19)}] ${msg}`);
}

function wait(ms: number): Promise<void> {
  return new Promise(r => setTimeout(r, ms));
}

function base64Url(s: string): string {
  return Buffer.from(s).toString('base64')
    .replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

function fakeJwt(): string {
  const h = base64Url(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const p = base64Url(JSON.stringify({ sub: 'demo-user', exp: Math.floor(Date.now() / 1000) + 3600 }));
  return `${h}.${p}.sig`;
}

function authResponse(email: string, firstName: string) {
  return {
    token: fakeJwt(),
    tenantId: 'tenant-demo',
    user: { id: 'user-demo', email, firstName, lastName: 'Demo', role: 'ADMIN', tenantId: 'tenant-demo' },
  };
}

// ─── Mock API ────────────────────────────────────────────────────────────────

function startMockApi(port: number): Promise<import('http').Server> {
  const server = createServer((req: IncomingMessage, res: ServerResponse) => {
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');

    if (req.method === 'OPTIONS') { res.statusCode = 204; res.end(); return; }

    const chunks: Buffer[] = [];
    req.on('data', c => chunks.push(c));
    req.on('end', () => {
      const body = chunks.length ? JSON.parse(Buffer.concat(chunks).toString()) : {};
      res.setHeader('Content-Type', 'application/json');

      if (req.url === '/gateway/auth/login' && req.method === 'POST') {
        res.end(JSON.stringify(authResponse('admin@tucolmadord.com', 'Admin')));
        return;
      }
      if (req.url === '/gateway/auth/register' && req.method === 'POST') {
        const name = (body.tenantName ?? 'Demo Colmado') as string;
        res.end(JSON.stringify(authResponse(`${name.toLowerCase().replace(/\s+/g, '.')}@demo.local`, name)));
        return;
      }
      res.statusCode = 404;
      res.end(JSON.stringify({ message: 'Not Found' }));
    });
  });

  return new Promise((resolve, reject) => {
    server.once('error', reject);
    server.listen(port, '127.0.0.1', () => { log(`Mock API corriendo en :${port}`); resolve(server); });
  });
}

// ─── Angular dev server ──────────────────────────────────────────────────────

function startAngular(): ChildProcess {
  const webAdmin = path.resolve(__dirname, '../frontend/web-admin');
  const proc = spawn('npm', ['run', 'start', '--', '--build-target', 'web-admin:build:e2e', '--port', String(APP_PORT), '--host', '127.0.0.1'], {
    cwd: webAdmin, shell: true, stdio: 'pipe', env: process.env,
  });
  proc.stdout?.on('data', (d: Buffer) => {
    if (d.toString().includes('Local:')) log(`Angular listo en :${APP_PORT}`);
  });
  return proc;
}

async function waitForServer(url: string, ms: number): Promise<void> {
  const start = Date.now();
  while (Date.now() - start < ms) {
    try { const r = await fetch(url); if (r.ok) return; } catch { /* polling */ }
    await wait(1000);
  }
  throw new Error(`Servidor no respondio en ${ms}ms`);
}

function stopProcess(proc: ChildProcess): void {
  if (!proc || proc.killed) return;
  if (process.platform === 'win32') {
    spawnSync('taskkill', ['/PID', String(proc.pid), '/T', '/F'], { stdio: 'ignore', windowsHide: true });
  } else {
    proc.kill('SIGTERM');
  }
}

// ─── Acciones de demo ────────────────────────────────────────────────────────

async function clickSidebarLink(page: Page, routerLink: string): Promise<void> {
  // Usa JS click para evitar intercepciones de Playwright en modo offline
  await page.evaluate(`document.querySelector('a[routerlink="${routerLink}"]').click()`);
  await wait(1500);
}

// ─── Script principal ─────────────────────────────────────────────────────────

(async () => {

  // Limpiar videos anteriores
  if (fs.existsSync(VIDEO_DIR)) {
    for (const f of fs.readdirSync(VIDEO_DIR)) {
      if (f.endsWith('.webm')) fs.rmSync(path.join(VIDEO_DIR, f));
    }
  } else {
    fs.mkdirSync(VIDEO_DIR, { recursive: true });
  }

  log('Iniciando mock API y Angular dev server...');
  const apiServer = await startMockApi(API_PORT);
  const angularProc = startAngular();

  log('Esperando que Angular compile...');
  await waitForServer(`${APP_URL}/auth/login`, 120_000);
  await wait(3000);

  const browser = await chromium.launch({
    headless: false,
    slowMo: SLOW_MO,
    args: ['--start-maximized'],
  });

  const context: BrowserContext = await browser.newContext({
    viewport:    { width: 1920, height: 1080 },
    recordVideo: { dir: VIDEO_DIR, size: { width: 1920, height: 1080 } },
    locale:      'es-DO',
  });

  const page: Page = await context.newPage();

  try {

    // =========================================================================
    // PARTE 1 — Registro de nuevo colmado
    // =========================================================================

    log('REGISTRO | Navegando a /auth/register...');
    await page.goto(`${APP_URL}/auth/register`, { waitUntil: 'networkidle' });
    await wait(2500);

    log('REGISTRO | Llenando formulario...');
    await page.locator('[data-testid="register-business-name"]').fill('Colmado La Bendicion');
    await wait(800);
    await page.locator('[data-testid="register-email"]').fill('demo@tucolmadord.com');
    await wait(800);
    await page.locator('[data-testid="register-password"]').fill('Demo@12345');
    await wait(1200);

    log('REGISTRO | Continuando a terminos...');
    await page.locator('[data-testid="register-continue-btn"]').click();
    await page.waitForSelector('[data-testid="terms-modal"]', { timeout: TIMEOUT_MS });
    await wait(2500);

    log('REGISTRO | Aceptando terminos...');
    await page.evaluate(`
      const el = document.querySelector('[data-testid="terms-checkbox"]');
      if (el && !el.checked) { el.checked = true; el.dispatchEvent(new Event('change', { bubbles: true })); }
    `);
    await wait(1500);

    log('REGISTRO | Creando cuenta...');
    await page.evaluate(`document.querySelector('[data-testid="create-account-btn"]').click()`);
    await page.waitForURL(/\/portal\/welcome/, { timeout: TIMEOUT_MS });
    await wait(3500);
    log('REGISTRO | Pantalla de bienvenida mostrada.');

    // =========================================================================
    // PARTE 2 — Entrar al portal → Dashboard
    // =========================================================================

    log('PORTAL | Entrando al portal desde bienvenida...');
    await page.click('a[routerlink="/portal/dashboard"]');
    await page.waitForURL(/\/portal\/dashboard/, { timeout: TIMEOUT_MS });
    await wait(3000);
    log('PORTAL | Dashboard cargado con stats y tabla de transacciones.');

    // Interactuar con la UI del dashboard
    log('PORTAL | Interactuando con stats cards...');
    await page.hover('.grid .bg-pos-surface:nth-child(1)');
    await wait(1000);
    await page.hover('.grid .bg-pos-surface:nth-child(2)');
    await wait(1000);
    await page.hover('.grid .bg-pos-surface:nth-child(3)');
    await wait(1000);
    await page.hover('.grid .bg-pos-surface:nth-child(4)');
    await wait(1500);

    log('PORTAL | Escribiendo en la busqueda de transacciones...');
    await page.locator('input[placeholder="Buscar ticket..."]').fill('TC-00');
    await wait(2000);
    await page.locator('input[placeholder="Buscar ticket..."]').clear();
    await wait(1000);

    // =========================================================================
    // PARTE 3 — Sidebar: colapsar y explorar links
    // =========================================================================

    log('SIDEBAR | Colapsando sidebar...');
    await page.click('div.relative.group.cursor-pointer'); // logo/toggle button
    await wait(2500);

    log('SIDEBAR | Expandiendo sidebar...');
    await page.click('div.relative.group.cursor-pointer');
    await wait(2500);

    log('SIDEBAR | Hovering sobre links de navegacion...');
    await page.hover('a[routerlink="/portal/inventory"]');
    await wait(1000);
    await page.hover('a[routerlink="/portal/sales"]');
    await wait(1000);
    await page.hover('a[routerlink="/portal/customers"]');
    await wait(1500);

    // =========================================================================
    // PARTE 4 — RF-04: Modo offline — el indicador cambia a rojo
    // =========================================================================

    log('RF-04 | Simulando caida de internet...');
    await context.setOffline(true);
    await wait(3000); // el indicador de conexion cambia a OFFLINE (rojo)

    log('RF-04 | Navegando via Angular router sin red...');
    await clickSidebarLink(page, '/portal/dashboard');
    await wait(2000);

    // Scroll en el dashboard para mostrar que la UI sigue respondiendo
    log('RF-04 | Interactuando con dashboard offline...');
    await page.mouse.wheel(0, 300);
    await wait(1500);
    await page.mouse.wheel(0, -300);
    await wait(2000);

    log('RF-04 | Restaurando conexion...');
    await context.setOffline(false);
    await wait(3000); // el indicador vuelve a ONLINE (verde)

    // =========================================================================
    // PARTE 5 — RF-03: Login independiente desde cero
    // =========================================================================

    log('RF-03 | Cerrando sesion...');
    await page.evaluate(`document.querySelector('button.p-3.text-slate-500').click()`);
    await page.waitForURL(/\/auth\/login/, { timeout: TIMEOUT_MS });
    await wait(2500);

    log('RF-03 | Ingresando credenciales...');
    await page.locator('[data-testid="login-email"]').fill('admin@tucolmadord.com');
    await wait(1000);
    await page.locator('[data-testid="login-password"]').fill('Demo@12345');
    await wait(1200);

    log('RF-03 | Iniciando sesion...');
    await page.locator('[data-testid="login-submit-btn"]').click();
    await page.waitForURL(/\/portal\/dashboard/, { timeout: TIMEOUT_MS });
    await wait(4000);

    log('RF-03 | Dashboard cargado con sesion activa.');
    log('Demostracion completada exitosamente.');

  } catch (error) {
    console.error('Error durante la grabacion:', error);
    process.exitCode = 1;
  } finally {

    log('Cerrando contexto — guardando video...');
    await context.close();
    await browser.close();

    stopProcess(angularProc);
    await new Promise<void>(r => apiServer.close(() => r()));

    const generated = fs.readdirSync(VIDEO_DIR).find(f => f.endsWith('.webm'));
    if (generated) {
      const dest = path.join(VIDEO_DIR, 'demo-tucolmadord.webm');
      fs.renameSync(path.join(VIDEO_DIR, generated), dest);
      log(`Video guardado en: ${dest}`);
    }
  }

})();
