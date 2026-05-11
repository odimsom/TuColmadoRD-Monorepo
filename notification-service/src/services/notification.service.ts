import { NotificationChannel } from "../domain/enums/notification-channel.enum";
import { OperationResult } from "../domain/result/operation-result";
import { NotificationDomainError } from "../domain/errors/notification-error";
import { resolveChannel } from "../channels/channel-registry";
import { renderTemplate } from "../templates/registry";
import { notificationQueue } from "../queue/notification.queue";

export interface SendNotificationDto {
  channel: string;
  to: string;
  templateId: string;
  templateData?: Record<string, string>;
}

export class NotificationService {
  async send(dto: SendNotificationDto): Promise<OperationResult<void, NotificationDomainError>> {
    const channelResult = resolveChannel(dto.channel);
    if (!channelResult.isOk) return OperationResult.fail(channelResult.error);

    const channelEnum = dto.channel as NotificationChannel;
    const rendered = renderTemplate(channelEnum, dto.templateId, dto.templateData ?? {});
    if (!rendered.isOk) return OperationResult.fail(rendered.error);

    await notificationQueue.add("send", dto);
    return OperationResult.ok(undefined);
  }
}
