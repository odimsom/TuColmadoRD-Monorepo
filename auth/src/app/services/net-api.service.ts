import { envConfig } from "../../config/env.config";

export class NetApiService {
  private readonly baseUrl = (envConfig.apiurl || "http://localhost:5000").replace(/\/+$/, "");

  private readonly isMock = envConfig.nodeEnv === "development";
  private readonly requestTimeoutMs = 5000;

  async notifyNewTenant(data: {
    tenantId: string;
    name: string;
    ownerEmail: string;
  }): Promise<void> {
    if (this.isMock) {
      console.log("🟡 [MOCK] Notificación al .NET:", data);
      return;
    }

    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), this.requestTimeoutMs);

    try {
      const response = await fetch(`${this.baseUrl}/api/tenants`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
        signal: controller.signal,
      });

      if (!response.ok) {
        throw new Error("NET_API_ERROR");
      }
    } catch (error) {
      if (error instanceof Error && error.message === "NET_API_ERROR") {
        throw error;
      }
      throw Object.assign(new Error("NET_API_ERROR"), { cause: error });
    } finally {
      clearTimeout(timeout);
    }
  }
}
