import { test } from '@playwright/test';

const MOBILE_VIEWPORT = { width: 390, height: 844 };

const PAGES = [
  { name: 'dashboard',    path: '/portal/dashboard' },
  { name: 'customers',   path: '/portal/customers' },
  { name: 'inventory',   path: '/portal/inventory' },
  { name: 'expenses',    path: '/portal/expenses' },
  { name: 'deliveries',  path: '/portal/deliveries' },
  { name: 'subscription', path: '/portal/subscription' },
  { name: 'pos',         path: '/pos' },
];

// Structurally-valid JWT — Angular guard only checks payload.exp, never the signature.
const FAKE_TOKEN =
  'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9' +
  '.eyJzdWIiOiJ0ZXN0LXVzZXItaWQiLCJ0ZW5hbnRfaWQiOiJhMWIyYzNkNC1lNWY2LTc4OTAtYWJjZC1lZjEyMzQ1Njc4OTAiLCJ0ZXJtaW5hbF9pZCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsInJvbGUiOiJvd25lciIsImVtYWlsIjoidGVzdEB0dWNvbG1hZG9yZC5jb20iLCJzdWJzY3JpcHRpb25fc3RhdHVzIjoiYWN0aXZlIiwiaWF0IjoxNzc4OTUxODg1LCJleHAiOjE3ODE1NDM4ODV9' +
  '.ZmFrZXNpZ25hdHVyZV9mb3JfY2xpZW50X3NpZGVfb25seQ';

const FAKE_USER = JSON.stringify({
  id: 'test-user-id',
  email: 'test@tucolmadord.com',
  firstName: 'Test',
  lastName: 'User',
  role: 'owner',
  tenantId: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
  subscriptionStatus: 'active',
});

const TENANT_ID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890';

// Stub API responses so the auth interceptor never sees a 401.
// Pages will render their layout with empty / skeleton data.
const API_STUBS: Record<string, object> = {
  // dashboard
  'dashboard':         { data: null },
  'shift':             { id: null, status: 'open', startTime: new Date().toISOString() },
  'stats':             { revenue: 0, criticalStock: 0, pendingDebts: 0, expenses: 0, shiftDuration: '0m' },
  'transactions':      { items: [], total: 0, page: 1, pageSize: 10 },
  // customers
  'customers':         { items: [], total: 0, page: 1, pageSize: 20 },
  // inventory
  'products':          { items: [], total: 0, page: 1, pageSize: 20 },
  'categories':        { items: [] },
  // expenses
  'expenses':          { items: [], total: 0, page: 1, pageSize: 20 },
  // deliveries
  'deliveries':        { items: [], total: 0, page: 1, pageSize: 20 },
  // subscription / license
  'license':           { status: 'active', plan: 'pro', renewsAt: '2026-12-31' },
  'subscription':      { status: 'active', plan: 'pro', renewsAt: '2026-12-31' },
  // POS
  'catalog':           { items: [], total: 0 },
};

test.describe('Mobile Responsive Audit — iPhone 14 (390×844)', () => {
  test.use({ viewport: MOBILE_VIEWPORT });

  test('screenshot all pages at mobile viewport', async ({ page }) => {
    // 1. Intercept ALL requests to the API gateway and return stub 200 responses.
    await page.route('**/gateway/**', async (route) => {
      const url = route.request().url();
      // Find the matching stub key
      const stub = Object.entries(API_STUBS).find(([key]) => url.includes(key));
      if (stub) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(stub[1]),
        });
      } else {
        // For unmatched endpoints return an empty 200
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: '{}',
        });
      }
    });

    // 2. Inject auth into localStorage BEFORE Angular boots.
    await page.addInitScript(
      ({ token, user, tenantId }) => {
        localStorage.setItem('tc_token', token);
        localStorage.setItem('tc_user', user);
        localStorage.setItem('tc_tenant', tenantId);
      },
      { token: FAKE_TOKEN, user: FAKE_USER, tenantId: TENANT_ID }
    );

    // 3. Screenshot each page.
    for (const { name, path } of PAGES) {
      await page.goto(path);
      // Wait for Angular routing + data fetch stubs to settle
      await page.waitForLoadState('networkidle', { timeout: 15_000 }).catch(() => {});
      await page.waitForTimeout(2500);

      await page.screenshot({
        path: `e2e/results/mobile-${name}.png`,
        fullPage: true,
      });
    }
  });
});
