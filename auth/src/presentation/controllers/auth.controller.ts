import { Request, Response, NextFunction } from "express";
import { UserRepository } from "../../infra/repositories/user.repository";
import { TenantRepository } from "../../infra/repositories/tenant.repository";
import { NetApiService } from "../../app/services/net-api.service";
import { RegisterUseCase } from "../../app/use-cases/register.use-case";
import { LoginUseCase } from "../../app/use-cases/login.use-case";
import { PairDeviceUseCase } from "../../app/use-cases/pair-device.use-case";
import { RenewLicenseUseCase } from "../../app/use-cases/renew-license.use-case";
import {
  ListEmployeesUseCase,
  CreateEmployeeUseCase,
  UpdateEmployeeUseCase,
  ToggleEmployeeUseCase,
} from "../../app/use-cases/employee.use-case";
import { AuthenticatedRequest } from "../middlewares/auth.middleware";

const userRepo = new UserRepository();
const tenantRepo = new TenantRepository();
const netApi = new NetApiService();

const registerUseCase   = new RegisterUseCase(userRepo, tenantRepo, netApi);
const loginUseCase      = new LoginUseCase(userRepo, tenantRepo);
const pairDeviceUseCase = new PairDeviceUseCase(userRepo, tenantRepo);
const renewLicenseUseCase = new RenewLicenseUseCase(tenantRepo);
const listEmployeesUseCase   = new ListEmployeesUseCase(userRepo);
const createEmployeeUseCase  = new CreateEmployeeUseCase(userRepo);
const updateEmployeeUseCase  = new UpdateEmployeeUseCase(userRepo);
const toggleEmployeeUseCase  = new ToggleEmployeeUseCase(userRepo);

export class AuthController {
  async register(
    req: Request,
    res: Response,
    next: NextFunction,
  ): Promise<void> {
    try {
      const result = await registerUseCase.execute(req.body);
      res.status(201).json(result);
    } catch (error) {
      next(error);
    }
  }

  async login(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const result = await loginUseCase.execute(req.body);
      res.status(200).json(result);
    } catch (error) {
      next(error);
    }
  }

  async pairDevice(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const result = await pairDeviceUseCase.execute(req.body);
      res.status(200).json(result);
    } catch (error: any) {
      if (error.message === "INVALID_CREDENTIALS") {
        res.status(401).json({ error: error.message });
      } else if (error.message === "TENANT_NOT_FOUND") {
        res.status(404).json({ error: error.message });
      } else {
        next(error);
      }
    }
  }

  async renewLicense(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) { res.status(401).json({ error: "UNAUTHORIZED" }); return; }
      const { tenant_id, terminal_id } = req.user;
      const result = await renewLicenseUseCase.execute(tenant_id, terminal_id);
      res.status(200).json(result);
    } catch (error: any) {
      if (error.message === "TENANT_NOT_FOUND" || error.message === "RENEWAL_REJECTED") {
        res.status(403).json({ error: error.message });
      } else {
        console.error("Renew license error:", error);
        res.status(500).json({ error: "INTERNAL_SERVER_ERROR" });
      }
    }
  }

  async listEmployees(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) { res.status(401).json({ error: "UNAUTHORIZED" }); return; }
      const employees = await listEmployeesUseCase.execute(req.user.tenant_id);
      res.status(200).json(employees);
    } catch (error) { next(error); }
  }

  async createEmployee(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) { res.status(401).json({ error: "UNAUTHORIZED" }); return; }
      const employee = await createEmployeeUseCase.execute(req.user.tenant_id, req.body);
      res.status(201).json(employee);
    } catch (error: any) {
      if (error.message === "INVALID_ROLE") res.status(400).json({ error: "INVALID_ROLE" });
      else if (error.message === "EMAIL_ALREADY_EXISTS") res.status(409).json({ error: "EMAIL_ALREADY_EXISTS" });
      else next(error);
    }
  }

  async updateEmployee(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) { res.status(401).json({ error: "UNAUTHORIZED" }); return; }
      const employee = await updateEmployeeUseCase.execute(req.params['id'] as string, req.user.tenant_id, req.body);
      res.status(200).json(employee);
    } catch (error: any) {
      if (error.message === "EMPLOYEE_NOT_FOUND") res.status(404).json({ error: "EMPLOYEE_NOT_FOUND" });
      else if (error.message === "INVALID_ROLE") res.status(400).json({ error: "INVALID_ROLE" });
      else next(error);
    }
  }

  async toggleEmployee(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) { res.status(401).json({ error: "UNAUTHORIZED" }); return; }
      const { active } = req.body;
      await toggleEmployeeUseCase.execute(req.params['id'] as string, req.user.tenant_id, !!active);
      res.status(204).send();
    } catch (error) { next(error); }
  }
}
