import { Role } from "../enums/role.enums";
import { UserStatus } from "../enums/user-status.enum";

export interface IUser {
  _id: string;
  tenantId: string;
  email: string;
  password: string;
  firstName?: string | null;
  lastName?: string | null;
  role: Role;
  status: UserStatus;
  verificationCode?: string | null;
  verificationCodeExpiry?: Date | null;
  createdAt: Date;
}
