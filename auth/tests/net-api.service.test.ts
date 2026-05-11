const DEFAULT_BASE_URL = "http://localhost:5000";
const DEFAULT_ENV = {
  apiurl: DEFAULT_BASE_URL,
  nodeEnv: "production",
} as const;

jest.mock("../src/config/env.config", () => ({
  envConfig: { ...DEFAULT_ENV },
}));

import { envConfig } from "../src/config/env.config";
import { NetApiService } from "../src/app/services/net-api.service";

describe("NetApiService", () => {
  const payload = {
    tenantId: "tenant-123",
    name: "Colmado Central",
    ownerEmail: "owner@colmado.com",
  };

  beforeEach(() => {
    jest.clearAllMocks();
    (global as any).fetch = jest.fn();
  });

  afterEach(() => {
    Object.assign(envConfig as any, DEFAULT_ENV);
  });

  it("no hace llamada HTTP en modo development (mock)", async () => {
    Object.assign(envConfig as any, { nodeEnv: "development", apiurl: "http://api.local/" });
    const logSpy = jest.spyOn(console, "log").mockImplementation(() => {});

    await expect(new NetApiService().notifyNewTenant(payload)).resolves.toBeUndefined();

    expect(global.fetch).not.toHaveBeenCalled();
    logSpy.mockRestore();
  });

  it("normaliza la URL base y envía la notificación al endpoint esperado", async () => {
    Object.assign(envConfig as any, { nodeEnv: "production", apiurl: "http://api.local/" });
    (global.fetch as jest.Mock).mockResolvedValue({ ok: true });

    await new NetApiService().notifyNewTenant(payload);

    expect(global.fetch).toHaveBeenCalledWith(
      "http://api.local/api/tenants",
      expect.objectContaining({
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
        signal: expect.any(AbortSignal),
      }),
    );
  });

  it("lanza NET_API_ERROR cuando .NET responde con estado no exitoso", async () => {
    Object.assign(envConfig as any, { nodeEnv: "production", apiurl: "http://api.local" });
    (global.fetch as jest.Mock).mockResolvedValue({ ok: false });

    await expect(new NetApiService().notifyNewTenant(payload)).rejects.toThrow("NET_API_ERROR");
  });

  it("lanza NET_API_ERROR cuando falla la llamada de red", async () => {
    Object.assign(envConfig as any, { nodeEnv: "production", apiurl: "http://api.local" });
    (global.fetch as jest.Mock).mockRejectedValue(new Error("network down"));

    await expect(new NetApiService().notifyNewTenant(payload)).rejects.toThrow("NET_API_ERROR");
  });

  it("limpia el timeout aunque falle la llamada", async () => {
    Object.assign(envConfig as any, { nodeEnv: "production", apiurl: "http://api.local" });
    const clearTimeoutSpy = jest.spyOn(global, "clearTimeout");
    (global.fetch as jest.Mock).mockRejectedValue(new Error("network down"));

    await expect(new NetApiService().notifyNewTenant(payload)).rejects.toThrow("NET_API_ERROR");
    expect(clearTimeoutSpy).toHaveBeenCalled();
    clearTimeoutSpy.mockRestore();
  });
});
