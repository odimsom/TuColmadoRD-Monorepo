import bcrypt from "bcryptjs";
import { UserRepository } from "../../infra/repositories/user.repository";
import { IUser } from "../../domain/interfaces/user.interface";
import { Role } from "../../domain/enums/role.enums";

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
  ): Promise<Omit<IUser, "password">> {
    const role = data.role as Role;
    if (!MANAGEABLE_ROLES.includes(role)) {
      throw new Error("INVALID_ROLE");
    }

    const normalizedEmail = data.email.trim().toLowerCase();
    const exists = await this.userRepo.existsByEmailAndTenant(normalizedEmail, tenantId);
    if (exists) throw new Error("EMAIL_ALREADY_EXISTS");

    const hashedPassword = await bcrypt.hash(data.password, 10);
    const user = await this.userRepo.create({
      tenantId,
      email: normalizedEmail,
      password: hashedPassword,
      firstName: data.firstName ?? null,
      lastName: data.lastName ?? null,
      role,
      isActive: true,
    });

    const { password: _pw, ...rest } = user;
    return rest;
  }
}

export class UpdateEmployeeUseCase {
  constructor(private readonly userRepo: UserRepository) {}

  async execute(
    id: string,
    tenantId: string,
    data: { firstName?: string; lastName?: string; role?: string },
  ): Promise<Omit<IUser, "password">> {
    if (data.role && !MANAGEABLE_ROLES.includes(data.role as Role)) {
      throw new Error("INVALID_ROLE");
    }

    const updated = await this.userRepo.updateById(id, tenantId, {
      ...(data.firstName !== undefined && { firstName: data.firstName }),
      ...(data.lastName  !== undefined && { lastName:  data.lastName }),
      ...(data.role      !== undefined && { role:      data.role as Role }),
    });
    if (!updated) throw new Error("EMPLOYEE_NOT_FOUND");

    const { password: _pw, ...rest } = updated;
    return rest;
  }
}

export class ToggleEmployeeUseCase {
  constructor(private readonly userRepo: UserRepository) {}

  async execute(id: string, tenantId: string, active: boolean): Promise<void> {
    if (active) {
      await this.userRepo.activate(id, tenantId);
    } else {
      await this.userRepo.deactivate(id, tenantId);
    }
  }
}
