import { envConfig } from "../../config/env.config";

export class EmailServiceClient {
  private readonly baseUrl: string;
  private readonly secret: string;

  constructor() {
    this.baseUrl = envConfig.emailServiceUrl;
    this.secret = envConfig.serviceSecret;
  }

  private async notify(templateId: string, to: string, data: Record<string, string>): Promise<void> {
    const res = await fetch(`${this.baseUrl}/api/v1/notifications/send`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "x-service-secret": this.secret,
      },
      body: JSON.stringify({ channel: "email", to, templateId, templateData: data }),
    });

    if (!res.ok) {
      const body = await res.text();
      throw new Error(`Notification service ${res.status}: ${body}`);
    }
  }

  async sendVerificationEmail(to: string, code: string, businessName: string): Promise<void> {
    await this.notify("beta-welcome", to, { code, businessName });
  }

  async sendAccountVerifiedEmail(to: string, firstName: string, businessName: string): Promise<void> {
    await this.notify("account-verified", to, { firstName, businessName });
  }

  async sendAppTutorialEmail(to: string, firstName: string): Promise<void> {
    await this.notify("app-tutorial", to, { firstName });
  }

  async sendWelcomeEmail(to: string, firstName: string): Promise<void> {
    await this.notify("welcome", to, { firstName });
  }
}
