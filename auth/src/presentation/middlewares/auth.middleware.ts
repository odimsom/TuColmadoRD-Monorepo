import { Request, Response, NextFunction } from "express";
import jwt from "jsonwebtoken";
import { envConfig } from "../../config/env.config";

export interface AuthenticatedRequest extends Request {
  user?: {
    sub: string;
    tenant_id: string;
    terminal_id: string;
    role: string;
    email: string;
  };
}

export const authMiddleware = (
  req: AuthenticatedRequest,
  res: Response,
  next: NextFunction,
) => {
  const authHeader = req.headers.authorization;
  if (!authHeader || !authHeader.startsWith("Bearer ")) {
    res.status(401).json({ error: "Missing or invalid Authorization header" });
    return;
  }

  const token = authHeader.split(" ")[1];

  try {
    const payload = jwt.verify(token, envConfig.jwt.secret) as any;
    req.user = payload;
    next();
  } catch (error) {
    res.status(401).json({ error: "Invalid or expired token" });
  }
};
