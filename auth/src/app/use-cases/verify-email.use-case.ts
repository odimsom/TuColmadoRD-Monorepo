import bcrypt from "bcryptjs";
import jwt from "jsonwebtoken";
import { UserRepository } from "../../infra/repositories/user.repository";
import { TenantRepository } from "../../infra/repositories/tenant.repository";
import { EmailServiceClient } from "../services/email-service.client";
import { IAuthResponse } from "../../domain/interfaces/auth.interface";
import { UserStateMachine } from "../../domain/state-machine/user-state-machine";
import { OperationResult } from "../../domain/result/operation-result";
import {
  AuthDomainError,
  UserNotFoundError,
  NoPendingVerificationError,
  VerificationCodeExpiredError,
  InvalidVerificationCodeError,
} from "../../domain/errors/auth-domain-error";
import { envConfig } from "../../config/env.config";

export class VerifyEmailUseCase {
  constructor(
    private readonly userRepo: UserRepository,
    private readonly tenantRepo: TenantRepository,
    private readonly emailClient: EmailServiceClient,
  ) {}

  async execute(email: string, code: string): Promise<OperationResult<IAuthResponse, AuthDomainError>> {
    const normalizedEmail = email.trim().toLowerCase();

    const user = await this.userRepo.findByEmail(normalizedEmail);
    if (!user || !UserStateMachine.isVerifiable(user.status)) {
      return OperationResult.fail(new UserNotFoundError());
    }

    if (!user.verificationCode || !user.verificationCodeExpiry) {
      return OperationResult.fail(new NoPendingVerificationError());
    }

    if (new Date() > user.verificationCodeExpiry) {
      return OperationResult.fail(new VerificationCodeExpiredError());
    }

    const isValid = await bcrypt.compare(code, user.verificationCode);
    if (!isValid) {
      return OperationResult.fail(new InvalidVerificationCodeError());
    }

    const transitionResult = UserStateMachine.transition(user.status, 'VERIFY_EMAIL');
    if (!transitionResult.isOk) {
      return OperationResult.fail(transitionResult.error);
    }

    await this.userRepo.setStatus(normalizedEmail, transitionResult.value);
    await this.userRepo.clearVerificationCode(normalizedEmail);

    const tenant = await this.tenantRepo.findById(user.tenantId);

    this.emailClient
      .sendAccountVerifiedEmail(normalizedEmail, user.firstName ?? '', tenant?.name ?? '')
      .catch(() => {});
    const subscriptionStatus = tenant?.subscriptionStatus ?? 'trialing';

    const token = jwt.sign(
      {
        sub: user._id,
        tenant_id: user.tenantId,
        terminal_id: "00000000-0000-0000-0000-000000000000",
        role: user.role,
        email: user.email,
        subscription_status: subscriptionStatus,
      },
      envConfig.jwt.secret,
      { expiresIn: envConfig.jwt.expiresIn as jwt.SignOptions["expiresIn"] },
    );

    return OperationResult.ok({
      accessToken: token,
      user: {
        id: user._id,
        email: user.email,
        firstName: user.firstName ?? null,
        lastName: user.lastName ?? null,
        role: user.role,
        tenantId: user.tenantId,
        subscriptionStatus,
      },
    });
  }
}
