export abstract class NotificationDomainError {
  abstract readonly code: string;
  abstract readonly message: string;

  toJSON(): { code: string; message: string } {
    return { code: this.code, message: this.message };
  }
}

export class UnsupportedChannelError extends NotificationDomainError {
  readonly code = 'UNSUPPORTED_CHANNEL' as const;
  readonly message = 'Canal de notificación no soportado.';
}

export class UnsupportedTemplateError extends NotificationDomainError {
  readonly code = 'UNSUPPORTED_TEMPLATE' as const;
  readonly message = 'Plantilla de notificación no reconocida.';
}

export class DeliveryFailedError extends NotificationDomainError {
  readonly code = 'DELIVERY_FAILED' as const;
  constructor(private readonly detail: string) { super(); }
  get message(): string { return `Error al enviar notificación: ${this.detail}`; }
}

export class MissingRecipientError extends NotificationDomainError {
  readonly code = 'MISSING_RECIPIENT' as const;
  readonly message = 'El destinatario es requerido.';
}

export class ChannelNotConfiguredError extends NotificationDomainError {
  readonly code = 'CHANNEL_NOT_CONFIGURED' as const;
  constructor(private readonly channel: string) { super(); }
  get message(): string { return `El canal '${this.channel}' no está configurado.`; }
}

export class InvalidStateTransitionError extends NotificationDomainError {
  readonly code = 'INVALID_TRANSITION' as const;
  constructor(private readonly detail: string) { super(); }
  get message(): string { return this.detail; }
}
