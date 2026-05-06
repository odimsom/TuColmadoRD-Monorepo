import { Role } from "../enums/role.enums";
import { SubscriptionStatus } from "./tenant.interface";

export interface IJwtPayload {
  sub: string;
  tenantId: string;
  role: Role;
  email: string;
  subscription_status: SubscriptionStatus;
}

export interface IAuthResponse {
  accessToken: string;
  user: {
    id:                 string;
    email:              string;
    firstName:          string | null;
    lastName:           string | null;
    role:               Role;
    tenantId:           string;
    subscriptionStatus: SubscriptionStatus;
  };
}
