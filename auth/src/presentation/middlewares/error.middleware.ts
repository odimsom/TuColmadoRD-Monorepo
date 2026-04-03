import { Request, Response, NextFunction } from "express";

const ERROR_MAP: Record<string, { status: number; message: string }> = {
  TENANT_NOT_FOUND: { status: 404, message: "Tenant no encontrado" },
  INVALID_CREDENTIALS: { status: 401, message: "Credenciales inválidas" },
  EMAIL_ALREADY_EXISTS: { status: 409, message: "El email ya está registrado" },
  NET_API_ERROR: {
    status: 502,
    message: "Error al sincronizar con el sistema principal",
  },
  "Error al sincronizar con el sistema principal": {
    status: 502,
    message: "Error al sincronizar con el sistema principal",
  },
};

export const errorMiddleware = (
  error: Error,
  _req: Request,
  res: Response,
  _next: NextFunction,
): void => {
  const mapped = ERROR_MAP[error.message];

  if (mapped) {
    res.status(mapped.status).json({ message: mapped.message });
    return;
  }

  console.error("Error no controlado:", error);
  res.status(500).json({ message: "Error interno del servidor" });
};
