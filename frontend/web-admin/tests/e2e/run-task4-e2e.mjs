import { spawn, spawnSync } from 'node:child_process';

const APP_PORT = Number(process.env.E2E_APP_PORT ?? 4210);
const SERVER_URL = process.env.E2E_SERVER_URL ?? `http://127.0.0.1:${APP_PORT}`;
const STARTUP_TIMEOUT_MS = Number(process.env.E2E_SERVER_TIMEOUT_MS ?? 120000);

function spawnNpm(args, options = {}) {
  return spawn('npm', args, {
    stdio: 'inherit',
    shell: true,
    env: process.env,
    ...options,
  });
}

function delay(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function waitForServer(url, timeoutMs) {
  const start = Date.now();
  const probeUrl = `${url}/auth/login`;

  while (Date.now() - start < timeoutMs) {
    try {
      const response = await fetch(probeUrl);
      if (response.ok) {
        const html = await response.text();
        if (html.includes('<app-root') || html.includes('<!doctype html')) {
          return;
        }
      }
    } catch {
      // Keep polling while the dev server starts.
    }

    await delay(1000);
  }

  throw new Error(`Dev server did not become ready in ${timeoutMs} ms at ${probeUrl}`);
}

async function waitForServerWarmup() {
  await delay(4000);
}

function waitForExit(child) {
  return new Promise((resolve, reject) => {
    child.once('error', reject);
    child.once('close', (code) => resolve(code ?? 1));
  });
}

function stopServer(processHandle) {
  if (!processHandle || processHandle.killed) {
    return;
  }

  if (process.platform === 'win32') {
    spawnSync('taskkill', ['/PID', String(processHandle.pid), '/T', '/F'], {
      stdio: 'ignore',
      windowsHide: true,
    });
    return;
  }

  processHandle.kill('SIGTERM');
}

async function run() {
  const devServer = spawnNpm([
    'run',
    'start',
    '--',
    '--build-target',
    'web-admin:build:e2e',
    '--port',
    String(APP_PORT),
    '--host',
    '127.0.0.1',
  ]);

  let exitCode = 1;

  try {
    await waitForServer(SERVER_URL, STARTUP_TIMEOUT_MS);
    await waitForServerWarmup();

    const e2e = spawnNpm(['run', 'test:selenium:task4'], {
      env: {
        ...process.env,
        E2E_APP_URL: `${SERVER_URL}`,
        E2E_MOCK_API_PORT: '5115',
      },
    });

    exitCode = await waitForExit(e2e);
  } finally {
    stopServer(devServer);
  }

  process.exitCode = exitCode;
}

run().catch((error) => {
  console.error('Failed to run Selenium task4 e2e:', error);
  process.exitCode = 1;
});
