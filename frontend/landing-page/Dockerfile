FROM node:22-alpine AS build
WORKDIR /app

ARG VITE_WEB_ADMIN_URL=https://app.tucolmadord.com
ENV VITE_WEB_ADMIN_URL=${VITE_WEB_ADMIN_URL}

RUN corepack enable && corepack prepare pnpm@latest --activate

COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY . .
RUN pnpm run build-only

FROM nginx:1.27-alpine
RUN apk add --no-cache curl
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
