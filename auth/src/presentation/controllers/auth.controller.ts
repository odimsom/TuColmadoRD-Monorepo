import { Request, Response, NextFunction } from "express";
import { UserRepository } from "../../infra/repositories/user.repository";
import { TenantRepository } from "../../infra/repositories/tenant.repository";
import { NetApiService } from "../../app/services/net-api.service";
import { EmailServiceClient } from "../../app/services/email-service.client";
import { RegisterUseCase } from "../../app/use-cases/register.use-case";
import { LoginUseCase } from "../../app/use-cases/login.use-case";
import { VerifyEmailUseCase } from "../../app/use-cases/verify-email.use-case";
import { ResendVerificationUseCase } from "../../app/use-cases/resend-verification.use-case";
import { PairDeviceUseCase } from "../../app/use-cases/pair-device.use-case";
import { RenewLicenseUseCase } from "../../app/use-cases/renew-license.use-case";
import {
  ListEmployeesUseCase,
  CreateEmployeeUseCase,
  UpdateEmployeeUseCase,
  ToggleEmployeeUseCase,
} from "../../app/use-cases/employee.use-case";
import { AuthenticatedRequest } from "../middlewares/auth.middleware";
import {
  AuthDomainError,
  InvalidCredentialsError,
  TenantNotFoundError,
  EmailNotVerifiedError,
  AccountSuspendedError,
  EmailAlreadyExistsError,
  InvalidRoleError,
  EmployeeNotFoundError,
  UserNotFoundError,
  VerificationCodeExpiredError,
  InvalidVerificationCodeError,
  NoPendingVerificationError,
  InvalidStateTransitionError,
} from "../../domain/errors/auth-domain-error";

function domainErrorStatus(error: AuthDomainError): number {
  if (error instanceof InvalidCredentialsError)     return 401;
  if (error instanceof AccountSuspendedError)       return 403;
  if (error instanceof EmailNotVerifiedError)       return 403;
  if (error instanceof TenantNotFoundError)         return 404;
  if (error instanceof UserNotFoundError)           return 404;
  if (error instanceof EmployeeNotFoundError)       return 404;
  if (error instanceof EmailAlreadyExistsError)     return 409;
  if (error instanceof VerificationCodeExpiredError) return 410;
  if (error instanceof InvalidVerificationCodeError) return 401;
  if (error instanceof InvalidRoleError)            return 400;
  if (error instanceof NoPendingVerificationError)  return 400;
  if (error instanceof InvalidStateTransitionError) return 422;
  return 500;
}

const userRepo    = new UserRepository();
const tenantRepo  = new TenantRepository();
const netApi      = new NetApiService();
const emailClient = new EmailServiceClient();

const registerUseCase           = new RegisterUseCase(userRepo, tenantRepo, netApi, emailClient);
const loginUseCase              = new LoginUseCase(userRepo, tenantRepo);
const verifyEmailUseCase        = new VerifyEmailUseCase(userRepo, tenantRepo, emailClient);
const resendVerificationUseCase = new ResendVerificationUseCase(userRepo, tenantRepo, emailClient);
const pairDeviceUseCase         = new PairDeviceUseCase(userRepo, tenantRepo);
const renewLicenseUseCase       = new RenewLicenseUseCase(tenantRepo);
const listEmployeesUseCase      = new ListEmployeesUseCase(userRepo);
const createEmployeeUseCase     = new CreateEmployeeUseCase(userRepo);
const updateEmployeeUseCase     = new UpdateEmployeeUseCase(userRepo);
const toggleEmployeeUseCase     = new ToggleEmployeeUseCase(userRepo);

