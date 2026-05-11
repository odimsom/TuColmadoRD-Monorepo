import bcrypt from "bcryptjs";
import { UserRepository } from "../../infra/repositories/user.repository";
import { IUser } from "../../domain/interfaces/user.interface";
import { Role } from "../../domain/enums/role.enums";
import { UserStatus } from "../../domain/enums/user-status.enum";
import { UserStateMachine } from "../../domain/state-machine/user-state-machine";
import { OperationResult } from "../../domain/result/operation-result";
import {
  AuthDomainError,
  EmailAlreadyExistsError,
  InvalidRoleError,
  EmployeeNotFoundError,
} from "../../domain/errors/auth-domain-error";

const MANAGEABLE_ROLES: Role[] = [Role.ADMIN, Role.CASHIER, Role.SELLER, Role.DELIVERY];

export class ListEmployeesUseCase {
  constructor(private readonly userRepo: UserRepository) {}

  async execute(tenantId: string): Promise<Omit<IUser, "password">[]> {
    const users = await this.userRepo.findAllByTenant(tenantId);
    return users.map(({ password: _pw, ...rest }) => rest);
  }
}

export class CreateEmployeeUseCase {
  constructor(private readonly userRepo: UserRepository) {}

  async execute(
    tenantId: string,
    data: { email: string; password: string; firstName?: string; lastName?: string; role: string },
  ): Promise<OperationResult<Omit<IUser, "password">, AuthDomainError>> {
    const role = data.role as Role;
    if (!MANAGEABLE_ROLES.includes(role)) {
      return OperationResult.fail(new InvalidRoleError());
    }

    const normalizedEmail = data.email.trim().toLowerCase();
    if (await this.userRepo.existsByEmailAndTenant(normalizedEmail, tenantId)) {
      return OperationResult.fail(new EmailAlreadyExistsError());
    }

    const hashedPassword = await bcrypt.hash(data.password, 10);
    const user = await this.userRepo.create({
      tenantId,
      email: normalizedEmail,
      password: hashedPassword,
      firstName: data.firstName ?? null,
      lastName: data.lastName ?? null,
      role,
      status: UserStatus.ACTIVE,
      verificationCode: null,
      verificationCodeExpiry: null,
    });

    const { password: _pw, ...rest } = user;
    return OperationResult.ok(rest);
  }
}

export class UpdateEmployeeUseCase {
  constructor(private readonly userRepo: UserRepository) {}

  async execute(
    id: string,
    tenantId: string,
    data: { firstName?: string; lastName?: string; role?: string },
  ): Promise<OperationResult<Omit<IUser, "password">, AuthDomainError>> {
    if (data.role && !MANAGEABLE_ROLES.includes(data.role as Role)) {
      return OperationResult.fail(new InvalidRoleError());
    }

    const updated = await this.userRepo.updateById(id, tenantId, {
      ...(data.firstName !== undefined && { firstName: data.firstName }),
      ...(data.lastName  !== undefined && { lastName:  data.lastName }),
      ...(data.role      !== undefined && { role:      data.role as Role }),
    });
    if (!updated) {
      return OperationResult.fail(new EmployeeNotFoundError());
    }

    const { password: _pw, ...rest } = updated;
    return OperationResult.ok(rest);
  }
}

export class ToggleEmployeeUseCase {
  constructor(private readonly userRepo: UserRepository) {}

  async execute(
    id: string,
    tenantId: string,
    activate: boolean,
  ): Promise<OperationResult<void, AuthDomainError>> {
    const user = await this.userRepo.findById(id, tenantId);
    if (!user) {
      return OperationResult.fail(new EmployeeNotFoundError());
    }

    const trigger = activate ? 'REACTIVATE' : 'SUSPEND';
    const transitionResult = UserStateMachine.transition(user.status, trigger);
    if (!transitionResult.isOk) {
      return OperationResult.fail(transitionResult.error);
    }

    await this.userRepo.setStatusById(id, tenantId, transitionResult.value);
    return OperationResult.ok(undefined);
  }
}
