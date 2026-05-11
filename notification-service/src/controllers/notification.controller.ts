import { Request, Response, NextFunction } from "express";
import { NotificationService } from "../services/notification.service";
import {
  NotificationDomainError,
  UnsupportedChannelError,
  UnsupportedTemplateError,
  MissingRecipientError,
  ChannelNotConfiguredError,
} from "../domain/errors/notification-error";

function errorStatus(error: NotificationDomainError): number {
  if (error instanceof UnsupportedChannelError)    return 400;
  if (error instanceof UnsupportedTemplateError)   return 400;
  if (error instanceof MissingRecipientError)      return 400;
  if (error instanceof ChannelNotConfiguredError)  return 503;
  return 502;
}

const notificationService = new NotificationService();

export class NotificationController {
  async send(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { channel, to, templateId, templateData } = req.body as {
        channel: string;
        to: string;
        templateId: string;
        templateData?: Record<string, string>;
      };

      if (!to) {
        res.status(400).json(new MissingRecipientError().toJSON());
        return;
      }

      const result = await notificationService.send({ channel, to, templateId, templateData });

      if (!result.isOk) {
        res.status(errorStatus(result.error)).json(result.error.toJSON());
        return;
      }

      res.status(200).json({ delivered: true });
    } catch (error) {
      next(error);
    }
  }
}
