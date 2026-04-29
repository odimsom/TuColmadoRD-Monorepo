FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
# Para producción real usar: npm run build
# Para stack Docker local usar configuración de desarrollo (gatewayUrl → localhost:5032)
ARG BUILD_CONFIG=development
RUN npm run build -- --configuration=$BUILD_CONFIG

FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/web-admin/browser /usr/share/nginx/html
