import { LoginUseCase } from '../src/app/use-cases/login.use-case';
import { RegisterUseCase } from '../src/app/use-cases/register.use-case';
import { PairDeviceUseCase } from '../src/app/use-cases/pair-device.use-case';
import { RenewLicenseUseCase } from '../src/app/use-cases/renew-license.use-case';
import { Role } from '../src/domain/enums/role.enums';

// ─── Module mocks ─────────────────────────────────────────────────────────────

jest.mock('bcryptjs', () => ({
  compare: jest.fn(),
  hash: jest.fn(),
}));

jest.mock('jsonwebtoken', () => ({
  sign: jest.fn().mockReturnValue('mocked.jwt.token'),
}));

jest.mock('fs', () => ({
  readFileSync: jest.fn().mockReturnValue('MOCK_PRIVATE_KEY'),
}));

jest.mock('../src/config/env.config', () => ({
  envConfig: {
    jwt: { secret: 'test-secret', expiresIn: '1d' },
  },
}));

import bcrypt from 'bcryptjs';
import jwt from 'jsonwebtoken';

const mockedBcrypt = bcrypt as jest.Mocked<typeof bcrypt>;
const mockedJwt = jwt as jest.Mocked<typeof jwt>;

// ─── Shared test helpers ──────────────────────────────────────────────────────

function makeUserRepo(overrides: Partial<Record<string, jest.Mock>> = {}) {
  return {
    findByEmail: jest.fn(),
    findByEmailAndTenant: jest.fn(),
    create: jest.fn(),
    delete: jest.fn(),
    ...overrides,
  };
}

function makeTenantRepo(overrides: Partial<Record<string, jest.Mock>> = {}) {
  return {
    findById: jest.fn(),
    create: jest.fn(),
    delete: jest.fn(),
    ...overrides,
  };
}

function makeNetApi(overrides: Partial<Record<string, jest.Mock>> = {}) {
  return {
    notifyNewTenant: jest.fn(),
    ...overrides,
  };
}

function fakeUser(overrides = {}) {
  return {
    _id: 'user-id-001',
    email: 'cajero@colmado.com',
    password: '$2b$10$hashedpassword',
    role: Role.OWNER,
    tenantId: 'tenant-id-001',
    isActive: true,
    ...overrides,
  };
}

function fakeTenant(overrides = {}) {
  return {
    _id: 'tenant-id-001',
    name: 'Colmado El Buen Precio',
    isActive: true,
    ...overrides,
  };
}

// ─── LoginUseCase ─────────────────────────────────────────────────────────────

describe('LoginUseCase', () => {
  beforeEach(() => jest.clearAllMocks());

  it('TC-01: execute con credenciales válidas retorna accessToken con claims correctos', async () => {
    const user = fakeUser();
    const tenant = fakeTenant();
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(user) });
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(tenant) });

    (mockedBcrypt.compare as jest.Mock).mockResolvedValue(true);

    const useCase = new LoginUseCase(userRepo as any, tenantRepo as any);
    const result = await useCase.execute({ email: 'Cajero@Colmado.COM', password: 'secret' });

    expect(result.accessToken).toBe('mocked.jwt.token');
    expect(result.user.email).toBe(user.email);
    expect(result.user.role).toBe(Role.OWNER);
    expect(result.user.tenantId).toBe('tenant-id-001');
    expect(mockedJwt.sign).toHaveBeenCalledWith(
      expect.objectContaining({
        sub: user._id,
        tenant_id: 'tenant-id-001',
        role: Role.OWNER,
        email: user.email,
      }),
      'test-secret',
      expect.any(Object),
    );
  });

  it('TC-02: execute con contraseña inválida lanza INVALID_CREDENTIALS', async () => {
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(fakeUser()) });
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(fakeTenant()) });

    (mockedBcrypt.compare as jest.Mock).mockResolvedValue(false);

    const useCase = new LoginUseCase(userRepo as any, tenantRepo as any);

    await expect(
      useCase.execute({ email: 'cajero@colmado.com', password: 'wrong' }),
    ).rejects.toThrow('INVALID_CREDENTIALS');
  });

  it('TC-03: execute con email desconocido lanza INVALID_CREDENTIALS', async () => {
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(null) });
    const tenantRepo = makeTenantRepo();

    const useCase = new LoginUseCase(userRepo as any, tenantRepo as any);

    await expect(
      useCase.execute({ email: 'noexiste@colmado.com', password: 'pass' }),
    ).rejects.toThrow('INVALID_CREDENTIALS');
  });

  it('TC-04: execute con tenantId explícito busca usuario por email y tenant', async () => {
    const user = fakeUser();
    const tenant = fakeTenant();
    const userRepo = makeUserRepo({
      findByEmailAndTenant: jest.fn().mockResolvedValue(user),
    });
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(tenant) });

    (mockedBcrypt.compare as jest.Mock).mockResolvedValue(true);

    const useCase = new LoginUseCase(userRepo as any, tenantRepo as any);
    await useCase.execute({
      email: 'cajero@colmado.com',
      password: 'secret',
      tenantId: 'tenant-id-001',
    });

    expect(userRepo.findByEmailAndTenant).toHaveBeenCalledWith(
      'cajero@colmado.com',
      'tenant-id-001',
    );
    expect(userRepo.findByEmail).not.toHaveBeenCalled();
  });

  it('TC-04b: execute con tenantId que no existe lanza TENANT_NOT_FOUND', async () => {
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(null) });
    const userRepo = makeUserRepo();

    const useCase = new LoginUseCase(userRepo as any, tenantRepo as any);

    await expect(
      useCase.execute({ email: 'x@x.com', password: 'p', tenantId: 'inexistente' }),
    ).rejects.toThrow('TENANT_NOT_FOUND');
  });
});

