FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS test-dotnet
WORKDIR /source

COPY backend/ .

RUN dotnet restore TuColmadoRD.slnx

RUN dotnet build TuColmadoRD.slnx \
        --configuration Release \
        --no-restore

RUN dotnet test TuColmadoRD.slnx \
        --configuration Release \
        --no-build \
        --logger "console;verbosity=normal" \
    && touch /dotnet-ok

FROM node:22-alpine AS test-auth
WORKDIR /app

RUN npm install -g pnpm

COPY auth/package.json auth/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY auth/ .

RUN pnpm test -- --ci \
    && touch /auth-ok

FROM node:22-alpine AS test-frontend
WORKDIR /app

COPY frontend/web-admin/ .
RUN npm ci

ENV CI=true
RUN npx ng test \
    && touch /frontend-ok

FROM alpine:3.19 AS ci
COPY --from=test-dotnet   /dotnet-ok   /results/dotnet-ok
COPY --from=test-auth     /auth-ok     /results/auth-ok
COPY --from=test-frontend /frontend-ok /results/frontend-ok
RUN echo "All unit tests passed" && ls /results/
