import { Role } from "../enums/role.enums";

export interface IUser {
  _id: string;
  tenantId: string;
  email: string;
  password: string;
  role: Role;
  isActive: boolean;
  createdAt: Date;
}
