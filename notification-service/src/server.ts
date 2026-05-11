import express from "express";
import { env } from "./config/env.config";
import notificationRoutes from "./routes/notification.routes";
import { startNotificationWorker } from "./queue/notification.worker";

const app = express();
app.use(express.json());

app.get("/health", (_req, res) => res.json({ status: "ok", service: "notification-service" }));
app.use("/api/v1/notifications", notificationRoutes);

app.use((err: Error, _req: express.Request, res: express.Response, _next: express.NextFunction) => {
  res.status(500).json({ code: "INTERNAL_ERROR", message: err.message });
});

startNotificationWorker();

app.listen(env.port, () => {
  process.stdout.write(`Notification service on port ${env.port}\n`);
});
