import { Router } from "express";
import { AuthController } from "../controllers/auth.controller";
import { authMiddleware } from "../middlewares/auth.middleware";

const router = Router();
const controller = new AuthController();

router.post("/register", (req, res, next) =>
  controller.register(req, res, next),
);
router.post("/login", (req, res, next) => controller.login(req, res, next));
router.post("/pair-device", (req, res, next) => controller.pairDevice(req, res, next));
router.post("/renew-license", authMiddleware, (req, res, next) => controller.renewLicense(req, res, next));

export default router;
