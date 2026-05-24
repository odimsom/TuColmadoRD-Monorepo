FROM node:22-alpine AS build
WORKDIR /app/frontend/landing-page

ARG VITE_WEB_ADMIN_URL=https://app.tucolmadord.com
ENV VITE_WEB_ADMIN_URL=${VITE_WEB_ADMIN_URL}

RUN corepack enable && corepack prepare pnpm@latest --activate

# Copy shared assets to maintain relative path ../../../../shared
# (style.css is at src/assets/css/style.css, so 4 levels up to reach shared)
COPY shared/ ../shared/

COPY landing-page/package.json landing-page/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY landing-page/ .
RUN pnpm run build-only

FROM nginx:1.27-alpine
RUN apk add --no-cache curl
COPY landing-page/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/frontend/landing-page/dist /usr/share/nginx/html
