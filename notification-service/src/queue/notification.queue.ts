import { Queue } from "bullmq";
import { getRedisConnection } from "./redis.connection";
import type { SendNotificationDto } from "../services/notification.service";

export const notificationQueue = new Queue<SendNotificationDto>("notifications", {
  connection: getRedisConnection(),
  defaultJobOptions: {
    attempts: 4,
    backoff: { type: "exponential", delay: 5_000 },
    removeOnComplete: { count: 100 },
    removeOnFail: { count: 200 },
  },
});