// ─── RegisterUseCase ──────────────────────────────────────────────────────────

describe('RegisterUseCase', () => {
  beforeEach(() => jest.clearAllMocks());

  it('TC-05: execute con datos válidos crea tenant, usuario y retorna JWT con role=owner', async () => {
    const user = fakeUser({ _id: 'new-user-id' });
    const userRepo = makeUserRepo({ create: jest.fn().mockResolvedValue(user) });
    const tenantRepo = makeTenantRepo({ create: jest.fn().mockResolvedValue(undefined) });
    const netApi = makeNetApi({ notifyNewTenant: jest.fn().mockResolvedValue(undefined) });

    (mockedBcrypt.hash as jest.Mock).mockResolvedValue('$2b$10$hashed');

    const useCase = new RegisterUseCase(userRepo as any, tenantRepo as any, netApi as any);
    const result = await useCase.execute({
      email: 'owner@colmado.com',
      password: 'securePass',
      tenantName: 'Colmado El Buen Precio',
    });

    expect(tenantRepo.create).toHaveBeenCalledTimes(1);
    expect(userRepo.create).toHaveBeenCalledWith(
      expect.objectContaining({ role: Role.OWNER, isActive: true }),
    );
    expect(netApi.notifyNewTenant).toHaveBeenCalledTimes(1);
    expect(result.accessToken).toBe('mocked.jwt.token');
    expect(result.user.role).toBe(Role.OWNER);
  });

  it('TC-06: execute revierte usuario y tenant si NetAPI falla', async () => {
    const user = fakeUser({ _id: 'rollback-user' });
    const userRepo = makeUserRepo({ create: jest.fn().mockResolvedValue(user) });
    const tenantRepo = makeTenantRepo({ create: jest.fn().mockResolvedValue(undefined) });
    const netApi = makeNetApi({
      notifyNewTenant: jest.fn().mockRejectedValue(new Error('Network timeout')),
    });

    (mockedBcrypt.hash as jest.Mock).mockResolvedValue('$2b$10$hashed');

    const useCase = new RegisterUseCase(userRepo as any, tenantRepo as any, netApi as any);

    await expect(
      useCase.execute({ email: 'x@x.com', password: 'p', tenantName: 'Test' }),
    ).rejects.toThrow('NET_API_ERROR');

    expect(userRepo.delete).toHaveBeenCalledWith(user._id, expect.any(String));
    expect(tenantRepo.delete).toHaveBeenCalled();
  });
});

// ─── PairDeviceUseCase ────────────────────────────────────────────────────────

