import { INotificationChannel } from "../domain/interfaces/notification-channel.interface";
import { NotificationChannel } from "../domain/enums/notification-channel.enum";
import { OperationResult } from "../domain/result/operation-result";
import { NotificationDomainError, UnsupportedChannelError } from "../domain/errors/notification-error";
import { EmailChannel } from "./email.channel";
import { SmsChannel } from "./sms.channel";

const registry = new Map<NotificationChannel, INotificationChannel>([
  [NotificationChannel.EMAIL, new EmailChannel()],
  [NotificationChannel.SMS,   new SmsChannel()],
]);

export function resolveChannel(
  channelId: string,
): OperationResult<INotificationChannel, NotificationDomainError> {
  const channel = registry.get(channelId as NotificationChannel);
  if (!channel) {
    return OperationResult.fail(new UnsupportedChannelError());
  }
  return OperationResult.ok(channel);
}
