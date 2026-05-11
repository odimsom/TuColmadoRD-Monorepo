import nodemailer from "nodemailer";
import { INotificationChannel, NotificationPayload } from "../domain/interfaces/notification-channel.interface";
import { NotificationChannel } from "../domain/enums/notification-channel.enum";
import { OperationResult } from "../domain/result/operation-result";
import { NotificationDomainError, DeliveryFailedError } from "../domain/errors/notification-error";
import { env } from "../config/env.config";

export class EmailChannel implements INotificationChannel {
  readonly channelId = NotificationChannel.EMAIL;

  async send(payload: NotificationPayload): Promise<OperationResult<void, NotificationDomainError>> {
    return env.resend.apiKey
      ? this.sendViaResend(payload)
      : this.sendViaSmtp(payload);
  }

  private async sendViaResend(payload: NotificationPayload): Promise<OperationResult<void, NotificationDomainError>> {
    try {
      const res = await fetch("https://api.resend.com/emails", {
        method: "POST",
        headers: {
          Authorization: `Bearer ${env.resend.apiKey}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          from: env.smtp.from,
          to: [payload.to],
          subject: payload.subject ?? "(sin asunto)",
          html: payload.body,
        }),
      });

      if (!res.ok) {
        const text = await res.text();
        return OperationResult.fail(new DeliveryFailedError(`Resend ${res.status}: ${text}`));
      }

      return OperationResult.ok(undefined);
    } catch (err: any) {
      return OperationResult.fail(new DeliveryFailedError(err?.message ?? 'Resend error'));
    }
  }

  private async sendViaSmtp(payload: NotificationPayload): Promise<OperationResult<void, NotificationDomainError>> {
    try {
      const transporter = nodemailer.createTransport({
        host: env.smtp.host,
        port: env.smtp.port,
        secure: env.smtp.secure,
        ...(env.smtp.user && { auth: { user: env.smtp.user, pass: env.smtp.pass } }),
      });

      await transporter.sendMail({
        from: env.smtp.from,
        to: payload.to,
        subject: payload.subject,
        html: payload.body,
      });

      return OperationResult.ok(undefined);
    } catch (err: any) {
      return OperationResult.fail(new DeliveryFailedError(err?.message ?? 'SMTP error'));
    }
  }
}
