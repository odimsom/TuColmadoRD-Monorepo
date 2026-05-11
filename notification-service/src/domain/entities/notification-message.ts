import { NotificationStatus } from "../enums/notification-status.enum";
import { NotificationChannel } from "../enums/notification-channel.enum";
import { NotificationStateMachine } from "../state-machine/notification-state-machine";
import { OperationResult } from "../result/operation-result";
import { NotificationDomainError } from "../errors/notification-error";

export interface NotificationMessageProps {
  channel: NotificationChannel;
  to: string;
  subject?: string;
  body: string;
  templateId?: string;
}

export class NotificationMessage {
  private _status: NotificationStatus;
  private _sentAt?: Date;
  private _failureReason?: string;

  private constructor(private readonly props: NotificationMessageProps) {
    this._status = NotificationStatus.PENDING;
  }

  static create(props: NotificationMessageProps): NotificationMessage {
    return new NotificationMessage(props);
  }

  get channel(): NotificationChannel { return this.props.channel; }
  get to(): string { return this.props.to; }
  get subject(): string | undefined { return this.props.subject; }
  get body(): string { return this.props.body; }
  get templateId(): string | undefined { return this.props.templateId; }
  get status(): NotificationStatus { return this._status; }
  get sentAt(): Date | undefined { return this._sentAt; }
  get failureReason(): string | undefined { return this._failureReason; }

  beginSend(): OperationResult<void, NotificationDomainError> {
    const result = NotificationStateMachine.transition(this._status, 'BEGIN_SEND');
    if (!result.isOk) return OperationResult.fail(result.error);
    this._status = result.value;
    return OperationResult.ok(undefined);
  }

  markSent(): OperationResult<void, NotificationDomainError> {
    const result = NotificationStateMachine.transition(this._status, 'SUCCEED');
    if (!result.isOk) return OperationResult.fail(result.error);
    this._status = result.value;
    this._sentAt = new Date();
    return OperationResult.ok(undefined);
  }

  markFailed(reason: string): OperationResult<void, NotificationDomainError> {
    const result = NotificationStateMachine.transition(this._status, 'FAIL');
    if (!result.isOk) return OperationResult.fail(result.error);
    this._status = result.value;
    this._failureReason = reason;
    return OperationResult.ok(undefined);
  }

  scheduleRetry(): OperationResult<void, NotificationDomainError> {
    const result = NotificationStateMachine.transition(this._status, 'RETRY');
    if (!result.isOk) return OperationResult.fail(result.error);
    this._status = result.value;
    return OperationResult.ok(undefined);
  }
}
