import assert from 'node:assert/strict';
import { createServer } from 'node:http';
import { Builder, By, until } from 'selenium-webdriver';
import chrome from 'selenium-webdriver/chrome.js';

const APP_URL = process.env.E2E_APP_URL ?? 'http://127.0.0.1:4200/auth/register';
const MOCK_API_PORT = Number(process.env.E2E_MOCK_API_PORT ?? 8080);
const TIMEOUT_MS = Number(process.env.E2E_TIMEOUT_MS ?? 30000);

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

function startMockApi(port) {
  const server = createServer((req, res) => {
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');

    if (req.method === 'OPTIONS') {
      res.statusCode = 204;
      res.end();
      return;
    }

    if (req.url === '/gateway/auth/register' && req.method === 'POST') {
      const chunks = [];
      req.on('data', (chunk) => chunks.push(chunk));
      req.on('end', () => {
        const body = Buffer.concat(chunks).toString('utf8');
        const input = body ? JSON.parse(body) : {};

        const response = {
          token: buildFakeJwt(),
          tenantId: 'tenant-e2e',
          user: {
            id: 'user-e2e',
            email: `${(input.fullName ?? 'test').toString().replace(/\s+/g, '.').toLowerCase()}@e2e.local`,
            firstName: input.fullName ?? 'Test',
            lastName: 'User',
            role: 'ADMIN',
            tenantId: 'tenant-e2e',
          },
        };

        res.setHeader('Content-Type', 'application/json');
        res.end(JSON.stringify(response));
      });
      return;
    }

    if (req.url === '/gateway/auth/login' && req.method === 'POST') {
      res.setHeader('Content-Type', 'application/json');
      res.end(
        JSON.stringify({
          token: buildFakeJwt(),
          tenantId: 'tenant-e2e',
          user: {
            id: 'user-e2e',
            email: 'login@e2e.local',
            firstName: 'Login',
            lastName: 'User',
            role: 'ADMIN',
            tenantId: 'tenant-e2e',
          },
        }),
      );
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

async function findByTestId(driver, testId) {
  return driver.wait(
    until.elementLocated(By.css(`[data-testid="${testId}"]`)),
    TIMEOUT_MS,
    `Element not found: ${testId}`,
  );
}

async function run() {
  const apiServer = await startMockApi(MOCK_API_PORT);
  let driver;

  try {
    const chromeOptions = new chrome.Options()
      .addArguments('--headless=new')
      .addArguments('--window-size=1440,900')
      .addArguments('--disable-gpu')
      .addArguments('--no-sandbox')
      .addArguments('--disable-dev-shm-usage');

    driver = await new Builder().forBrowser('chrome').setChromeOptions(chromeOptions).build();

    await driver.get(APP_URL);

    await (await findByTestId(driver, 'register-full-name')).sendKeys('Usuario Selenium');
    await (await findByTestId(driver, 'register-business-name')).sendKeys('Colmado Prueba Selenium');
    await (await findByTestId(driver, 'register-whatsapp')).sendKeys('+18095551234');
    await (await findByTestId(driver, 'register-password')).sendKeys('clave1234');

    const continueButton = await findByTestId(driver, 'register-continue-btn');
    await driver.wait(async () => continueButton.isEnabled(), TIMEOUT_MS);
    await continueButton.click();

    await findByTestId(driver, 'terms-modal');

    const createAccountButton = await findByTestId(driver, 'create-account-btn');
    const disabledBeforeAccept = await createAccountButton.getAttribute('disabled');
    assert.notEqual(
      disabledBeforeAccept,
      null,
      'Create account button should be disabled before accepting terms',
    );

    await (await findByTestId(driver, 'terms-checkbox')).click();
    await driver.wait(async () => createAccountButton.isEnabled(), TIMEOUT_MS);
    await createAccountButton.click();

    const successTitle = await findByTestId(driver, 'register-success-title');
    await driver.wait(until.elementIsVisible(successTitle), TIMEOUT_MS);

    const successText = await successTitle.getText();
    assert.match(successText, /Todo listo/i);

    const token = await driver.executeScript("return window.localStorage.getItem('tc_token');");
    assert.ok(token, 'Expected tc_token in localStorage after successful registration');

    console.log('Selenium e2e passed: account creation flow is working.');
  } finally {
    if (driver) {
      await driver.quit();
    }

    await new Promise((resolve, reject) => {
      apiServer.close((err) => (err ? reject(err) : resolve()));
    });
  }
}

run().catch((error) => {
  console.error('Selenium e2e failed:', error);
  process.exitCode = 1;
});
