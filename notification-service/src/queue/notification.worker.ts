import { Worker } from "bullmq";
import { getRedisConnection } from "./redis.connection";
import type { SendNotificationDto } from "../services/notification.service";
import { resolveChannel } from "../channels/channel-registry";
import { renderTemplate } from "../templates/registry";
import { NotificationMessage } from "../domain/entities/notification-message";
import { NotificationChannel } from "../domain/enums/notification-channel.enum";

export function startNotificationWorker(): Worker<SendNotificationDto> {
  return new Worker<SendNotificationDto>(
    "notifications",
    async (job) => {
      const dto = job.data;

      const channelResult = resolveChannel(dto.channel);
      if (!channelResult.isOk) throw new Error(channelResult.error.message);

      const channelEnum = dto.channel as NotificationChannel;
      const rendered = renderTemplate(channelEnum, dto.templateId, dto.templateData ?? {});
      if (!rendered.isOk) throw new Error(rendered.error.message);

      const message = NotificationMessage.create({
        channel: channelEnum,
        to: dto.to,
        subject: rendered.value.subject,
        body: rendered.value.body,
        templateId: dto.templateId,
      });

      const beginResult = message.beginSend();
      if (!beginResult.isOk) throw new Error(beginResult.error.message);

      const sendResult = await channelResult.value.send({
        to: message.to,
        subject: message.subject,
        body: message.body,
      });

      if (!sendResult.isOk) {
        message.markFailed(sendResult.error.message);
        throw new Error(sendResult.error.message);
      }

      message.markSent();
    },
    {
      connection: getRedisConnection(),
      concurrency: 5,
    },
  );
}
