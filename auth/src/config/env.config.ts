import dotenv from "dotenv";
import path from "path";
dotenv.config({ path: path.resolve(__dirname, "../../.env") });

export const envConfig = {
  port: process.env.PORT || "3000",
  mongoUri: process.env.MONGODB_URI || "",
  jwt: {
    secret: process.env.JWT_SECRET || "",
    expiresIn: process.env.JWT_EXPIRES_IN || "7d",
  },
  nodeEnv: process.env.NODE_ENV || "development",
  apiurl: process.env.NET_API_URL || "http://localhost:5000",
  emailServiceUrl: process.env.EMAIL_SERVICE_URL || "http://email-service:4000",
  serviceSecret: process.env.SERVICE_SECRET || "internal-secret",
} as const;
