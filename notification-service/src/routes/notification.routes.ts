import { Router, Request, Response, NextFunction } from "express";
import { NotificationController } from "../controllers/notification.controller";
import { env } from "../config/env.config";

const router = Router();
const controller = new NotificationController();

function requireServiceSecret(req: Request, res: Response, next: NextFunction): void {
  if (req.headers["x-service-secret"] !== env.serviceSecret) {
    res.status(401).json({ code: "UNAUTHORIZED", message: "Acceso denegado." });
    return;
  }
  next();
}

router.post("/send", requireServiceSecret, (req, res, next) => controller.send(req, res, next));

export default router;
