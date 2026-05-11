import { NotificationStatus } from "../enums/notification-status.enum";
import { OperationResult } from "../result/operation-result";
import { NotificationDomainError, InvalidStateTransitionError } from "../errors/notification-error";

type NotificationTrigger = 'BEGIN_SEND' | 'SUCCEED' | 'FAIL' | 'RETRY';

type TransitionMap = Partial<Record<NotificationTrigger, NotificationStatus>>;

const ALLOWED: Record<NotificationStatus, TransitionMap> = {
  [NotificationStatus.PENDING]:  { BEGIN_SEND: NotificationStatus.SENDING },
  [NotificationStatus.SENDING]:  { SUCCEED: NotificationStatus.SENT, FAIL: NotificationStatus.FAILED },
  [NotificationStatus.FAILED]:   { RETRY: NotificationStatus.RETRYING },
  [NotificationStatus.RETRYING]: { BEGIN_SEND: NotificationStatus.SENDING },
  [NotificationStatus.SENT]:     {},
};

export class NotificationStateMachine {
  static transition(
    current: NotificationStatus,
    trigger: NotificationTrigger,
  ): OperationResult<NotificationStatus, NotificationDomainError> {
    const next = ALLOWED[current]?.[trigger];
    if (!next) {
      return OperationResult.fail(
        new InvalidStateTransitionError(`'${trigger}' no permitido desde '${current}'.`),
      );
    }
    return OperationResult.ok(next);
  }
}
