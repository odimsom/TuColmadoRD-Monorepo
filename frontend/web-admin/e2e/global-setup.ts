import { execSync } from 'child_process';
import { writeFileSync, unlinkSync } from 'fs';
import { tmpdir } from 'os';
import { join } from 'path';
import type { FullConfig } from '@playwright/test';

const SEED_JS = `
var d = db.getSiblingDB('tu_colmado_auth');
d.tenants.updateOne(
  { _id: ObjectId('507f1f77bcf86cd799430001') },
  { $set: { name: 'Colmado E2E', isActive: true, subscriptionStatus: 'trialing' } },
  { upsert: true }
);
d.users.updateOne(
  { email: 'test@tucolmadord.com' },
  { $set: {
    email: 'test@tucolmadord.com',
    password: '$2b$10$66XFK98Nq4Ok14iVWR4kzeoKXmV7mFee/Rf57aOLRSO7Hd91Zd036',
    firstName: 'Test',
    lastName: 'E2E',
    role: 'OWNER',
    tenantId: '507f1f77bcf86cd799430001',
    status: 'ACTIVE',
    verificationCode: null,
    verificationCodeExpiry: null
  }},
  { upsert: true }
);
print('seed ok');
`;

function run(cmd: string): boolean {
  try {
    execSync(cmd, { stdio: 'pipe', timeout: 15_000 });
    return true;
  } catch {
    return false;
  }
}

export default async function globalSetup(_config: FullConfig) {
  const tmpFile = join(tmpdir(), 'seed_e2e.js');
  writeFileSync(tmpFile, SEED_JS);

  const mongoUri = 'mongodb://tucolmadord:ArroZZ12hju.,@localhost:27017/admin';
  const container = 'tucolmadord-mongo-1';

  // Try direct docker exec first (user terminal), then sg docker (CI / restricted shell)
  const copied =
    run(`docker cp ${tmpFile} ${container}:/tmp/seed_e2e.js`) ||
    run(`sg docker -c "docker cp ${tmpFile} ${container}:/tmp/seed_e2e.js"`);

  if (copied) {
    const seeded =
      run(`docker exec ${container} mongosh '${mongoUri}' /tmp/seed_e2e.js`) ||
      run(`sg docker -c "docker exec ${container} mongosh '${mongoUri}' /tmp/seed_e2e.js"`);

    if (seeded) {
      console.log('[setup] Usuario de prueba sembrado en MongoDB ✓');
    } else {
      console.warn('[setup] Seed falló al ejecutar mongosh');
    }
  } else {
    console.warn('[setup] No se pudo copiar seed a MongoDB — los tests que requieren login fallarán');
  }

  try { unlinkSync(tmpFile); } catch { /* ignore */ }
}
