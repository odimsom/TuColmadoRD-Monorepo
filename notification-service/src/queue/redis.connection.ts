import { Redis } from "ioredis";
import { env } from "../config/env.config";

let _connection: Redis | null = null;

export function getRedisConnection(): Redis {
  if (!_connection) {
    _connection = new Redis(env.redis.url, { maxRetriesPerRequest: null });
  }
  return _connection;
}
