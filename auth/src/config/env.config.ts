import dotenv from "dotenv";
dotenv.config();

export const envConfig = {
  port: process.env.PORT || "3000",
  mongoUri: process.env.MONGODB_URI || "",
  jwt: {
    secret: process.env.JWT_SECRET || "",
    expiresIn: process.env.JWT_EXPIRES_IN || "1d",
  },
  nodeEnv: process.env.NODE_ENV || "development",
  apiurl: process.env.NET_API_URL || "http://localhost:5000",
} as const;
