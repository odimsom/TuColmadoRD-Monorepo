import { LoginUseCase } from '../src/app/use-cases/login.use-case';
import { RegisterUseCase } from '../src/app/use-cases/register.use-case';
import { PairDeviceUseCase } from '../src/app/use-cases/pair-device.use-case';
import { RenewLicenseUseCase } from '../src/app/use-cases/renew-license.use-case';
import { Role } from '../src/domain/enums/role.enums';
import { UserStatus } from '../src/domain/enums/user-status.enum';
import {
  InvalidCredentialsError,
  TenantNotFoundError,
  EmailNotVerifiedError,
} from '../src/domain/errors/auth-domain-error';

jest.mock('bcryptjs', () => ({ compare: jest.fn(), hash: jest.fn() }));
jest.mock('jsonwebtoken', () => ({ sign: jest.fn().mockReturnValue('mocked.jwt.token') }));
jest.mock('fs', () => ({ readFileSync: jest.fn().mockReturnValue('MOCK_PRIVATE_KEY') }));
jest.mock('../src/config/env.config', () => ({
  envConfig: { jwt: { secret: 'test-secret', expiresIn: '1d' } },
}));

import bcrypt from 'bcryptjs';
import jwt from 'jsonwebtoken';

const mockedBcrypt = bcrypt as jest.Mocked<typeof bcrypt>;
const mockedJwt = jwt as jest.Mocked<typeof jwt>;

function makeUserRepo(overrides: Partial<Record<string, jest.Mock>> = {}) {
  return {
    findByEmail: jest.fn(),
    findByEmailAndTenant: jest.fn(),
    create: jest.fn(),
    delete: jest.fn(),
    setVerificationCode: jest.fn().mockResolvedValue(undefined),
    clearVerificationCode: jest.fn().mockResolvedValue(undefined),
    setStatus: jest.fn().mockResolvedValue(undefined),
    ...overrides,
  };
}

function makeTenantRepo(overrides: Partial<Record<string, jest.Mock>> = {}) {
  return { findById: jest.fn(), create: jest.fn(), delete: jest.fn(), ...overrides };
}

function makeNetApi(overrides: Partial<Record<string, jest.Mock>> = {}) {
  return { notifyNewTenant: jest.fn(), ...overrides };
}

function makeEmailClient(overrides: Partial<Record<string, jest.Mock>> = {}) {
  return {
    sendVerificationEmail: jest.fn().mockResolvedValue(undefined),
    sendAccountVerifiedEmail: jest.fn().mockResolvedValue(undefined),
    sendAppTutorialEmail: jest.fn().mockResolvedValue(undefined),
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
    status: UserStatus.ACTIVE,
    verificationCode: null,
    verificationCodeExpiry: null,
    ...overrides,
  };
}

function fakeTenant(overrides = {}) {
  return { _id: 'tenant-id-001', name: 'Colmado El Buen Precio', isActive: true, ...overrides };
}

