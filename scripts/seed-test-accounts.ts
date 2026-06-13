import mongoose from 'mongoose';
import bcrypt from 'bcryptjs';
import { randomUUID } from 'crypto';
import { Client } from 'pg';

// Override via env vars to target any environment.
const MONGODB_URI = process.env.MONGODB_URI
  ?? 'mongodb://tucolmado:local1234@localhost:27017/tu_colmado_auth?authSource=admin';
const POSTGRES_URI = process.env.POSTGRES_URI
  ?? 'postgresql://postgres:1234@localhost:54329/TuColmadoDb';

// Fixed IDs — seed is fully idempotent (safe to re-run).
const EXAMPLE_TENANT_ID = 'a1000000-0000-4000-8000-000000000001';

// ─── Mongoose inline schemas (minimal — mirror the real auth service models) ──

const TenantSchema = new mongoose.Schema(
  {
    _id:                { type: String, required: true },
    name:               { type: String, required: true },
    isActive:           { type: Boolean, default: true },
    subscriptionStatus: { type: String, enum: ['active', 'trialing', 'expired'], default: 'active' },
  },
  { timestamps: true, _id: false },
);

const UserSchema = new mongoose.Schema(
  {
    tenantId:  { type: String, required: true },
    email:     { type: String, required: true, lowercase: true, trim: true },
    password:  { type: String, required: true },
    firstName: { type: String, default: null },
    lastName:  { type: String, default: null },
    role:      { type: String, enum: ['owner', 'admin', 'cashier', 'seller', 'delivery'], required: true },
    status:    { type: String, enum: ['PENDING_VERIFICATION', 'ACTIVE', 'SUSPENDED'], default: 'PENDING_VERIFICATION' },
    verificationCode:       { type: String, default: null },
    verificationCodeExpiry: { type: Date, default: null },
  },
  { timestamps: true },
);

const Tenant = mongoose.models['Tenant'] ?? mongoose.model('Tenant', TenantSchema);
const User   = mongoose.models['User']   ?? mongoose.model('User',   UserSchema);

// ─── Enum values that match the C# domain enums ───────────────────────────────
// PresentationType: 1=BulkContainer, 2=PackagedUnit
// SellMode:        1=ByUnit, 2=ByWeight
// UnitOfMeasure:   1=Pound, 10=Unit, 20=Liter, 22=Bottle

const BULK   = { presentationType: 1, sellMode: 2 }; // weighed goods
const PACKED = { presentationType: 2, sellMode: 1 }; // closed packages

const UNIT   = 10;
const POUND  = 1;
const LITER  = 20;
const BOTTLE = 22;

interface ProductDef {
  name: string;
  cat:  string;
  cost: number;
  sale: number;
  itbis: number;
  unit: number;
  presentationType: number;
  sellMode: number;
  stock: number;
}

