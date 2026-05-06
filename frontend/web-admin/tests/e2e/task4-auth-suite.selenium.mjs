import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import path from 'node:path';
import { createServer } from 'node:http';
import { Builder, By, until } from 'selenium-webdriver';
import chrome from 'selenium-webdriver/chrome.js';

const APP_BASE_URL = process.env.E2E_APP_URL ?? 'http://127.0.0.1:4210';
const MOCK_API_PORT = Number(process.env.E2E_MOCK_API_PORT ?? 5115);
const TIMEOUT_MS = Number(process.env.E2E_TIMEOUT_MS ?? 30000);
const IS_HEADLESS = (process.env.E2E_HEADLESS ?? 'true').toLowerCase() !== 'false';
const STEP_DELAY_MS = Number(process.env.E2E_STEP_DELAY_MS ?? 0);
const ARTIFACTS_DIR = path.resolve('tests/e2e/artifacts');
const SCREENSHOTS_DIR = path.join(ARTIFACTS_DIR, 'screenshots');
const REPORT_FILE = path.join(ARTIFACTS_DIR, 'report-task4.html');

function delay(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function pauseBetweenSteps() {
  if (STEP_DELAY_MS > 0) {
    await delay(STEP_DELAY_MS);
  }
}

function base64Url(input) {
  return Buffer.from(input)
    .toString('base64')
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/g, '');
}