describe('PairDeviceUseCase', () => {
  beforeEach(() => jest.clearAllMocks());

  it('TC-07: execute con credenciales válidas retorna terminalId UUID y publicLicenseKey', async () => {
    const user = fakeUser();
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(user) });
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(fakeTenant()) });

    (mockedBcrypt.compare as jest.Mock).mockResolvedValue(true);

    const useCase = new PairDeviceUseCase(userRepo as any, tenantRepo as any);
    const result = await useCase.execute({
      email: 'cajero@colmado.com',
      password: 'secret',
    });

    expect(result.tenantId).toBe('tenant-id-001');
    expect(result.terminalId).toMatch(
      /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i,
    );
    expect(result.publicLicenseKey).toMatch(/^LIC-[A-Z0-9]+-[A-Z0-9]+$/);
  });

  it('TC-08: execute con contraseña inválida lanza INVALID_CREDENTIALS', async () => {
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(fakeUser()) });
    const tenantRepo = makeTenantRepo();

    (mockedBcrypt.compare as jest.Mock).mockResolvedValue(false);

    const useCase = new PairDeviceUseCase(userRepo as any, tenantRepo as any);

    await expect(
      useCase.execute({ email: 'cajero@colmado.com', password: 'wrong' }),
    ).rejects.toThrow('INVALID_CREDENTIALS');
  });

  it('TC-08b: execute con email desconocido lanza INVALID_CREDENTIALS', async () => {
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(null) });
    const tenantRepo = makeTenantRepo();

    const useCase = new PairDeviceUseCase(userRepo as any, tenantRepo as any);

    await expect(
      useCase.execute({ email: 'ghost@colmado.com', password: 'p' }),
    ).rejects.toThrow('INVALID_CREDENTIALS');
  });
});

// ─── RenewLicenseUseCase ──────────────────────────────────────────────────────

describe('RenewLicenseUseCase', () => {
  beforeEach(() => jest.clearAllMocks());

  it('TC-09: execute con tenant activo retorna licenseToken RS256 y validUntil en el futuro', async () => {
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(fakeTenant()) });

    const useCase = new RenewLicenseUseCase(tenantRepo as any);
    const result = await useCase.execute('tenant-id-001', 'terminal-id-001', 30);

    expect(result.licenseToken).toBe('mocked.jwt.token');
    expect(mockedJwt.sign).toHaveBeenCalledWith(
      expect.objectContaining({
        tenant_id: 'tenant-id-001',
        terminal_id: 'terminal-id-001',
      }),
      'MOCK_PRIVATE_KEY',
      { algorithm: 'RS256' },
    );
    const validUntilDate = new Date(result.validUntil);
    expect(validUntilDate.getTime()).toBeGreaterThan(Date.now());
  });

  it('TC-10: execute con tenant inactivo lanza RENEWAL_REJECTED', async () => {
    const tenantRepo = makeTenantRepo({
      findById: jest.fn().mockResolvedValue(fakeTenant({ isActive: false })),
    });

    const useCase = new RenewLicenseUseCase(tenantRepo as any);

    await expect(
      useCase.execute('tenant-id-001', 'terminal-001', 30),
    ).rejects.toThrow('RENEWAL_REJECTED');
  });

  it('TC-10b: execute con tenant inexistente lanza TENANT_NOT_FOUND', async () => {
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(null) });

    const useCase = new RenewLicenseUseCase(tenantRepo as any);

    await expect(
      useCase.execute('no-existe', 'terminal-001', 30),
    ).rejects.toThrow('TENANT_NOT_FOUND');
  });

  it('TC-09b: validUntil refleja los días configurados correctamente', async () => {
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(fakeTenant()) });
    const now = Date.now();

    const useCase = new RenewLicenseUseCase(tenantRepo as any);
    const result = await useCase.execute('tenant-id-001', 'terminal-id-001', 7);

    const validUntil = new Date(result.validUntil).getTime();
    const expectedMin = now + 6 * 24 * 60 * 60 * 1000; // al menos 6 días
    const expectedMax = now + 8 * 24 * 60 * 60 * 1000; // máximo 8 días

    expect(validUntil).toBeGreaterThan(expectedMin);
    expect(validUntil).toBeLessThan(expectedMax);
  });
});