const PRODUCTS: ProductDef[] = [
  // Alimentos y Abarrotes
  { name: 'Arroz (libra)',           cat: 'Alimentos y Abarrotes', cost: 18,  sale: 22,  itbis: 0,    unit: POUND,  ...BULK,   stock: 200 },
  { name: 'Habichuelas Rojas (lb)',  cat: 'Alimentos y Abarrotes', cost: 25,  sale: 30,  itbis: 0,    unit: POUND,  ...BULK,   stock: 100 },
  { name: 'Aceite Vegetal 1L',       cat: 'Alimentos y Abarrotes', cost: 160, sale: 185, itbis: 0.18, unit: BOTTLE, ...PACKED, stock: 48  },
  { name: 'Espaguetis 400g',         cat: 'Alimentos y Abarrotes', cost: 45,  sale: 55,  itbis: 0.18, unit: UNIT,   ...PACKED, stock: 60  },
  { name: 'Sazón Completo (sobre)',  cat: 'Alimentos y Abarrotes', cost: 8,   sale: 12,  itbis: 0,    unit: UNIT,   ...PACKED, stock: 120 },
  { name: 'Ajo (cabeza)',            cat: 'Alimentos y Abarrotes', cost: 20,  sale: 28,  itbis: 0,    unit: UNIT,   ...PACKED, stock: 80  },
  { name: 'Sal Yodada 1Kg',          cat: 'Alimentos y Abarrotes', cost: 30,  sale: 40,  itbis: 0,    unit: UNIT,   ...PACKED, stock: 50  },
  // Bebidas
  { name: 'Agua Mineral 500ml',      cat: 'Bebidas',               cost: 18,  sale: 25,  itbis: 0.18, unit: BOTTLE, ...PACKED, stock: 144 },
  { name: 'Refresco 2L',             cat: 'Bebidas',               cost: 100, sale: 130, itbis: 0.18, unit: BOTTLE, ...PACKED, stock: 36  },
  { name: 'Jugo Tampico 473ml',      cat: 'Bebidas',               cost: 35,  sale: 45,  itbis: 0.18, unit: BOTTLE, ...PACKED, stock: 72  },
  { name: 'Cerveza Presidente Lata', cat: 'Bebidas',               cost: 70,  sale: 90,  itbis: 0.18, unit: UNIT,   ...PACKED, stock: 48  },
  // Lácteos y Huevos
  { name: 'Leche Evaporada 370g',    cat: 'Lácteos y Huevos',      cost: 75,  sale: 95,  itbis: 0.18, unit: UNIT,   ...PACKED, stock: 48  },
  { name: 'Huevo (unidad)',          cat: 'Lácteos y Huevos',      cost: 14,  sale: 18,  itbis: 0,    unit: UNIT,   ...PACKED, stock: 120 },
  { name: 'Queso de Mano (libra)',   cat: 'Lácteos y Huevos',      cost: 180, sale: 220, itbis: 0,    unit: POUND,  ...BULK,   stock: 20  },
  // Carnes y Embutidos
  { name: 'Salami Induveca (libra)', cat: 'Carnes y Embutidos',    cost: 120, sale: 150, itbis: 0,    unit: POUND,  ...BULK,   stock: 30  },
  { name: 'Longaniza (libra)',       cat: 'Carnes y Embutidos',    cost: 100, sale: 130, itbis: 0,    unit: POUND,  ...BULK,   stock: 25  },
  // Limpieza y Hogar
  { name: 'Cloro 1L',                cat: 'Limpieza y Hogar',      cost: 65,  sale: 85,  itbis: 0.18, unit: LITER,  ...PACKED, stock: 30  },
  { name: 'Detergente en Polvo',     cat: 'Limpieza y Hogar',      cost: 40,  sale: 55,  itbis: 0.18, unit: UNIT,   ...PACKED, stock: 40  },
  { name: 'Papel Higiénico x4',     cat: 'Limpieza y Hogar',      cost: 120, sale: 155, itbis: 0.18, unit: UNIT,   ...PACKED, stock: 24  },
  // Higiene Personal
  { name: 'Jabón de Baño',           cat: 'Higiene Personal',      cost: 35,  sale: 45,  itbis: 0.18, unit: UNIT,   ...PACKED, stock: 50  },
  { name: 'Champú (sachet)',         cat: 'Higiene Personal',      cost: 10,  sale: 15,  itbis: 0.18, unit: UNIT,   ...PACKED, stock: 100 },
  // Snacks y Dulces
  { name: 'Galletas Soda',           cat: 'Snacks y Dulces',       cost: 25,  sale: 35,  itbis: 0.18, unit: UNIT,   ...PACKED, stock: 60  },
  { name: 'Chicle (unidad)',         cat: 'Snacks y Dulces',       cost: 2,   sale: 3,   itbis: 0.18, unit: UNIT,   ...PACKED, stock: 200 },
  { name: 'Frío Frío (sobre)',       cat: 'Snacks y Dulces',       cost: 10,  sale: 15,  itbis: 0,    unit: UNIT,   ...PACKED, stock: 150 },
];

