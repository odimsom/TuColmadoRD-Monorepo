import bcrypt from "bcryptjs";
import { UserRepository } from "../../infra/repositories/user.repository";
import { TenantRepository } from "../../infra/repositories/tenant.repository";
import { EmailServiceClient } from "../services/email-service.client";
import { UserStateMachine } from "../../domain/state-machine/user-state-machine";
import { OperationResult } from "../../domain/result/operation-result";
import { AuthDomainError } from "../../domain/errors/auth-domain-error";

function generateCode(): string {
  return Math.floor(100000 + Math.random() * 900000).toString();
}

export class ResendVerificationUseCase {
  constructor(
    private readonly userRepo: UserRepository,
    private readonly tenantRepo: TenantRepository,
    private readonly emailClient: EmailServiceClient,
  ) {}

  async execute(email: string): Promise<OperationResult<void, AuthDomainError>> {
    const normalizedEmail = email.trim().toLowerCase();
    const user = await this.userRepo.findByEmail(normalizedEmail);

    if (!user || !UserStateMachine.isVerifiable(user.status)) {
      return OperationResult.ok(undefined);
    }

    const tenant = await this.tenantRepo.findById(user.tenantId);
    const businessName = tenant?.name ?? "tu negocio";

    const code = generateCode();
    const expiry = new Date(Date.now() + 15 * 60 * 1000);
    const hashedCode = await bcrypt.hash(code, 8);
    await this.userRepo.setVerificationCode(normalizedEmail, hashedCode, expiry);
    await this.emailClient.sendVerificationEmail(normalizedEmail, code, businessName);

    return OperationResult.ok(undefined);
  }
}
