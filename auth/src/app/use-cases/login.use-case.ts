import bcrypt from "bcryptjs";
import jwt from "jsonwebtoken";
import { LoginDto } from "../dtos/login.dto";
import { IAuthResponse } from "../../domain/interfaces/auth.interface";
import { UserRepository } from "../../infra/repositories/user.repository";
import { TenantRepository } from "../../infra/repositories/tenant.repository";
import { envConfig } from "../../config/env.config";

export class LoginUseCase {
  constructor(
    private readonly userRepo: UserRepository,
    private readonly tenantRepo: TenantRepository,
  ) {}

  async execute(dto: LoginDto): Promise<IAuthResponse> {
    const normalizedEmail = dto.email.trim().toLowerCase();
    const incomingTenantId = dto.tenantId?.trim();

    let resolvedTenantId: string;
    let user;

    if (incomingTenantId) {
      const tenant = await this.tenantRepo.findById(incomingTenantId);
      if (!tenant) {
        throw new Error("TENANT_NOT_FOUND");
      }

      resolvedTenantId = incomingTenantId;
      user = await this.userRepo.findByEmailAndTenant(
        normalizedEmail,
        resolvedTenantId,
      );
    } else {
      user = await this.userRepo.findByEmail(normalizedEmail);
      if (!user) {
        throw new Error("INVALID_CREDENTIALS");
      }

      resolvedTenantId = user.tenantId;

      const tenant = await this.tenantRepo.findById(resolvedTenantId);
      if (!tenant) {
        throw new Error("TENANT_NOT_FOUND");
      }
    }

    if (!user) {
      throw new Error("INVALID_CREDENTIALS");
    }

    const isValid = await bcrypt.compare(dto.password, user.password);
    if (!isValid) {
      throw new Error("INVALID_CREDENTIALS");
    }

    const token = jwt.sign(
      {
        sub: user._id,
        tenant_id: resolvedTenantId,
        terminal_id: "00000000-0000-0000-0000-000000000000", // Default terminal for web logins
        role: user.role,
        email: user.email,
      },
      envConfig.jwt.secret,
      { expiresIn: envConfig.jwt.expiresIn as jwt.SignOptions["expiresIn"] },
    );

    return {
      accessToken: token,
      user: {
        id: user._id,
        email: user.email,
        role: user.role,
        tenantId: resolvedTenantId,
      },
    };
  }
}