const CATEGORIES = [
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

async function seed() {
  console.log('Iniciando seed de Compañía Ejemplo...');
  console.log('  MongoDB :', MONGODB_URI.replace(/:([^@]+)@/, ':***@'));
  console.log('  Postgres:', POSTGRES_URI.replace(/:([^@]+)@/, ':***@'));

  // ── MongoDB ─────────────────────────────────────────────────────────────────
  console.log('\n[MongoDB] Conectando...');
  await mongoose.connect(MONGODB_URI);

  const ownerHash  = await bcrypt.hash('ArroZZ12hju.,', 10);
  const staffHash  = await bcrypt.hash('Pruebas@1234', 10);

  console.log('[MongoDB] Upserting Tenant...');
  await Tenant.findOneAndUpdate(
    { _id: EXAMPLE_TENANT_ID },
    {
      _id: EXAMPLE_TENANT_ID,
      name: 'Colmado Ejemplo SRL',
      isActive: true,
      subscriptionStatus: 'active',
    },
    { upsert: true, new: true },
  );

  const users = [
    {
      tenantId:  EXAMPLE_TENANT_ID,
      email:     'borrome941@gmail.com',
      password:  ownerHash,
      firstName: 'Francisco',
      lastName:  'Castro',
      role:      'owner',
      status:    'ACTIVE',
    },
    {
      tenantId:  EXAMPLE_TENANT_ID,
      email:     'cajero@ejemplo.com',
      password:  staffHash,
      firstName: 'Cajero',
      lastName:  'Ejemplo',
      role:      'seller',
      status:    'ACTIVE',
    },
    {
      tenantId:  EXAMPLE_TENANT_ID,
      email:     'delivery@ejemplo.com',
      password:  staffHash,
      firstName: 'Mensajero',
      lastName:  'Ejemplo',
      role:      'delivery',
      status:    'ACTIVE',
    },
  ];

  console.log('[MongoDB] Upserting usuarios...');
  for (const u of users) {
    await User.findOneAndUpdate(
      { email: u.email },
      { $set: u },
      { upsert: true, new: true },
    );
  }

  console.log('[MongoDB] Usuarios listos.');
  await mongoose.disconnect();

  // ── PostgreSQL ───────────────────────────────────────────────────────────────
  console.log('\n[Postgres] Conectando...');
  const pg = new Client({ connectionString: POSTGRES_URI });
  await pg.connect();

  // TenantProfile
  console.log('[Postgres] Upserting TenantProfile...');
  await pg.query(
    `INSERT INTO "System"."TenantProfiles"
       ("Id", "TenantId", "BusinessName", "Rnc", "BusinessAddress", "Phone", "Email")
     VALUES ($1, $2, $3, $4, $5, $6, $7)
     ON CONFLICT ("TenantId") DO UPDATE SET
       "BusinessName"    = EXCLUDED."BusinessName",
       "BusinessAddress" = EXCLUDED."BusinessAddress";`,
    [
      randomUUID(),
      EXAMPLE_TENANT_ID,
      'Colmado Ejemplo SRL',
      '131111111',
      'Av. Principal #1, Santo Domingo',
      '809-555-0001',
      'borrome941@gmail.com',
    ],
  );

  // Categories
  console.log('[Postgres] Upserting categorías...');
  const catMap: Record<string, string> = {};

  for (const name of CATEGORIES) {
    // Deterministic ID based on tenant + name keeps reruns idempotent
    // without needing a unique constraint on (TenantId, Name).
    const existing = await pg.query<{ Id: string }>(
      `SELECT "Id" FROM "Inventory"."Categories" WHERE "TenantId" = $1 AND "Name" = $2 LIMIT 1`,
      [EXAMPLE_TENANT_ID, name],
    );

    if (existing.rows.length > 0) {
      catMap[name] = existing.rows[0].Id;
    } else {
      const id = randomUUID();
      await pg.query(
        `INSERT INTO "Inventory"."Categories" ("Id", "TenantId", "Name", "IsActive")
         VALUES ($1, $2, $3, true)`,
        [id, EXAMPLE_TENANT_ID, name],
      );
      catMap[name] = id;
    }
  }

  console.log(`[Postgres] ${CATEGORIES.length} categorías listas.`);

  // Products, Presentations, Stock
  console.log('[Postgres] Verificando productos...');
  const { rows: existingProducts } = await pg.query<{ cnt: string }>(
    `SELECT COUNT(*)::text as cnt FROM "Inventory"."Products" WHERE "TenantId" = $1`,
    [EXAMPLE_TENANT_ID],
  );

  if (parseInt(existingProducts[0].cnt) > 0) {
    console.log(`[Postgres] Ya existen ${existingProducts[0].cnt} productos — omitiendo.`);
  } else {
    const now = new Date().toISOString();
    console.log(`[Postgres] Insertando ${PRODUCTS.length} productos + presentaciones + stock...`);

    for (const p of PRODUCTS) {
      const productId      = randomUUID();
      const presentationId = randomUUID();

      // Product
      await pg.query(
        `INSERT INTO "Inventory"."Products"
           ("Id", "TenantId", "CategoryId", "Name", "ItbisRate", "IsActive", "CreatedAt", "UpdatedAt")
         VALUES ($1, $2, $3, $4, $5, true, $6, $6)`,
        [productId, EXAMPLE_TENANT_ID, catMap[p.cat], p.name, p.itbis, now],
      );

      // ProductPresentation (one default per product)
      await pg.query(
        `INSERT INTO "Inventory"."ProductPresentations"
           ("Id", "TenantId", "ProductId", "DisplayName", "Brand",
            "PresentationType", "SellMode", "MeasureUnit",
            "CostPrice", "SalePrice", "NominalCapacity",
            "IsActive", "CreatedAt", "UpdatedAt")
         VALUES ($1, $2, $3, $4, '', $5, $6, $7, $8, $9, NULL, true, $10, $10)`,
        [
          presentationId,
          EXAMPLE_TENANT_ID,
          productId,
          p.name,
          p.presentationType,
          p.sellMode,
          p.unit,
          p.cost,
          p.sale,
          now,
        ],
      );

      // PackagedStock — only for PackagedUnit presentations (presentationType=2)
      if (p.presentationType === 2) {
        await pg.query(
          `INSERT INTO "Inventory"."PackagedStock"
             ("Id", "TenantId", "PresentationId", "Quantity", "LastUpdatedAt")
           VALUES ($1, $2, $3, $4, $5)`,
          [randomUUID(), EXAMPLE_TENANT_ID, presentationId, p.stock, now],
        );
      }
    }

    console.log(`[Postgres] ${PRODUCTS.length} productos creados.`);
  }

  await pg.end();

  // ── Summary ──────────────────────────────────────────────────────────────────
  console.log('\n✅ Seed completado.');
  console.log('Tenant ID :', EXAMPLE_TENANT_ID);
  console.log('\nCuentas de acceso:');
  console.log('  borrome941@gmail.com   pass: ArroZZ12hju.,   role: owner    → /portal');
  console.log('  cajero@ejemplo.com     pass: Pruebas@1234    role: seller   → /pos');
  console.log('  delivery@ejemplo.com   pass: Pruebas@1234    role: delivery → delivery');
}

seed().catch(err => {
  console.error('Error durante el seed:', err);
  process.exit(1);
});
