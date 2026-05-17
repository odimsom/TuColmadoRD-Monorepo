const DEFAULT_BASE_URL = "http://localhost:5000";
const DEFAULT_ENV = {
  apiurl: DEFAULT_BASE_URL,
  nodeEnv: "production",
} as const;

jest.mock("../src/config/env.config", () => ({
  envConfig: {
    apiurl: "http://localhost:5000",
    nodeEnv: "production",
  },
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
    const service = new NetApiService();
    (global.fetch as jest.Mock).mockResolvedValue({ ok: false, status: 400 });

    await expect(service.notifyNewTenant(payload)).rejects.toThrow("NET_API_ERROR");
    await expect(service.notifyNewTenant(payload)).rejects.toThrow("NET_API_ERROR");
    expect(global.fetch).toHaveBeenCalledTimes(2);
  });

  it("lanza NET_API_ERROR cuando falla la llamada de red", async () => {
    Object.assign(envConfig as any, { nodeEnv: "production", apiurl: "http://api.local" });
    const networkError = new Error("network down");
    (global.fetch as jest.Mock).mockRejectedValue(networkError);

    await expect(new NetApiService().notifyNewTenant(payload)).rejects.toMatchObject({
      message: "NET_API_ERROR",
      cause: networkError,
    });
  });

  it("limpia el timeout aunque falle la llamada", async () => {
    Object.assign(envConfig as any, { nodeEnv: "production", apiurl: "http://api.local" });
    const clearTimeoutSpy = jest.spyOn(global, "clearTimeout");
    (global.fetch as jest.Mock).mockRejectedValue(new Error("network down"));

    await expect(new NetApiService().notifyNewTenant(payload)).rejects.toThrow("NET_API_ERROR");
    expect(clearTimeoutSpy).toHaveBeenCalled();
    clearTimeoutSpy.mockRestore();
  });

  it("activa degradación temporal cuando el server no responde y reintenta luego", async () => {
    Object.assign(envConfig as any, { nodeEnv: "production", apiurl: "http://api.local" });
    const nowSpy = jest.spyOn(Date, "now");
    const service = new NetApiService();
    const timeoutError = Object.assign(new TypeError("network timeout"), { name: "AbortError" });
    (global.fetch as jest.Mock)
      .mockRejectedValueOnce(timeoutError)
      .mockResolvedValueOnce({ ok: true });

    nowSpy.mockReturnValue(1_000);
    await expect(service.notifyNewTenant(payload)).rejects.toMatchObject({
      message: "NET_API_ERROR",
      cause: timeoutError,
    });
    expect(global.fetch).toHaveBeenCalledTimes(1);

    nowSpy.mockReturnValue(15_000);
    await expect(service.notifyNewTenant(payload)).rejects.toMatchObject({
      message: "NET_API_ERROR",
      cause: expect.objectContaining({ message: "NET_API_TEMPORARILY_UNAVAILABLE" }),
    });
    expect(global.fetch).toHaveBeenCalledTimes(1);

    nowSpy.mockReturnValue(32_000);
    await expect(service.notifyNewTenant(payload)).resolves.toBeUndefined();
    expect(global.fetch).toHaveBeenCalledTimes(2);
    nowSpy.mockRestore();
  });

  it("activa degradación temporal con errores 5xx y bloquea reintentos durante cooldown", async () => {
    Object.assign(envConfig as any, { nodeEnv: "production", apiurl: "http://api.local" });
    const nowSpy = jest.spyOn(Date, "now");
    const service = new NetApiService();
    (global.fetch as jest.Mock).mockResolvedValue({ ok: false, status: 503 });

    nowSpy.mockReturnValue(10_000);
    await expect(service.notifyNewTenant(payload)).rejects.toThrow("NET_API_ERROR");
    expect(global.fetch).toHaveBeenCalledTimes(1);

    nowSpy.mockReturnValue(20_000);
    await expect(service.notifyNewTenant(payload)).rejects.toMatchObject({
      message: "NET_API_ERROR",
      cause: expect.objectContaining({ message: "NET_API_TEMPORARILY_UNAVAILABLE" }),
    });
    expect(global.fetch).toHaveBeenCalledTimes(1);
    nowSpy.mockRestore();
  });
});
