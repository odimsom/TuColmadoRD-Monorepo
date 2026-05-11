import { UserStatus } from "../enums/user-status.enum";
import { OperationResult } from "../result/operation-result";
import { AuthDomainError, InvalidStateTransitionError } from "../errors/auth-domain-error";

export type UserTransition = 'VERIFY_EMAIL' | 'SUSPEND' | 'REACTIVATE';

type TransitionMap = Partial<Record<UserTransition, UserStatus>>;

const ALLOWED: Record<UserStatus, TransitionMap> = {
  [UserStatus.PENDING_VERIFICATION]: { VERIFY_EMAIL: UserStatus.ACTIVE },
  [UserStatus.ACTIVE]:               { SUSPEND: UserStatus.SUSPENDED },
  [UserStatus.SUSPENDED]:            { REACTIVATE: UserStatus.ACTIVE },
};

export class UserStateMachine {
  static transition(
    current: UserStatus,
    trigger: UserTransition,
  ): OperationResult<UserStatus, AuthDomainError> {
    const next = ALLOWED[current]?.[trigger];
    if (!next) {
      return OperationResult.fail(
        new InvalidStateTransitionError(`'${trigger}' no permitido desde '${current}'.`),
      );
    }
    return OperationResult.ok(next);
  }

  static canLogin(status: UserStatus): boolean {
    return status === UserStatus.ACTIVE;
  }

  static isVerifiable(status: UserStatus): boolean {
    return status === UserStatus.PENDING_VERIFICATION;
  }
}
