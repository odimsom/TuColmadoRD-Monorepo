# Root Dockerfile — builds Gateway + API together for single-container deployments.
# For multi-service production use docker-compose.yml instead.
#
# Build:  docker build -t tucolmadord .
# Run:    docker run -p 5032:5032 -e DB_HOST=... tucolmadord

# ─── Stage 1: Build .NET gateway and API ──────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /source

# Copy solution and all project files first for layer caching
COPY backend/TuColmadoRD.slnx backend/
COPY backend/src/core/TuColmadoRD.Core.Domain/TuColmadoRD.Core.Domain.csproj                     backend/src/core/TuColmadoRD.Core.Domain/
COPY backend/src/core/TuColmadoRD.Core.Application/TuColmadoRD.Core.Application.csproj           backend/src/core/TuColmadoRD.Core.Application/
COPY backend/src/infrastructure/TuColmadoRD.Infrastructure.Persistence/TuColmadoRD.Infrastructure.Persistence.csproj   backend/src/infrastructure/TuColmadoRD.Infrastructure.Persistence/
COPY backend/src/infrastructure/TuColmadoRD.Infrastructure.CrossCutting/TuColmadoRD.Infrastructure.CrossCutting.csproj backend/src/infrastructure/TuColmadoRD.Infrastructure.CrossCutting/
COPY backend/src/infrastructure/TuColmadoRD.Infrastructure.IOC/TuColmadoRD.Infrastructure.IOC.csproj                   backend/src/infrastructure/TuColmadoRD.Infrastructure.IOC/
COPY backend/src/Presentations/TuColmadoRD.Presentation.API/TuColmadoRD.Presentation.API.csproj   backend/src/Presentations/TuColmadoRD.Presentation.API/
COPY backend/src/Presentations/TuColmadoRD.ApiGateway/TuColmadoRD.ApiGateway.csproj               backend/src/Presentations/TuColmadoRD.ApiGateway/

RUN dotnet restore backend/src/Presentations/TuColmadoRD.Presentation.API/TuColmadoRD.Presentation.API.csproj
RUN dotnet restore backend/src/Presentations/TuColmadoRD.ApiGateway/TuColmadoRD.ApiGateway.csproj

COPY backend/src/ backend/src/

RUN dotnet publish backend/src/Presentations/TuColmadoRD.Presentation.API/TuColmadoRD.Presentation.API.csproj \
    -c Release -o /app/api
RUN dotnet publish backend/src/Presentations/TuColmadoRD.ApiGateway/TuColmadoRD.ApiGateway.csproj \
    -c Release -o /app/gateway

# ─── Stage 2: Runtime image ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y supervisor && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/api     ./api/
COPY --from=build /app/gateway ./gateway/

COPY <<EOF /etc/supervisor/conf.d/tucolmadord.conf
[supervisord]
nodaemon=true

[program:api]
command=dotnet /app/api/TuColmadoRD.Presentation.API.dll
environment=ASPNETCORE_URLS="http://+:5200",ASPNETCORE_ENVIRONMENT="Production"
autostart=true
autorestart=true
stdout_logfile=/dev/stdout
stdout_logfile_maxbytes=0
stderr_logfile=/dev/stderr
stderr_logfile_maxbytes=0

[program:gateway]
command=dotnet /app/gateway/TuColmadoRD.ApiGateway.dll
environment=ASPNETCORE_URLS="http://+:5032",GatewayOptions__CoreApiUrl="http://localhost:5200"
autostart=true
autorestart=true
stdout_logfile=/dev/stdout
stdout_logfile_maxbytes=0
stderr_logfile=/dev/stderr
stderr_logfile_maxbytes=0
EOF

EXPOSE 5032

CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/supervisord.conf"]
