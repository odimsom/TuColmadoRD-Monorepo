import { OperationResult } from "../result/operation-result";
import { NotificationDomainError } from "../errors/notification-error";

export interface NotificationPayload {
  to: string;
  subject?: string;
  templateId?: string;
  templateData?: Record<string, string>;
  body?: string;
}

export interface INotificationChannel {
  readonly channelId: string;
  send(payload: NotificationPayload): Promise<OperationResult<void, NotificationDomainError>>;
}
