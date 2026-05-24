FROM node:22-alpine AS build
WORKDIR /app/frontend/web-admin

# Copy shared assets to maintain relative path ../../shared
COPY shared/ ../shared/

COPY web-admin/package*.json ./
RUN npm ci

COPY web-admin/ .
ARG BUILD_CONFIG=production
RUN npm run build -- --configuration=$BUILD_CONFIG

FROM nginx:1.27-alpine
RUN apk add --no-cache curl
COPY web-admin/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/frontend/web-admin/dist/tucolmadord-webapp/browser /usr/share/nginx/html
