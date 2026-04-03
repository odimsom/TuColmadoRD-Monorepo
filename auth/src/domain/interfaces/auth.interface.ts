import { Role } from "../enums/role.enums";

export interface IJwtPayload {
  sub: string;
  tenantId: string;
  role: Role;
  email: string;
}

export interface IAuthResponse {
  accessToken: string;
  user: {
    id: string;
    email: string;
    role: Role;
    tenantId: string;
  };
}
