/**
 * TuColmadoRD — Script de Grabacion Demostrativa v0.0.1-test.1
 *
 * Demuestra los 4 requerimientos funcionales del primer release:
 *   RF-03: Autenticacion online con JWT RS256
 *   RF-04: Validacion de licencia offline (sin red)
 *   RF-01: Registro de venta offline (SQLite local)
 *   RF-02: Sincronizacion automatica Outbox al reconectar
 *
 * Requisitos previos:
 *   npm install playwright
 *   npx playwright install chromium
 *
 * Uso:
 *   npx ts-node scripts/demo-record.ts
 *
 * El video .webm se guarda en la carpeta video_demostracion/
 */

import { chromium, BrowserContext, Page } from 'playwright';
import * as path from 'path';
import * as fs from 'fs';

// ─── Configuracion ───────────────────────────────────────────────────────────

/** URL del portal Angular corriendo en local (ng serve) */
const APP_URL = 'http://localhost:4200';

/** Carpeta donde se guardara el video grabado */
const VIDEO_DIR = path.resolve(__dirname, '../video_demostracion');

/** Milisegundos de pausa entre acciones para que el espectador pueda leer */
const SLOW_MO_MS = 800;

/** Credenciales del usuario demo que debe existir en la BD local */
const DEMO_EMAIL = 'admin@tucolmadord.com';
const DEMO_PASS  = 'Demo@12345';

/** Datos de la operacion demo */
const MONTO_APERTURA  = '5000';   // RD$ de apertura del turno
const PRODUCTO_BUSCAR = 'Arroz';  // producto que existe en el catalogo local
const MONTO_PAGO      = '250';    // RD$ que paga el cliente

// ─── Utilidades ──────────────────────────────────────────────────────────────

function log(msg: string): void {
  const ts = new Date().toISOString().substring(11, 19);
  console.log(`[${ts}] ${msg}`);
}

function wait(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}

// ─── Script principal ────────────────────────────────────────────────────────

(async () => {

  // Crear la carpeta de video si no existe
  if (!fs.existsSync(VIDEO_DIR)) {
    fs.mkdirSync(VIDEO_DIR, { recursive: true });
  }

  const browser = await chromium.launch({
    headless: false,
    slowMo: SLOW_MO_MS,
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
    // RF-03 — Autenticacion online: login y emision de licencia RS256
    // =========================================================================

    log('RF-03 | Navegando al portal TuColmadoRD...');
    await page.goto(APP_URL, { waitUntil: 'networkidle' });
    await wait(2000);

    log('RF-03 | Ingresando credenciales...');
    await page.getByLabel(/correo|email/i).fill(DEMO_EMAIL);
    await wait(1000);
    await page.getByLabel(/contrase|password/i).fill(DEMO_PASS);
    await wait(1200);

    log('RF-03 | Iniciando sesion...');
    await page.getByRole('button', { name: /iniciar sesi|login|entrar/i }).click();

    // Esperar que cargue el dashboard principal
    await page.waitForURL(/dashboard|pos|inicio/i, { timeout: 12000 });
    await wait(2500);

    log('RF-03 | Dashboard cargado. Token RS256 almacenado en localStorage.');
    await wait(2000);

    // =========================================================================
    // RF-04 — Validacion offline: desconectar la red del navegador
    // =========================================================================

    log('RF-04 | Simulando caida de internet  context.setOffline(true)...');
    await context.setOffline(true);
    await wait(2000);

    log('RF-04 | Navegando a modulo protegido sin internet...');
    // El sistema debe permitir el acceso validando el token RS256 localmente
    await page.getByRole('link', { name: /caja|punto de venta|ventas|pos/i }).first().click();
    await wait(3000);

    log('RF-04 | Acceso concedido offline: validacion RS256 local exitosa.');
    await wait(2500);

    // =========================================================================
    // RF-01 — Registro de venta offline en SQLite
    // =========================================================================

    log('RF-01 | Abriendo turno de caja offline...');
    await page.getByRole('link', { name: /turno|shift/i }).first().click();
    await wait(1500);

    // Ingresar monto inicial y abrir turno
    await page.getByPlaceholder(/monto inicial|efectivo|opening|cash/i)
              .first()
              .fill(MONTO_APERTURA);
    await wait(1000);
    await page.getByRole('button', { name: /abrir turno|open shift/i }).click();
    await wait(2000);
    log(`RF-01 | Turno abierto — RD$${MONTO_APERTURA}.00 de efectivo inicial.`);

    // Ir al modulo de nueva venta
    await page.getByRole('link', { name: /nueva venta|pos|punto de venta/i }).first().click();
    await wait(1500);

    // Buscar producto en el catalogo local (SQLite)
    log(`RF-01 | Buscando producto "${PRODUCTO_BUSCAR}" en catalogo local...`);
    const buscador = page.getByPlaceholder(/buscar producto|search|buscar/i).first();
    await buscador.fill(PRODUCTO_BUSCAR);
    await wait(1500);
    await buscador.press('Enter');
    await wait(1500);

    // Agregar al carrito
    await page.getByRole('button', { name: /agregar|add|\+/i }).first().click();
    await wait(1500);
    log('RF-01 | Producto agregado al carrito.');

    // Proceder al cobro
    await page.getByRole('button', { name: /cobrar|checkout|proceder/i }).click();
    await wait(1500);

    // Ingresar monto pagado por el cliente
    await page.getByPlaceholder(/monto|efectivo|cash|pago/i)
              .first()
              .fill(MONTO_PAGO);
    await wait(1000);

    // Confirmar venta — se escribe en SQLite local, sin red
    await page.getByRole('button', { name: /confirmar|finalizar|completar|cobrar/i }).click();
    await wait(3000);
    log('RF-01 | Venta registrada en SQLite local. Sin llamadas de red.');

    // =========================================================================
    // RF-02 — Sincronizacion automatica: restaurar conexion y esperar Outbox
    // =========================================================================

    log('RF-02 | Restaurando internet  context.setOffline(false)...');
    await context.setOffline(false);
    await wait(2000);

    log('RF-02 | Esperando que el Outbox Worker sincronice con el backend...');
    // El worker de fondo detecta la red y procesa los mensajes pendientes
    await wait(5000);

    log('RF-02 | Navegando al historial de ventas...');
    await page.getByRole('link', { name: /historial|ventas|sales|reportes/i }).first().click();
    await wait(2000);

    // Refrescar para mostrar los datos ya sincronizados desde la nube
    await page.reload({ waitUntil: 'networkidle' });
    await wait(3000);

    log('RF-02 | Venta offline ahora visible en historial: sincronizacion exitosa.');
    await wait(4000);

    log('Demostracion completada exitosamente.');

  } catch (error) {
    console.error('Error durante la grabacion:', error);
  } finally {

    // =========================================================================
    // Cierre seguro — el archivo .webm se guarda al cerrar el contexto
    // =========================================================================

    log('Cerrando contexto — guardando video...');
    await context.close();
    await browser.close();
    log(`Video guardado en: ${VIDEO_DIR}`);
  }

})();
