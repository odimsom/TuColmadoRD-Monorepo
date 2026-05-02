import { Router } from "express";
import { AuthController } from "../controllers/auth.controller";
import { authMiddleware } from "../middlewares/auth.middleware";
import { AuthenticatedRequest } from "../middlewares/auth.middleware";

const router = Router();
const controller = new AuthController();

router.post("/register",    (req, res, next) => controller.register(req, res, next));
router.post("/login",       (req, res, next) => controller.login(req, res, next));
router.post("/pair-device", (req, res, next) => controller.pairDevice(req, res, next));
router.post("/renew-license", authMiddleware, (req, res, next) => controller.renewLicense(req as AuthenticatedRequest, res, next));

// Employee management (Owner/Admin only)
router.get(   "/employees",     authMiddleware, (req, res, next) => controller.listEmployees(req as AuthenticatedRequest, res, next));
router.post(  "/employees",     authMiddleware, (req, res, next) => controller.createEmployee(req as AuthenticatedRequest, res, next));
router.put(   "/employees/:id", authMiddleware, (req, res, next) => controller.updateEmployee(req as AuthenticatedRequest, res, next));
router.patch( "/employees/:id", authMiddleware, (req, res, next) => controller.toggleEmployee(req as AuthenticatedRequest, res, next));

export default router;
