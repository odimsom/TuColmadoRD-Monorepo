import mongoose from 'mongoose';
import bcrypt from 'bcryptjs';
import { randomUUID } from 'crypto';
import { Client } from 'pg';

const MONGODB_URI = 'mongodb://root:1234@localhost:27019/tu_colmado_auth?authSource=admin';
const POSTGRES_URI = 'postgresql://postgres:1234@localhost:54329/TuColmadoDb';

const tenantSchema = new mongoose.Schema({
  _id: { type: String, required: true },
  name: { type: String, required: true },
  isActive: { type: Boolean, default: true },
  subscriptionStatus: { type: String, enum: ['active', 'trialing', 'expired'], default: 'trialing' }
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
    { _id: tenantId, name: 'Colmado Pruebas SRL', isActive: true, subscriptionStatus: 'trialing' },
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
      ("Id", "TenantId", "BusinessName", "Rnc", "BusinessAddress", "Phone", "Email")
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

  console.log('Creando Categorías de Inventario en Postgres...');
  const categoryNames = [
    'Alimentos y Abarrotes',
    'Bebidas',
    'Lácteos y Huevos',
    'Carnes y Embutidos',
    'Limpieza y Hogar',
    'Higiene Personal',
    'Snacks y Dulces',
    'Congelados',
    'Otros',
  ];

  // Pre-generate IDs so we can reference them in products
  const catMap: Record<string, string> = {};
  for (const name of categoryNames) catMap[name] = randomUUID();

  for (const [name, id] of Object.entries(catMap)) {
    await client.query(
      `INSERT INTO "Inventory"."Categories" ("Id", "TenantId", "Name", "IsActive")
       VALUES ($1, $2, $3, true)
       ON CONFLICT DO NOTHING;`,
      [id, tenantId, name]
    );
  }

  console.log(`${categoryNames.length} categorías creadas.`);

  // ── Productos de prueba ───────────────────────────────────────────────────
  console.log('Creando Productos de prueba en Postgres...');

  // UnitType: 1=Unidad, 2=Libra, 3=Kilogramo, 4=Litro, 5=Galón, 6=Caja, 7=Docena
  const products = [
    // Alimentos y Abarrotes
    { name: 'Arroz',                   cat: 'Alimentos y Abarrotes', cost: 18,  sale: 22,  itbis: 0,    unit: 2, stock: 200 },
    { name: 'Habichuelas Rojas',       cat: 'Alimentos y Abarrotes', cost: 25,  sale: 30,  itbis: 0,    unit: 2, stock: 100 },
    { name: 'Aceite Vegetal 1L',       cat: 'Alimentos y Abarrotes', cost: 160, sale: 185, itbis: 0.18, unit: 4, stock: 48  },
    { name: 'Espaguetis 400g',         cat: 'Alimentos y Abarrotes', cost: 45,  sale: 55,  itbis: 0.18, unit: 1, stock: 60  },
    { name: 'Sazón Completo (sobre)',  cat: 'Alimentos y Abarrotes', cost: 8,   sale: 12,  itbis: 0,    unit: 1, stock: 120 },
    { name: 'Ajo (cabeza)',            cat: 'Alimentos y Abarrotes', cost: 20,  sale: 28,  itbis: 0,    unit: 1, stock: 80  },
    { name: 'Sal Yodada 1Kg',         cat: 'Alimentos y Abarrotes', cost: 30,  sale: 40,  itbis: 0,    unit: 3, stock: 50  },
    // Bebidas
    { name: 'Agua Mineral 500ml',      cat: 'Bebidas',              cost: 18,  sale: 25,  itbis: 0.18, unit: 1, stock: 144 },
    { name: 'Refresco 2L',             cat: 'Bebidas',              cost: 100, sale: 130, itbis: 0.18, unit: 1, stock: 36  },
    { name: 'Jugo Tampico 473ml',      cat: 'Bebidas',              cost: 35,  sale: 45,  itbis: 0.18, unit: 1, stock: 72  },
    { name: 'Cerveza Presidente Lata', cat: 'Bebidas',              cost: 70,  sale: 90,  itbis: 0.18, unit: 1, stock: 48  },
    // Lácteos y Huevos
    { name: 'Leche Evaporada 370g',    cat: 'Lácteos y Huevos',    cost: 75,  sale: 95,  itbis: 0.18, unit: 1, stock: 48  },
    { name: 'Huevo (unidad)',          cat: 'Lácteos y Huevos',    cost: 14,  sale: 18,  itbis: 0,    unit: 1, stock: 120 },
    { name: 'Queso de Mano',           cat: 'Lácteos y Huevos',    cost: 180, sale: 220, itbis: 0,    unit: 2, stock: 20  },
    // Carnes y Embutidos
    { name: 'Salami Induveca',         cat: 'Carnes y Embutidos',  cost: 120, sale: 150, itbis: 0,    unit: 2, stock: 30  },
    { name: 'Longaniza',               cat: 'Carnes y Embutidos',  cost: 100, sale: 130, itbis: 0,    unit: 2, stock: 25  },
    // Limpieza y Hogar
    { name: 'Cloro 1L',               cat: 'Limpieza y Hogar',    cost: 65,  sale: 85,  itbis: 0.18, unit: 4, stock: 30  },
    { name: 'Detergente en Polvo',     cat: 'Limpieza y Hogar',    cost: 40,  sale: 55,  itbis: 0.18, unit: 1, stock: 40  },
    { name: 'Papel Higiénico x4',     cat: 'Limpieza y Hogar',    cost: 120, sale: 155, itbis: 0.18, unit: 1, stock: 24  },
    // Higiene Personal
    { name: 'Jabón de Baño',           cat: 'Higiene Personal',    cost: 35,  sale: 45,  itbis: 0.18, unit: 1, stock: 50  },
    { name: 'Champú (sachet)',         cat: 'Higiene Personal',    cost: 10,  sale: 15,  itbis: 0.18, unit: 1, stock: 100 },
    // Snacks y Dulces
    { name: 'Galletas Soda',           cat: 'Snacks y Dulces',     cost: 25,  sale: 35,  itbis: 0.18, unit: 1, stock: 60  },
    { name: 'Chicle (unidad)',         cat: 'Snacks y Dulces',     cost: 2,   sale: 3,   itbis: 0.18, unit: 1, stock: 200 },
    { name: 'Frío Frío (sobre)',       cat: 'Snacks y Dulces',     cost: 10,  sale: 15,  itbis: 0,    unit: 1, stock: 150 },
  ];

  const now = new Date().toISOString();
  for (const p of products) {
    await client.query(
      `INSERT INTO "Inventory"."Products"
         ("Id", "TenantId", "Name", "CategoryId", "CostPrice", "SalePrice", "ItbisRate",
          "UnitType", "IsActive", "CreatedAt", "UpdatedAt", "StockQuantity")
       VALUES ($1, $2, $3, $4, $5, $6, $7, $8, true, $9, $9, $10)
       ON CONFLICT DO NOTHING;`,
      [randomUUID(), tenantId, p.name, catMap[p.cat], p.cost, p.sale, p.itbis, p.unit, now, p.stock]
    );
  }

  console.log(`${products.length} productos creados.`);
  await client.end();

  // ── Summary ───────────────────────────────────────────────────────────────
  console.log('\n✅ Seed completado.');
  console.log('Tenant ID :', tenantId);
  console.log('Password  : Pruebas@1234\n');
  console.log('Cuentas de acceso:');
  console.log('  owner@pruebassrl.com    (owner)    → /portal');
  console.log('  cajero@pruebassrl.com   (seller)   → /pos');
  console.log('  delivery@pruebassrl.com (delivery) → app móvil/delivery');
  console.log(`\n9 categorías + ${products.length} productos de prueba creados.`);
}

seed().catch(err => {
  console.error('Error durante el seed:', err);
  process.exit(1);
});
