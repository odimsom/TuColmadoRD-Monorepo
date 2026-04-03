import jwt from "jsonwebtoken";
import fs from "fs";
import path from "path";
import { TenantRepository } from "../../infra/repositories/tenant.repository";

export class RenewLicenseUseCase {
  private readonly privateKey: string;

  constructor(private readonly tenantRepo: TenantRepository) {
    try {
      this.privateKey = fs.readFileSync(
        path.join(__dirname, "../../../keys/private.pem"),
        "utf8",
      );
    } catch (err) {
      console.warn("WARNING: private.pem not found in keys/. RS256 token generation will fail if invoked.");
      this.privateKey = "";
    }
  }

  async execute(tenantId: string, terminalId: string, daysValid: number = 30) {
    const tenant = await this.tenantRepo.findById(tenantId);
    if (!tenant) {
      throw new Error("TENANT_NOT_FOUND");
    }

    if (!tenant.isActive) {
      throw new Error("RENEWAL_REJECTED");
    }

    const issuedAt = Math.floor(Date.now() / 1000);
    const validUntil = new Date();
    validUntil.setDate(validUntil.getDate() + daysValid);
    const validUntilUnix = Math.floor(validUntil.getTime() / 1000);

    const licenseToken = jwt.sign(
      {
        tenant_id: tenantId,
        terminal_id: terminalId,
        valid_until: validUntilUnix,
      },
      this.privateKey,
      { algorithm: "RS256" },
    );

    return {
      licenseToken,
      validUntil: validUntil.toISOString(),
    };
  }
}