function buildFakeJwt() {
  const header = base64Url(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const payload = base64Url(
    JSON.stringify({
      sub: 'test-user',
      exp: Math.floor(Date.now() / 1000) + 3600,
    }),
  );

  return `${header}.${payload}.signature`;
}

function buildAuthResponse({ email = 'qa@e2e.local', firstName = 'QA' } = {}) {
  return {
    token: buildFakeJwt(),
    tenantId: 'tenant-e2e',
    user: {
      id: 'user-e2e',
      email,
      firstName,
      lastName: 'Automation',
      role: 'ADMIN',
      tenantId: 'tenant-e2e',
    },
  };
}

function startMockApi(port, behavior) {
  const server = createServer((req, res) => {
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');

    if (req.method === 'OPTIONS') {
      res.statusCode = 204;
      res.end();
      return;
    }

    if (req.url === '/gateway/auth/login' && req.method === 'POST') {
      if (behavior.login === 'error') {
        res.statusCode = 401;
        res.setHeader('Content-Type', 'application/json');
        res.end(JSON.stringify({ message: 'Credenciales invalidas' }));
        return;
      }

      res.setHeader('Content-Type', 'application/json');
      res.end(JSON.stringify(buildAuthResponse({ email: 'login@e2e.local', firstName: 'Login' })));
      return;
    }

    if (req.url === '/gateway/auth/register' && req.method === 'POST') {
      if (behavior.register === 'error') {
        res.statusCode = 400;
        res.setHeader('Content-Type', 'application/json');
        res.end(JSON.stringify({ message: 'No se pudo crear la cuenta de prueba' }));
        return;
      }

      const chunks = [];
      req.on('data', (chunk) => chunks.push(chunk));
      req.on('end', () => {
        const rawBody = Buffer.concat(chunks).toString('utf8');
        const input = rawBody ? JSON.parse(rawBody) : {};

        res.setHeader('Content-Type', 'application/json');
        res.end(
          JSON.stringify(
            buildAuthResponse({
              email: `${(input.fullName ?? 'test').toString().replace(/\s+/g, '.').toLowerCase()}@e2e.local`,
              firstName: input.fullName ?? 'Test',
            }),
          ),
        );
      });
      return;
    }

    res.statusCode = 404;
    res.end('Not Found');
  });

  return new Promise((resolve, reject) => {
    server.once('error', reject);
    server.listen(port, '127.0.0.1', () => resolve(server));
  });
}

async function clearSession(driver) {
  await driver.executeScript('window.localStorage.clear(); window.sessionStorage.clear();');
}

async function findByTestId(driver, testId) {
  return driver.wait(
    until.elementLocated(By.css(`[data-testid="${testId}"]`)),
    TIMEOUT_MS,
    `Element not found: ${testId}`,
  );
}

async function saveScreenshot(driver, fileName) {
  const image = await driver.takeScreenshot();
  const absolute = path.join(SCREENSHOTS_DIR, fileName);
  await fs.writeFile(absolute, image, 'base64');
  return `screenshots/${fileName}`;
}

function htmlEscape(value) {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

async function runScenario(driver, scenario, results) {
  const startedAt = Date.now();
  let screenshotPath = '';

  try {
    await scenario.run(driver);
    screenshotPath = await saveScreenshot(driver, `${scenario.id}-pass.png`);

    results.push({
      ...scenario,
      status: 'PASS',
      durationMs: Date.now() - startedAt,
      screenshotPath,
      errorMessage: '',
    });
  } catch (error) {
    screenshotPath = await saveScreenshot(driver, `${scenario.id}-fail.png`);

    results.push({
      ...scenario,
      status: 'FAIL',
      durationMs: Date.now() - startedAt,
      screenshotPath,
      errorMessage: error?.stack ?? String(error),
    });

    throw error;
  }
}

async function fillLoginForm(driver, {
  email = 'admin@tucolmadord.com',
  password = 'secreto123',
} = {}) {
  await (await findByTestId(driver, 'login-email')).clear();
  await (await findByTestId(driver, 'login-email')).sendKeys(email);
  await pauseBetweenSteps();

  await (await findByTestId(driver, 'login-password')).clear();
  await (await findByTestId(driver, 'login-password')).sendKeys(password);
  await pauseBetweenSteps();
}

async function fillRegisterForm(driver, {
  businessName = 'Colmado Suite QA',
  email = 'qa@e2e.local',
  password = 'clave1234',
} = {}) {
  await (await findByTestId(driver, 'register-business-name')).clear();
  await (await findByTestId(driver, 'register-business-name')).sendKeys(businessName);
  await pauseBetweenSteps();

  await (await findByTestId(driver, 'register-email')).clear();
  await (await findByTestId(driver, 'register-email')).sendKeys(email);
  await pauseBetweenSteps();

  await (await findByTestId(driver, 'register-password')).clear();
  await (await findByTestId(driver, 'register-password')).sendKeys(password);
  await pauseBetweenSteps();
}

async function acceptTerms(driver) {
  const checkbox = await findByTestId(driver, 'terms-checkbox');
  await driver.executeScript(
    `
      const el = arguments[0];
      if (!el.checked) {
        el.checked = true;
        el.dispatchEvent(new Event('change', { bubbles: true }));
      }
    `,
    checkbox,
  );
}

async function clickByJs(driver, element) {
  await driver.executeScript('arguments[0].click();', element);
}

async function buildReport(results) {
  const total = results.length;
  const passed = results.filter((r) => r.status === 'PASS').length;
  const failed = total - passed;

  const rows = results
    .map((result) => {
      const statusClass = result.status === 'PASS' ? 'pass' : 'fail';
      const errorCell = result.errorMessage
        ? `<pre>${htmlEscape(result.errorMessage)}</pre>`
        : '<span class="muted">-</span>';

      return `
        <tr>
          <td>${htmlEscape(result.id)}</td>
          <td>${htmlEscape(result.userStory)}</td>
          <td>${htmlEscape(result.testType)}</td>
          <td class="${statusClass}">${htmlEscape(result.status)}</td>
          <td>${result.durationMs} ms</td>
          <td><a href="${htmlEscape(result.screenshotPath)}" target="_blank">Ver evidencia</a></td>
          <td>${errorCell}</td>
        </tr>
      `;
    })
    .join('');

  const generatedAt = new Date().toISOString();

  const html = `<!doctype html>
<html lang="es">
<head>
  <meta charset="utf-8" />
  <title>Reporte Selenium - Tarea 4</title>
  <style>
    body { font-family: Arial, sans-serif; margin: 24px; background: #0f172a; color: #e2e8f0; }
    h1 { margin-bottom: 4px; }
    .meta { color: #94a3b8; margin-bottom: 20px; }
    .cards { display: flex; gap: 12px; margin-bottom: 16px; }
    .card { background: #1e293b; border: 1px solid #334155; border-radius: 8px; padding: 12px 14px; min-width: 140px; }
    .num { font-size: 22px; font-weight: bold; }
    table { width: 100%; border-collapse: collapse; background: #1e293b; }
    th, td { border: 1px solid #334155; padding: 10px; vertical-align: top; }
    th { background: #0b1222; text-align: left; }
    .pass { color: #22c55e; font-weight: bold; }
    .fail { color: #ef4444; font-weight: bold; }
    .muted { color: #94a3b8; }
    a { color: #60a5fa; }
    pre { white-space: pre-wrap; margin: 0; font-size: 12px; color: #fca5a5; }
  </style>
</head>
<body>
  <h1>Reporte Selenium - Tarea 4</h1>
  <div class="meta">Generado: ${htmlEscape(generatedAt)}</div>
  <div class="cards">
    <div class="card"><div>Total</div><div class="num">${total}</div></div>
    <div class="card"><div>Pasadas</div><div class="num">${passed}</div></div>
    <div class="card"><div>Fallidas</div><div class="num">${failed}</div></div>
  </div>
  <table>
    <thead>
      <tr>
        <th>ID</th>
        <th>Historia de Usuario</th>
        <th>Tipo</th>
        <th>Estado</th>
        <th>Duracion</th>
        <th>Captura</th>
        <th>Error</th>
      </tr>
    </thead>
    <tbody>
      ${rows}
    </tbody>
  </table>
</body>
</html>`;

  await fs.writeFile(REPORT_FILE, html, 'utf8');
}

function createScenarios() {
  return [
    {
      id: 'US01-HAPPY-LOGIN',
      userStory: 'US01 - Como colmadero quiero iniciar sesion para entrar al dashboard',
      testType: 'Camino feliz',
      run: async (driver) => {
        await driver.get(`${APP_BASE_URL}/auth/login`);
        await clearSession(driver);
        await fillLoginForm(driver);

        const submit = await findByTestId(driver, 'login-submit-btn');
        await driver.wait(async () => submit.isEnabled(), TIMEOUT_MS);
        await pauseBetweenSteps();
        await submit.click();

        await driver.wait(async () => (await driver.getCurrentUrl()).includes('/portal/dashboard'), TIMEOUT_MS);

        const token = await driver.executeScript("return window.localStorage.getItem('tc_token');");
        assert.ok(token, 'Debe guardar token en localStorage luego del login correcto');
      },
    },
    {
      id: 'US02-NEG-LOGIN',
      userStory: 'US02 - Como colmadero quiero ver error cuando credenciales son invalidas',
      testType: 'Prueba negativa',
      run: async (driver) => {
        await driver.get(`${APP_BASE_URL}/auth/login`);
        await clearSession(driver);
        await fillLoginForm(driver, {
          email: 'fail@tucolmadord.com',
          password: 'incorrecta',
        });

        await pauseBetweenSteps();
        await (await findByTestId(driver, 'login-submit-btn')).click();

        await driver.wait(async () => {
          const url = await driver.getCurrentUrl();
          const token = await driver.executeScript("return window.localStorage.getItem('tc_token');");
          const alerts = await driver.findElements(By.css('[data-testid="login-error-alert"]'));
          return alerts.length > 0 || (url.includes('/auth/login') && !token);
        }, TIMEOUT_MS);

        const token = await driver.executeScript("return window.localStorage.getItem('tc_token');");
        const finalUrl = await driver.getCurrentUrl();

        assert.equal(token, null, 'No debe crearse sesion cuando el login falla');
        assert.match(finalUrl, /\/auth\/login/i);

        const alerts = await driver.findElements(By.css('[data-testid="login-error-alert"]'));
        if (alerts.length > 0) {
          const message = await alerts[0].getText();
          assert.match(message, /Credenciales inválidas|Credenciales invalidas/i);
        }
      },
    },
    {
      id: 'US03-LIMIT-LOGIN',
      userStory: 'US03 - Como colmadero quiero validaciones minimas antes de enviar login',
      testType: 'Prueba de limites',
      run: async (driver) => {
        await driver.get(`${APP_BASE_URL}/auth/login`);
        await clearSession(driver);

        const submit = await findByTestId(driver, 'login-submit-btn');
        const disabled = await submit.getAttribute('disabled');
        assert.notEqual(disabled, null, 'Boton de login debe iniciar deshabilitado con formulario vacio');

        await (await findByTestId(driver, 'login-email')).sendKeys('correo-invalido');
        await pauseBetweenSteps();
        const stillDisabled = await submit.getAttribute('disabled');
        assert.notEqual(stillDisabled, null, 'Boton debe seguir deshabilitado con email invalido');
      },
    },
    {
      id: 'US04-HAPPY-REGISTER',
      userStory: 'US04 - Como colmadero quiero crear mi cuenta y activar sesion',
      testType: 'Camino feliz',
      run: async (driver) => {
        await driver.get(`${APP_BASE_URL}/auth/register`);
        await clearSession(driver);
        await fillRegisterForm(driver);

        const continueButton = await findByTestId(driver, 'register-continue-btn');
        await driver.wait(async () => continueButton.isEnabled(), TIMEOUT_MS);
        await pauseBetweenSteps();
        await continueButton.click();

        await findByTestId(driver, 'terms-modal');
        await acceptTerms(driver);

        const createAccountButton = await findByTestId(driver, 'create-account-btn');
        await driver.wait(async () => createAccountButton.isEnabled(), TIMEOUT_MS);
        await pauseBetweenSteps();
        await clickByJs(driver, createAccountButton);

        await driver.wait(
          async () => (await driver.getCurrentUrl()).includes('/portal/welcome'),
          TIMEOUT_MS,
          'Expected navigation to /portal/welcome after successful registration',
        );
        const token = await driver.executeScript("return window.localStorage.getItem('tc_token');");
        assert.ok(token, 'Debe guardar token en localStorage luego del registro correcto');
      },
    },
    {
      id: 'US05-NEG-LIMIT-REGISTER',
      userStory: 'US05 - Como colmadero quiero validacion de limites y manejo de error en registro',
      testType: 'Negativa + Limites',
      run: async (driver) => {
        await driver.get(`${APP_BASE_URL}/auth/register`);
        await clearSession(driver);

        await fillRegisterForm(driver, {
          businessName: 'BB',
          email: 'noemail',
          password: '123',
        });

        const continueButton = await findByTestId(driver, 'register-continue-btn');
        let disabled = await continueButton.getAttribute('disabled');
        assert.notEqual(disabled, null, 'Boton continuar debe estar deshabilitado con datos fuera de limite');

        await fillRegisterForm(driver, {
          businessName: 'Colmado Error API',
          email: 'error-api@e2e.local',
          password: 'clave1234',
        });

        await driver.wait(async () => continueButton.isEnabled(), TIMEOUT_MS);
        await pauseBetweenSteps();
        await continueButton.click();
        await acceptTerms(driver);
        const createAccountButton = await findByTestId(driver, 'create-account-btn');
        await driver.wait(async () => createAccountButton.isEnabled(), TIMEOUT_MS);
        await pauseBetweenSteps();
        await clickByJs(driver, createAccountButton);

        const errorState = await findByTestId(driver, 'register-error-state');
        await driver.wait(until.elementIsVisible(errorState), TIMEOUT_MS);
        const errorMessage = await (await findByTestId(driver, 'register-error-message')).getText();
        assert.ok(errorMessage.length > 3, 'Debe mostrarse mensaje de error cuando API responde fallo');
      },
    },
  ];
}

async function run() {
  await fs.mkdir(SCREENSHOTS_DIR, { recursive: true });

  const results = [];
  const allScenarios = createScenarios();
  const behavior = { login: 'success', register: 'success' };

  const apiServer = await startMockApi(MOCK_API_PORT, behavior);
  let driver;

  try {
    const chromeOptions = new chrome.Options()
      .addArguments('--window-size=1440,900')
      .addArguments('--disable-gpu')
      .addArguments('--no-sandbox')
      .addArguments('--disable-dev-shm-usage');

    if (IS_HEADLESS) {
      chromeOptions.addArguments('--headless=new');
    }

    driver = await new Builder().forBrowser('chrome').setChromeOptions(chromeOptions).build();

    for (const scenario of allScenarios) {
      if (scenario.id === 'US02-NEG-LOGIN') {
        behavior.login = 'error';
      } else {
        behavior.login = 'success';
      }

      if (scenario.id === 'US05-NEG-LIMIT-REGISTER') {
        behavior.register = 'error';
      } else {
        behavior.register = 'success';
      }

      await runScenario(driver, scenario, results);
    }

    await buildReport(results);

    console.log(`Selenium task4 suite passed: ${results.length} escenarios ejecutados.`);
    console.log(`Browser mode: ${IS_HEADLESS ? 'headless' : 'headed'}`);
    console.log(`Step delay: ${STEP_DELAY_MS}ms`);
    console.log(`HTML report: ${REPORT_FILE}`);
  } finally {
    if (driver) {
      await driver.quit();
    }

    await new Promise((resolve, reject) => {
      apiServer.close((err) => (err ? reject(err) : resolve()));
    });
  }
}

run().catch(async (error) => {
  console.error('Selenium task4 suite failed:', error);
  process.exitCode = 1;
});