export class AuthController {
  async register(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const result = await registerUseCase.execute(req.body);
      if (!result.isOk) {
        res.status(domainErrorStatus(result.error)).json(result.error.toJSON());
        return;
      }
      res.status(201).json(result.value);
    } catch (error) { next(error); }
  }

  async login(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const result = await loginUseCase.execute(req.body);
      if (!result.isOk) {
        res.status(domainErrorStatus(result.error)).json(result.error.toJSON());
        return;
      }
      res.status(200).json(result.value);
    } catch (error) { next(error); }
  }

  async verifyEmail(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { email, code } = req.body as { email: string; code: string };
      if (!email || !code) {
        res.status(400).json({ code: 'INVALID_REQUEST', message: 'email y code son requeridos.' });
        return;
      }
      const result = await verifyEmailUseCase.execute(email, code);
      if (!result.isOk) {
        res.status(domainErrorStatus(result.error)).json(result.error.toJSON());
        return;
      }
      res.status(200).json(result.value);
    } catch (error) { next(error); }
  }

  async resendVerification(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { email } = req.body as { email: string };
      if (!email) {
        res.status(400).json({ code: 'INVALID_REQUEST', message: 'email es requerido.' });
        return;
      }
      await resendVerificationUseCase.execute(email);
      res.status(200).json({ message: "Si el correo existe y no está verificado, recibirás un nuevo código." });
    } catch (error) { next(error); }
  }

  async pairDevice(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const result = await pairDeviceUseCase.execute(req.body);
      res.status(200).json(result);
    } catch (error: any) {
      if (error.message === "INVALID_CREDENTIALS") {
        res.status(401).json({ code: "INVALID_CREDENTIALS", message: "Credenciales inválidas." });
      } else if (error.message === "TENANT_NOT_FOUND") {
        res.status(404).json({ code: "TENANT_NOT_FOUND", message: "Empresa no encontrada." });
      } else {
        next(error);
      }
    }
  }

  async renewLicense(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) { res.status(401).json({ code: "UNAUTHORIZED", message: "No autenticado." }); return; }
      const { tenant_id, terminal_id } = req.user;
      const result = await renewLicenseUseCase.execute(tenant_id, terminal_id);
      res.status(200).json(result);
    } catch (error: any) {
      if (error.message === "TENANT_NOT_FOUND") {
        res.status(404).json({ code: "TENANT_NOT_FOUND", message: "Empresa no encontrada." });
      } else if (error.message === "RENEWAL_REJECTED") {
        res.status(403).json({ code: "RENEWAL_REJECTED", message: "Renovación rechazada." });
      } else {
        next(error);
      }
    }
  }

  async listEmployees(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) { res.status(401).json({ code: "UNAUTHORIZED" }); return; }
      const employees = await listEmployeesUseCase.execute(req.user.tenant_id);
      res.status(200).json(employees);
    } catch (error) { next(error); }
  }

  async createEmployee(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) { res.status(401).json({ code: "UNAUTHORIZED" }); return; }
      const result = await createEmployeeUseCase.execute(req.user.tenant_id, req.body);
      if (!result.isOk) {
        res.status(domainErrorStatus(result.error)).json(result.error.toJSON());
        return;
      }
      res.status(201).json(result.value);
    } catch (error) { next(error); }
  }

  async updateEmployee(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) { res.status(401).json({ code: "UNAUTHORIZED" }); return; }
      const result = await updateEmployeeUseCase.execute(req.params['id'] as string, req.user.tenant_id, req.body);
      if (!result.isOk) {
        res.status(domainErrorStatus(result.error)).json(result.error.toJSON());
        return;
      }
      res.status(200).json(result.value);
    } catch (error) { next(error); }
  }

  async toggleEmployee(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) { res.status(401).json({ code: "UNAUTHORIZED" }); return; }
      const { active } = req.body;
      const result = await toggleEmployeeUseCase.execute(req.params['id'] as string, req.user.tenant_id, !!active);
      if (!result.isOk) {
        res.status(domainErrorStatus(result.error)).json(result.error.toJSON());
        return;
      }
      res.status(204).send();
    } catch (error) { next(error); }
  }
}
