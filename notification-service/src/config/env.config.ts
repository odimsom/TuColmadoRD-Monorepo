import dotenv from "dotenv";
dotenv.config();

export const env = {
  port: process.env.PORT || "4000",
  redis: {
    url: process.env.REDIS_URL || "redis://localhost:6379",
  },
  smtp: {
    host: process.env.SMTP_HOST || "mailhog",
    port: parseInt(process.env.SMTP_PORT || "1025"),
    secure: process.env.SMTP_SECURE === "true",
    user: process.env.SMTP_USER || "",
    pass: process.env.SMTP_PASS || "",
    from: process.env.SMTP_FROM || "TuColmado RD <noreply@tucolmadord.com>",
  },
  twilio: {
    accountSid: process.env.TWILIO_ACCOUNT_SID || "",
    authToken: process.env.TWILIO_AUTH_TOKEN || "",
    fromNumber: process.env.TWILIO_FROM || "",
  },
  resend: {
    apiKey: process.env.RESEND_API_KEY || "",
  },
  serviceSecret: process.env.SERVICE_SECRET || "internal-secret",
} as const;
