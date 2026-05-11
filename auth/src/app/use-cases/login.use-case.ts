import bcrypt from "bcryptjs";
import jwt from "jsonwebtoken";
import { LoginDto } from "../dtos/login.dto";
import { IAuthResponse } from "../../domain/interfaces/auth.interface";
import { UserRepository } from "../../infra/repositories/user.repository";
import { TenantRepository } from "../../infra/repositories/tenant.repository";
import { UserStateMachine } from "../../domain/state-machine/user-state-machine";
import { UserStatus } from "../../domain/enums/user-status.enum";
import { OperationResult } from "../../domain/result/operation-result";
import {
  AuthDomainError,
  InvalidCredentialsError,
  TenantNotFoundError,
  EmailNotVerifiedError,
  AccountSuspendedError,
} from "../../domain/errors/auth-domain-error";
import { envConfig } from "../../config/env.config";

export class LoginUseCase {
  constructor(
    private readonly userRepo: UserRepository,
    private readonly tenantRepo: TenantRepository,
  ) {}

  async execute(dto: LoginDto): Promise<OperationResult<IAuthResponse, AuthDomainError>> {
    const normalizedEmail = dto.email.trim().toLowerCase();
    const incomingTenantId = dto.tenantId?.trim();

    let resolvedTenantId: string;
    let subscriptionStatus: import("../../domain/interfaces/tenant.interface").SubscriptionStatus = 'active';
    let user;

    if (incomingTenantId) {
      const tenant = await this.tenantRepo.findById(incomingTenantId);
      if (!tenant) {
        return OperationResult.fail(new TenantNotFoundError());
      }
      resolvedTenantId = incomingTenantId;
      subscriptionStatus = tenant.subscriptionStatus ?? 'active';
      user = await this.userRepo.findByEmailAndTenant(normalizedEmail, resolvedTenantId);
    } else {
      user = await this.userRepo.findByEmail(normalizedEmail);
      if (!user) {
        return OperationResult.fail(new InvalidCredentialsError());
      }
      resolvedTenantId = user.tenantId;
      const tenant = await this.tenantRepo.findById(resolvedTenantId);
      if (!tenant) {
        return OperationResult.fail(new TenantNotFoundError());
      }
      subscriptionStatus = tenant.subscriptionStatus ?? 'active';
    }

    if (!user) {
      return OperationResult.fail(new InvalidCredentialsError());
    }

    const isValid = await bcrypt.compare(dto.password, user.password);
    if (!isValid) {
      return OperationResult.fail(new InvalidCredentialsError());
    }

    if (!UserStateMachine.canLogin(user.status)) {
      return OperationResult.fail(
        user.status === UserStatus.PENDING_VERIFICATION
          ? new EmailNotVerifiedError()
          : new AccountSuspendedError(),
      );
    }

    const token = jwt.sign(
      {
        sub: user._id,
        tenant_id: resolvedTenantId,
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
        tenantId: resolvedTenantId,
        subscriptionStatus,
      },
    });
  }
}
