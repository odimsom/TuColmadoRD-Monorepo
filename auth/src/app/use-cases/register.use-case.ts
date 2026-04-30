import { randomUUID } from "crypto";
import bcrypt from "bcryptjs";
import jwt from "jsonwebtoken";
import { RegisterDto } from "../dtos/register.dto";
import { IAuthResponse } from "../../domain/interfaces/auth.interface";
import { UserRepository } from "../../infra/repositories/user.repository";
import { TenantRepository } from "../../infra/repositories/tenant.repository";
import { NetApiService } from "../services/net-api.service";
import { envConfig } from "../../config/env.config";
import { Role } from "../../domain/enums/role.enums";

export class RegisterUseCase {
  constructor(
    private readonly userRepo: UserRepository,
    private readonly tenantRepo: TenantRepository,
    private readonly netApi: NetApiService,
  ) {}

  async execute(dto: RegisterDto): Promise<IAuthResponse> {
    const tenantId = randomUUID();
    const normalizedEmail = dto.email.trim().toLowerCase();

    await this.tenantRepo.create({
      _id: tenantId,
      name: dto.tenantName,
      isActive: true,
      subscriptionStatus: 'trialing',
    });

    let user;
    try {
      const hashedPassword = await bcrypt.hash(dto.password, 10);
      user = await this.userRepo.create({
        tenantId,
        email: normalizedEmail,
        password: hashedPassword,
        role: Role.OWNER,
        isActive: true,
      });
    } catch (error) {
      await this.tenantRepo.delete(tenantId);
      throw error;
    }

    try {
      await this.netApi.notifyNewTenant({
        tenantId,
        name: dto.tenantName,
        ownerEmail: normalizedEmail,
      });
    } catch (error) {
      console.error("❌ Failed to notify API of new tenant:", error);
      // Log but don't fail registration - tenant is already created in Auth
      // The API can be notified later through a retry mechanism
    }

    const token = jwt.sign(
      {
        sub: user._id,
        tenant_id: tenantId,
        terminal_id: "00000000-0000-0000-0000-000000000000",
        role: Role.OWNER,
        email: normalizedEmail,
        subscription_status: 'trialing',
      },
      envConfig.jwt.secret,
      { expiresIn: envConfig.jwt.expiresIn as jwt.SignOptions["expiresIn"] },
    );

    return {
      accessToken: token,
      user: {
        id:                 user._id,
        email:              user.email,
        firstName:          user.firstName ?? null,
        lastName:           user.lastName  ?? null,
        role:               Role.OWNER,
        tenantId,
        subscriptionStatus: 'trialing',
      },
    };
  }
}
