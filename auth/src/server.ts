import express from "express";
import cors from "cors";
import helmet from "helmet";
import { envConfig } from "./config/env.config";
import { connectDatabase } from "./config/database.config";
import authRoutes from "./presentation/routes/auth.routes";
import { errorMiddleware } from "./presentation/middlewares/error.middleware";
import { setupSwagger } from "./presentation/docs/swagger";

const app = express();

app.use(helmet());
app.use(cors());
app.use(express.json());

app.get("/health", (_req, res) => {
  res.json({ status: "ok", service: "auth-service" });
});

setupSwagger(app);

app.use("/api/auth", authRoutes);

app.use(errorMiddleware);

const start = async () => {
  await connectDatabase();
  app.listen(envConfig.port, () => {
    console.log(`Auth service corriendo en puerto ${envConfig.port} 🚀`);
  });
};

start();
