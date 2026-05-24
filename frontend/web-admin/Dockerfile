FROM node:22-alpine AS build
WORKDIR /app

RUN npm install -g pnpm@latest

# Deps primero (cache layer)
COPY web-admin/package.json web-admin/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

# Tokens compartidos: van a /shared/ para que styles.css los resuelva via ../../shared/
COPY shared/ /shared/

# Código fuente del webapp
COPY web-admin/ .

ARG BUILD_CONFIG=production
RUN pnpm run build --configuration=$BUILD_CONFIG

FROM nginx:1.27-alpine
RUN apk add --no-cache curl
COPY web-admin/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/web-admin/browser /usr/share/nginx/html
