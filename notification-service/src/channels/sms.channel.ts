import { INotificationChannel, NotificationPayload } from "../domain/interfaces/notification-channel.interface";
import { NotificationChannel } from "../domain/enums/notification-channel.enum";
import { OperationResult } from "../domain/result/operation-result";
import { NotificationDomainError, ChannelNotConfiguredError, DeliveryFailedError } from "../domain/errors/notification-error";
import { env } from "../config/env.config";

export class SmsChannel implements INotificationChannel {
  readonly channelId = NotificationChannel.SMS;

  async send(payload: NotificationPayload): Promise<OperationResult<void, NotificationDomainError>> {
    if (!env.twilio.accountSid || !env.twilio.authToken) {
      return OperationResult.fail(new ChannelNotConfiguredError('sms'));
    }

    try {
      const body = new URLSearchParams({
        To: payload.to,
        From: env.twilio.fromNumber,
        Body: payload.body ?? '',
      });

      const response = await fetch(
        `https://api.twilio.com/2010-04-01/Accounts/${env.twilio.accountSid}/Messages.json`,
        {
          method: 'POST',
          headers: {
            Authorization: `Basic ${Buffer.from(`${env.twilio.accountSid}:${env.twilio.authToken}`).toString('base64')}`,
            'Content-Type': 'application/x-www-form-urlencoded',
          },
          body: body.toString(),
        },
      );

      if (!response.ok) {
        const text = await response.text();
        return OperationResult.fail(new DeliveryFailedError(`Twilio ${response.status}: ${text}`));
      }

      return OperationResult.ok(undefined);
    } catch (err: any) {
      return OperationResult.fail(new DeliveryFailedError(err?.message ?? 'Twilio error'));
    }
  }
}
