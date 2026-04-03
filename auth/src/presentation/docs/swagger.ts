import { Express, Request, Response } from "express";
import swaggerUi from "swagger-ui-express";
import swaggerJsdoc from "swagger-jsdoc";

const swaggerSpec = swaggerJsdoc({
  definition: {
    openapi: "3.0.3",
    info: {
      title: "TuColmado Auth API",
      version: "1.0.0",
      description: "Documentacion del servicio de autenticacion de TuColmadoRD",
    },
    servers: [
      {
        url: "/",
      },
    ],
    tags: [
      { name: "Health" },
      { name: "Auth" },
    ],
    paths: {
      "/health": {
        get: {
          tags: ["Health"],
          summary: "Health check",
          responses: {
            "200": {
              description: "Servicio activo",
            },
          },
        },
      },
      "/api/auth/register": {
        post: {
          tags: ["Auth"],
          summary: "Registrar usuario y tenant",
          requestBody: {
            required: true,
            content: {
              "application/json": {
                schema: { type: "object" },
              },
            },
          },
          responses: {
            "201": { description: "Registrado" },
            "400": { description: "Solicitud invalida" },
          },
        },
      },
      "/api/auth/login": {
        post: {
          tags: ["Auth"],
          summary: "Iniciar sesion",
          requestBody: {
            required: true,
            content: {
              "application/json": {
                schema: { type: "object" },
              },
            },
          },
          responses: {
            "200": { description: "Login exitoso" },
            "401": { description: "Credenciales invalidas" },
          },
        },
      },
      "/api/auth/pair-device": {
        post: {
          tags: ["Auth"],
          summary: "Emparejar dispositivo local",
          requestBody: {
            required: true,
            content: {
              "application/json": {
                schema: { type: "object" },
              },
            },
          },
          responses: {
            "200": { description: "Dispositivo emparejado" },
            "401": { description: "No autorizado" },
            "404": { description: "Tenant no encontrado" },
          },
        },
      },
      "/api/auth/renew-license": {
        post: {
          tags: ["Auth"],
          summary: "Renovar licencia",
          security: [{ bearerAuth: [] }],
          responses: {
            "200": { description: "Licencia renovada" },
            "401": { description: "No autorizado" },
            "403": { description: "Renovacion rechazada" },
          },
        },
      },
    },
    components: {
      securitySchemes: {
        bearerAuth: {
          type: "http",
          scheme: "bearer",
          bearerFormat: "JWT",
        },
      },
    },
  },
  apis: [],
});

export const setupSwagger = (app: Express): void => {
  const swaggerJsonHandler = (_req: Request, res: Response): void => {
    res.json(swaggerSpec);
  };

  // Backward-compatible aliases for gateway/proxy path differences.
  app.get("/api/docs/v1/swagger.json", swaggerJsonHandler);
  app.get("/api/docs/swagger.json", swaggerJsonHandler);
  app.get("/swagger.json", swaggerJsonHandler);

  app.use("/api/docs", swaggerUi.serve, swaggerUi.setup(swaggerSpec));
};
