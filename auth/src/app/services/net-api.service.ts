import { envConfig } from "../../config/env.config";

export class NetApiService {
  private static readonly DEFAULT_BASE_URL = "http://localhost:5000";
  private static readonly NET_API_ERROR = "NET_API_ERROR";
  private static readonly UNAVAILABLE_COOLDOWN_MS = 30_000;
  private static readonly UNAVAILABLE_CAUSE = "NET_API_TEMPORARILY_UNAVAILABLE";

  private static normalizeBaseUrl(url?: string): string {
    return (url || NetApiService.DEFAULT_BASE_URL).replace(/\/+$/, "");
  }

  private readonly baseUrl = NetApiService.normalizeBaseUrl(envConfig.apiurl);

  private readonly isMock = envConfig.nodeEnv === "development";
  private readonly requestTimeoutMs = 5000;
  private unavailableUntilMs = 0;

  private isTemporarilyUnavailable(): boolean {
    return this.unavailableUntilMs > Date.now();
  }

  private markTemporarilyUnavailable(): void {
    this.unavailableUntilMs = Date.now() + NetApiService.UNAVAILABLE_COOLDOWN_MS;
  }

  private markAvailable(): void {
    this.unavailableUntilMs = 0;
  }

  async notifyNewTenant(data: {
    tenantId: string;
    name: string;
    ownerEmail: string;
  }): Promise<void> {
    if (this.isMock) {
      console.log("🟡 [MOCK] Notificación al .NET:", data);
      return;
    }

    if (this.isTemporarilyUnavailable()) {
      throw Object.assign(new Error(NetApiService.NET_API_ERROR), {
        cause: new Error(NetApiService.UNAVAILABLE_CAUSE),
      });
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
        this.markTemporarilyUnavailable();
        throw new Error(NetApiService.NET_API_ERROR);
      }
      this.markAvailable();
    } catch (error) {
      if (error instanceof Error && error.message === NetApiService.NET_API_ERROR) {
        throw error;
      }
      this.markTemporarilyUnavailable();
      throw Object.assign(new Error(NetApiService.NET_API_ERROR), { cause: error });
    } finally {
      clearTimeout(timeout);
    }
  }
}
