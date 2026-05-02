import bcrypt from "bcryptjs";
import crypto from "crypto";
import { PairDeviceDto } from "../dtos/pair-device.dto";
import { UserRepository } from "../../infra/repositories/user.repository";
import { TenantRepository } from "../../infra/repositories/tenant.repository";

export class PairDeviceUseCase {
  constructor(
    private readonly userRepo: UserRepository,
    private readonly tenantRepo: TenantRepository,
  ) {}

  async execute(dto: PairDeviceDto) {
    // 1. Authenticate user to verify they belong to a tenant and have permissions
    const user = await this.userRepo.findByEmail(dto.email);
    if (!user) {
      throw new Error("INVALID_CREDENTIALS");
    }

    const isValid = await bcrypt.compare(dto.password, user.password);
    if (!isValid) {
      throw new Error("INVALID_CREDENTIALS");
    }

    const tenant = await this.tenantRepo.findById(user.tenantId);
    if (!tenant) {
      throw new Error("TENANT_NOT_FOUND");
    }

    // 2. Generate a terminal ID for this new device pairing
    const terminalId = crypto.randomUUID();

    // 3. Generate a public license key
    const publicLicenseKey = `LIC-${user.tenantId.substring(0, 8).toUpperCase()}-${terminalId.substring(0, 8).toUpperCase()}`;

    // Return the required pairing information
    return {
      tenantId: user.tenantId,
      terminalId,
      publicLicenseKey
    };
  }
}
