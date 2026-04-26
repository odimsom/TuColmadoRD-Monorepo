import mongoose from 'mongoose';
import bcrypt from 'bcryptjs';
import { randomUUID } from 'crypto';
import { Client } from 'pg';

const MONGODB_URI = 'mongodb://root:1234@localhost:27019/tu_colmado_auth?authSource=admin';
const POSTGRES_URI = 'postgresql://postgres:1234@localhost:54329/TuColmadoDb';

const tenantSchema = new mongoose.Schema({
  _id: { type: String, required: true },
  name: { type: String, required: true },
  isActive: { type: Boolean, default: true }
}, { timestamps: true });

const userSchema = new mongoose.Schema({
  tenantId:  { type: String, required: true },
  email:     { type: String, required: true, unique: true },
  password:  { type: String, required: true },
  firstName: { type: String, default: null },
  lastName:  { type: String, default: null },
  // Must match Role enum in auth service: owner | delivery | seller
  role:      { type: String, enum: ['owner', 'delivery', 'seller'], required: true },
  isActive:  { type: Boolean, default: true }
}, { timestamps: true });

const Tenant = mongoose.models.Tenant || mongoose.model('Tenant', tenantSchema);
const User   = mongoose.models.User   || mongoose.model('User',   userSchema);

async function seed() {
  console.log('Iniciando script de seed de pruebas...');

  // ── MongoDB ───────────────────────────────────────────────────────────────
  console.log('Conectando a MongoDB...');
  await mongoose.connect(MONGODB_URI);

  const tenantId     = randomUUID();
  const passwordHash = await bcrypt.hash('Pruebas@1234', 10);

  console.log('Creando Tenant en Mongo...');
  await Tenant.findOneAndUpdate(
    { _id: tenantId },
    { _id: tenantId, name: 'Colmado Pruebas SRL', isActive: true },
    { upsert: true, new: true }
  );

  console.log('Creando Usuarios en Mongo...');
  const users = [
    {
      tenantId,
      email:     'owner@pruebassrl.com',
      password:  passwordHash,
      firstName: 'Dueño',
      lastName:  'Pruebas',
      role:      'owner',    // → accede a /portal (Owner | Admin guard)
      isActive:  true
    },
    {
      tenantId,
      email:     'cajero@pruebassrl.com',
      password:  passwordHash,
      firstName: 'Cajero',
      lastName:  'Uno',
      role:      'seller',   // → accede a /pos (cajero/vendedor)
      isActive:  true
    },
    {
      tenantId,
      email:     'delivery@pruebassrl.com',
      password:  passwordHash,
      firstName: 'Mensajero',
      lastName:  'Pruebas',
      role:      'delivery', // → rol de delivery
      isActive:  true
    },
  ];

  for (const u of users) {
    await User.findOneAndUpdate(
      { email: u.email },
      { $set: u },
      { upsert: true, new: true }
    );
  }

  console.log('Usuarios creados correctamente en Mongo.');
  await mongoose.disconnect();

  // ── PostgreSQL ────────────────────────────────────────────────────────────
  console.log('Conectando a PostgreSQL...');
  const client = new Client({ connectionString: POSTGRES_URI });
  await client.connect();

  console.log('Creando TenantProfile en Postgres...');
  const tenantProfileId = randomUUID();

  const insertQuery = `
    INSERT INTO "System"."TenantProfiles"
      ("TenantProfileId", "TenantId", "BusinessName", "Rnc", "BusinessAddress", "Phone", "Email")
    VALUES ($1, $2, $3, $4, $5, $6, $7)
    ON CONFLICT ("TenantId") DO NOTHING;
  `;

  await client.query(insertQuery, [
    tenantProfileId,
    tenantId,
    'Colmado Pruebas SRL',
    '131111111',
    'Av. Principal #1, Santo Domingo',
    '809-555-0000',
    'contacto@pruebassrl.com'
  ]);

  console.log('TenantProfile creado en Postgres.');
  await client.end();

  // ── Summary ───────────────────────────────────────────────────────────────
  console.log('\n✅ Seed completado.');
  console.log('Tenant ID :', tenantId);
  console.log('Password  : Pruebas@1234\n');
  console.log('Cuentas de acceso:');
  console.log('  owner@pruebassrl.com    (owner)    → /portal');
  console.log('  cajero@pruebassrl.com   (seller)   → /pos');
  console.log('  delivery@pruebassrl.com (delivery) → app móvil/delivery');
}

seed().catch(err => {
  console.error('Error durante el seed:', err);
  process.exit(1);
});
