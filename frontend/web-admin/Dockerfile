FROM node:22-alpine AS build
WORKDIR /app

RUN npm install -g pnpm@latest

COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY . .
ARG BUILD_CONFIG=production
RUN pnpm run build -- --configuration=$BUILD_CONFIG

FROM nginx:1.27-alpine
RUN apk add --no-cache curl
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/web-admin/browser /usr/share/nginx/html
