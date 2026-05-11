import { randomUUID } from "crypto";
import bcrypt from "bcryptjs";
import { RegisterDto } from "../dtos/register.dto";
import { UserRepository } from "../../infra/repositories/user.repository";
import { TenantRepository } from "../../infra/repositories/tenant.repository";
import { NetApiService } from "../services/net-api.service";
import { EmailServiceClient } from "../services/email-service.client";
import { Role } from "../../domain/enums/role.enums";
import { UserStatus } from "../../domain/enums/user-status.enum";
import { OperationResult } from "../../domain/result/operation-result";
import { AuthDomainError, EmailAlreadyExistsError, InternalAuthError } from "../../domain/errors/auth-domain-error";

export interface RegisterPendingResult {
  readonly requiresVerification: true;
  readonly email: string;
  readonly message: string;
}

function generateCode(): string {
  return Math.floor(100000 + Math.random() * 900000).toString();
}

export class RegisterUseCase {
  constructor(
    private readonly userRepo: UserRepository,
    private readonly tenantRepo: TenantRepository,
    private readonly netApi: NetApiService,
    private readonly emailClient: EmailServiceClient,
  ) {}

  async execute(dto: RegisterDto): Promise<OperationResult<RegisterPendingResult, AuthDomainError>> {
    const tenantId = randomUUID();
    const normalizedEmail = dto.email.trim().toLowerCase();

    await this.tenantRepo.create({
      _id: tenantId,
      name: dto.tenantName,
      isActive: true,
      subscriptionStatus: 'trialing',
    });

    try {
      const hashedPassword = await bcrypt.hash(dto.password, 10);
      await this.userRepo.create({
        tenantId,
        email: normalizedEmail,
        password: hashedPassword,
        role: Role.OWNER,
        status: UserStatus.PENDING_VERIFICATION,
        verificationCode: null,
        verificationCodeExpiry: null,
      });
    } catch (error: any) {
      await this.tenantRepo.delete(tenantId);
      if (error?.code === 11000) {
        return OperationResult.fail(new EmailAlreadyExistsError());
      }
      return OperationResult.fail(new InternalAuthError('Error al crear el usuario.'));
    }

    const code = generateCode();
    const expiry = new Date(Date.now() + 15 * 60 * 1000);
    const hashedCode = await bcrypt.hash(code, 8);
    await this.userRepo.setVerificationCode(normalizedEmail, hashedCode, expiry);

    await this.emailClient.sendVerificationEmail(normalizedEmail, code, dto.tenantName).catch(() => {});
    await this.netApi.notifyNewTenant({ tenantId, name: dto.tenantName, ownerEmail: normalizedEmail }).catch(() => {});

    return OperationResult.ok({
      requiresVerification: true,
      email: normalizedEmail,
      message: "Te enviamos un código de verificación a tu correo. Ingrésalo para activar tu cuenta.",
    });
  }
}