describe('LoginUseCase', () => {
  beforeEach(() => jest.clearAllMocks());

  it('TC-01: credenciales válidas → accessToken con claims correctos', async () => {
    const user = fakeUser();
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(user) });
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(fakeTenant()) });
    (mockedBcrypt.compare as jest.Mock).mockResolvedValue(true);

    const result = await new LoginUseCase(userRepo as any, tenantRepo as any)
      .execute({ email: 'Cajero@Colmado.COM', password: 'secret' });

    expect(result.isOk).toBe(true);
    expect(result.value.accessToken).toBe('mocked.jwt.token');
    expect(result.value.user.role).toBe(Role.OWNER);
    expect(mockedJwt.sign).toHaveBeenCalledWith(
      expect.objectContaining({ sub: user._id, tenant_id: 'tenant-id-001' }),
      'test-secret', expect.any(Object),
    );
  });

  it('TC-02: contraseña inválida → InvalidCredentialsError', async () => {
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(fakeUser()) });
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(fakeTenant()) });
    (mockedBcrypt.compare as jest.Mock).mockResolvedValue(false);

    const result = await new LoginUseCase(userRepo as any, tenantRepo as any)
      .execute({ email: 'cajero@colmado.com', password: 'wrong' });

    expect(result.isOk).toBe(false);
    expect(result.error).toBeInstanceOf(InvalidCredentialsError);
  });

  it('TC-03: email desconocido → InvalidCredentialsError', async () => {
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(null) });
    const result = await new LoginUseCase(userRepo as any, makeTenantRepo() as any)
      .execute({ email: 'noexiste@colmado.com', password: 'pass' });

    expect(result.isOk).toBe(false);
    expect(result.error).toBeInstanceOf(InvalidCredentialsError);
  });

  it('TC-03b: usuario PENDING_VERIFICATION → EmailNotVerifiedError', async () => {
    const user = fakeUser({ status: UserStatus.PENDING_VERIFICATION });
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(user) });
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(fakeTenant()) });
    (mockedBcrypt.compare as jest.Mock).mockResolvedValue(true);

    const result = await new LoginUseCase(userRepo as any, tenantRepo as any)
      .execute({ email: 'cajero@colmado.com', password: 'secret' });

    expect(result.isOk).toBe(false);
    expect(result.error).toBeInstanceOf(EmailNotVerifiedError);
  });

  it('TC-04: tenantId explícito usa findByEmailAndTenant', async () => {
    const user = fakeUser();
    const userRepo = makeUserRepo({ findByEmailAndTenant: jest.fn().mockResolvedValue(user) });
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(fakeTenant()) });
    (mockedBcrypt.compare as jest.Mock).mockResolvedValue(true);

    await new LoginUseCase(userRepo as any, tenantRepo as any)
      .execute({ email: 'cajero@colmado.com', password: 'secret', tenantId: 'tenant-id-001' });

    expect(userRepo.findByEmailAndTenant).toHaveBeenCalledWith('cajero@colmado.com', 'tenant-id-001');
    expect(userRepo.findByEmail).not.toHaveBeenCalled();
  });

  it('TC-04b: tenantId inexistente → TenantNotFoundError', async () => {
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(null) });
    const result = await new LoginUseCase(makeUserRepo() as any, tenantRepo as any)
      .execute({ email: 'x@x.com', password: 'p', tenantId: 'inexistente' });

    expect(result.isOk).toBe(false);
    expect(result.error).toBeInstanceOf(TenantNotFoundError);
  });
});

describe('RegisterUseCase', () => {
  beforeEach(() => jest.clearAllMocks());

  it('TC-05: datos válidos → requiresVerification true y emailClient llamado', async () => {
    const user = fakeUser({ _id: 'new-user-id', status: UserStatus.PENDING_VERIFICATION });
    const userRepo = makeUserRepo({ create: jest.fn().mockResolvedValue(user) });
    const tenantRepo = makeTenantRepo({ create: jest.fn().mockResolvedValue(undefined) });
    const netApi = makeNetApi({ notifyNewTenant: jest.fn().mockResolvedValue(undefined) });
    const emailClient = makeEmailClient();
    (mockedBcrypt.hash as jest.Mock).mockResolvedValue('$2b$10$hashed');

    const result = await new RegisterUseCase(userRepo as any, tenantRepo as any, netApi as any, emailClient as any)
      .execute({ email: 'owner@colmado.com', password: 'securePass', tenantName: 'Colmado El Buen Precio' });

    expect(result.isOk).toBe(true);
    expect(result.value.requiresVerification).toBe(true);
    expect(result.value.email).toBe('owner@colmado.com');
    expect(emailClient.sendVerificationEmail).toHaveBeenCalledTimes(1);
    expect(userRepo.create).toHaveBeenCalledWith(
      expect.objectContaining({ role: Role.OWNER, status: UserStatus.PENDING_VERIFICATION }),
    );
  });

  it('TC-06: fallo en NetAPI no afecta el resultado (registro resiliente)', async () => {
    const user = fakeUser({ _id: 'rollback-user', status: UserStatus.PENDING_VERIFICATION });
    const userRepo = makeUserRepo({ create: jest.fn().mockResolvedValue(user) });
    const tenantRepo = makeTenantRepo({ create: jest.fn().mockResolvedValue(undefined) });
    const netApi = makeNetApi({ notifyNewTenant: jest.fn().mockRejectedValue(new Error('timeout')) });
    const emailClient = makeEmailClient();
    (mockedBcrypt.hash as jest.Mock).mockResolvedValue('$2b$10$hashed');

    const result = await new RegisterUseCase(userRepo as any, tenantRepo as any, netApi as any, emailClient as any)
      .execute({ email: 'x@x.com', password: 'p', tenantName: 'Test' });

    expect(result.isOk).toBe(true);
    expect(result.value.requiresVerification).toBe(true);
  });
});

