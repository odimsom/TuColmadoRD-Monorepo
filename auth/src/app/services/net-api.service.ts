import { envConfig } from "../../config/env.config";

// TODO: update real api
export class NetApiService {
  private readonly baseUrl = envConfig.apiurl || "http://localhost:5000";

  private readonly isMock = envConfig.nodeEnv === "development";

  async notifyNewTenant(data: {
    tenantId: string;
    name: string;
    ownerEmail: string;
  }): Promise<void> {
    if (this.isMock) {
      console.log("🟡 [MOCK] Notificación al .NET:", data);
      return;
    }

    const response = await fetch(`${this.baseUrl}/api/tenants`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error("NET_API_ERROR");
    }
  }
}
