FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
ARG BUILD_CONFIG=production
RUN npm run build -- --configuration=$BUILD_CONFIG

FROM nginx:1.27-alpine
RUN apk add --no-cache curl
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/web-admin/browser /usr/share/nginx/html