describe('PairDeviceUseCase', () => {
  beforeEach(() => jest.clearAllMocks());

  it('TC-07: credenciales válidas → terminalId UUID y publicLicenseKey', async () => {
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(fakeUser()) });
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(fakeTenant()) });
    (mockedBcrypt.compare as jest.Mock).mockResolvedValue(true);

    const result = await new PairDeviceUseCase(userRepo as any, tenantRepo as any)
      .execute({ email: 'cajero@colmado.com', password: 'secret', deviceName: 'Terminal-01' });

    expect(result.tenantId).toBe('tenant-id-001');
    expect(result.terminalId).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i);
    expect(result.publicLicenseKey).toMatch(/^LIC-[A-Z0-9-]+-[A-Z0-9]+$/);
  });

  it('TC-08: contraseña inválida → lanza INVALID_CREDENTIALS', async () => {
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(fakeUser()) });
    (mockedBcrypt.compare as jest.Mock).mockResolvedValue(false);
    await expect(new PairDeviceUseCase(userRepo as any, makeTenantRepo() as any)
      .execute({ email: 'cajero@colmado.com', password: 'wrong', deviceName: 'T' })).rejects.toThrow('INVALID_CREDENTIALS');
  });

  it('TC-08b: email desconocido → lanza INVALID_CREDENTIALS', async () => {
    const userRepo = makeUserRepo({ findByEmail: jest.fn().mockResolvedValue(null) });
    await expect(new PairDeviceUseCase(userRepo as any, makeTenantRepo() as any)
      .execute({ email: 'ghost@colmado.com', password: 'p', deviceName: 'T' })).rejects.toThrow('INVALID_CREDENTIALS');
  });
});

describe('RenewLicenseUseCase', () => {
  beforeEach(() => jest.clearAllMocks());

  it('TC-09: tenant activo → licenseToken RS256 y validUntil futuro', async () => {
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(fakeTenant()) });
    const result = await new RenewLicenseUseCase(tenantRepo as any).execute('tenant-id-001', 'terminal-id-001', 30);
    expect(result.licenseToken).toBe('mocked.jwt.token');
    expect(new Date(result.validUntil).getTime()).toBeGreaterThan(Date.now());
  });

  it('TC-10: tenant inactivo → lanza RENEWAL_REJECTED', async () => {
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(fakeTenant({ isActive: false })) });
    await expect(new RenewLicenseUseCase(tenantRepo as any).execute('t', 't', 30)).rejects.toThrow('RENEWAL_REJECTED');
  });

  it('TC-10b: tenant inexistente → lanza TENANT_NOT_FOUND', async () => {
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(null) });
    await expect(new RenewLicenseUseCase(tenantRepo as any).execute('no-existe', 't', 30)).rejects.toThrow('TENANT_NOT_FOUND');
  });

  it('TC-09b: validUntil refleja días configurados', async () => {
    const tenantRepo = makeTenantRepo({ findById: jest.fn().mockResolvedValue(fakeTenant()) });
    const now = Date.now();
    const result = await new RenewLicenseUseCase(tenantRepo as any).execute('tenant-id-001', 'terminal-id-001', 7);
    const validUntil = new Date(result.validUntil).getTime();
    expect(validUntil).toBeGreaterThan(now + 6 * 24 * 60 * 60 * 1000);
    expect(validUntil).toBeLessThan(now + 8 * 24 * 60 * 60 * 1000);
  });
});
