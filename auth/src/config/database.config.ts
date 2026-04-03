import mongoose from "mongoose";
import { envConfig } from "./env.config";

export const connectDatabase = async (): Promise<void> => {
  try {
    await mongoose.connect(envConfig.mongoUri);
    console.log("MongoDB conectado ✅");
  } catch (error) {
    console.error("Error conectando MongoDB ❌", error);
    process.exit(1);
  }
};
