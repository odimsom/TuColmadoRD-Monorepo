FROM node:22-alpine AS build
WORKDIR /app

# Copy shared assets if needed (handled by path relative to context)
COPY shared/ ../shared/

COPY web-admin/package*.json ./
RUN npm ci

COPY web-admin/ .
ARG BUILD_CONFIG=production
RUN npm run build -- --configuration=$BUILD_CONFIG

FROM nginx:1.27-alpine
RUN apk add --no-cache curl
COPY web-admin/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/web-admin/browser /usr/share/nginx/html
