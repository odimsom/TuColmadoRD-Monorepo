FROM node:22-alpine AS build
WORKDIR /app

ARG VITE_WEB_ADMIN_URL
ENV VITE_WEB_ADMIN_URL=${VITE_WEB_ADMIN_URL}

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:1.27-alpine
RUN apk add --no-cache curl
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
