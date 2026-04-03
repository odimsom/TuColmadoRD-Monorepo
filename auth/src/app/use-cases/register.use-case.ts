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
      await this.userRepo.delete(user._id, tenantId);
      await this.tenantRepo.delete(tenantId);
      throw new Error("NET_API_ERROR");
    }

    const token = jwt.sign(
      { sub: user._id, tenantId, role: Role.OWNER, email: normalizedEmail },
      envConfig.jwt.secret,
      { expiresIn: envConfig.jwt.expiresIn as jwt.SignOptions["expiresIn"] },
    );

    return {
      accessToken: token,
      user: { id: user._id, email: user.email, role: Role.OWNER, tenantId },
    };
  }
}
