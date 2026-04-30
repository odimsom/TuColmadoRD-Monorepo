# Producción - Troubleshooting Guide

## Problemas Comunes y Soluciones

### 1. Error 500 - Internal Server Error (endpoints de API)

**Causa probable:** Las migraciones de base de datos no se ejecutaron

**Solución:**
```bash
# Ejecutar migraciones manualmente en el servidor
docker compose exec api dotnet ef database update \
    --project src/infrastructure/TuColmadoRD.Infrastructure.Persistence \
    --startup-project src/Presentations/TuColmadoRD.Presentation.API \
    --configuration Release
```

**Verificación:**
```bash
# Ver logs de la API
docker compose logs -f api

# Conectarse a PostgreSQL para verificar schema
psql -h localhost -U admin_colmado -d TuColmadoDb -c "\dt"
```

---

### 2. CORS Bloqueado - "No 'Access-Control-Allow-Origin' header"

**Causas posibles:**
1. Variables de entorno `PUBLIC_WEB_DOMAIN` no están siendo interpoladas
2. Traefik no está agregando los headers CORS correctamente
3. El Gateway está siendo bypasado por Traefik

**Soluciones:**

**a) Verificar variables de entorno:**
```bash
# En el servidor
cat /app/tucolmadord/.env | grep PUBLIC_

# Verificar que docker-compose las lee correctamente
docker compose config | grep -A5 "GatewayOptions__AllowedOrigins"
```

**b) Verificar configuración de Traefik:**
```bash
# Traefik debe estar corriendo y escuchando en puerto 80/443
curl -I https://api.tucolmadord.com  # Debe retornar 200 o 502, no timeout

# Ver si Traefik está en el proceso
ps aux | grep traefik
```

**c) Agregar headers CORS explícitamente en Traefik:**

Si Traefik no tiene middleware de CORS, agregar en `docker-compose.yml`:

```yaml
gateway:
  # ... existing config ...
  labels:
    - traefik.http.middlewares.tucolmadord-cors.headers.accesscontrolalloworiginlist=https://app.tucolmadord.com,https://tucolmadord.com,https://www.tucolmadord.com
    - traefik.http.middlewares.tucolmadord-cors.headers.accesscontrolallowmethods=GET,POST,PUT,DELETE,OPTIONS
    - traefik.http.middlewares.tucolmadord-cors.headers.accesscontrolallowheaders=*
    - traefik.http.middlewares.tucolmadord-cors.headers.accesscontrolmaxage=3600
    - traefik.http.routers.tucolmadord-api.middlewares=tucolmadord-cors
```

---

### 3. MIME Type Error - CSS served as text/plain

**Causa:** Nginx o web server no está configurando el MIME type correcto

**Solución en Dockerfile de web-admin:**
```dockerfile
# Asegurar que nginx serve assets con MIME types correctos
RUN echo 'types {\n  text/css css;\n  application/javascript js;\n}' >> /etc/nginx/mime.types
```

O en la configuración de nginx (nginx.conf):
```nginx
types {
    text/html html;
    text/css css;
    application/javascript js;
    image/png png;
    image/jpeg jpg;
}
```

---

### 4. Traefik Configuration

**Nota:** Traefik corre como servicio independiente en el servidor (NO en docker-compose del monorepo)

**Verificar instalación:**
```bash
# Traefik debe escuchar en 80/443 y redirigir a los contenedores
ss -tlnp | grep 80
ss -tlnp | grep 443

# Ver configuración de Traefik
cat /etc/traefik/traefik.yml  # o donde esté instalado
```

**Puerto mapping:**
- Traefik 80 → contenedores (con labels)
- Traefik 443 (TLS) → contenedores (con labels)

---

### 5. Database Connection Issues

**Verificar conectividad a PostgreSQL:**
```bash
docker compose exec api psql -h postgres -U admin_colmado -d TuColmadoDb -c "SELECT 1"

# Ver logs de PostgreSQL
docker compose logs postgres
```

---

## Deployment Checklist

- [ ] `.env` tiene todas las variables requeridas
- [ ] `PUBLIC_WEB_DOMAIN`, `PUBLIC_API_DOMAIN` están correctos
- [ ] Traefik está corriendo y escuchando en 80/443
- [ ] Certificados SSL están validos (`docker-compose logs traefik`)
- [ ] Migraciones de base de datos se ejecutaron (`docker compose logs api`)
- [ ] CORS headers están presentes en respuestas de API
- [ ] Assets estáticos se sirven con MIME type correcto

---

## Commands Útiles

```bash
# Rebuild y restart
docker compose down && docker compose up --build -d

# Ver estado de servicios
docker compose ps

# Ver logs de un servicio específico
docker compose logs -f [service_name]

# Ejecutar comando en contenedor
docker compose exec [service_name] [command]

# Rebuild solo la imagen (sin restart)
docker compose build --no-cache [service_name]

# Limpiar volúmenes (⚠️ DESTRUCTIVO)
docker compose down -v
```

---

## Logs a revisar primero

1. **API 500 errors**: `docker compose logs api | grep -i error`
2. **CORS issues**: `docker compose logs gateway | grep -i cors`
3. **Migraciones fallidas**: `docker compose logs api | grep -i migrat`
4. **Traefik routing**: `docker logs traefik 2>&1 | grep tucolmadord`

---

## 6. Container Health Status - "restarting" Loop

**Síntomas:** Contenedor muestra `restarting` en `docker compose ps`, nunca llega a `running`

**Causas posibles:**
1. Aplicación critica en startup (ej: migraciones de base de datos)
2. Dependencias no están listas (base de datos, otros servicios)
3. Límite de reintentos agotado

**Soluciones:**

**a) Ver por qué el contenedor falla:**
```bash
# Ver logs detallados
docker compose logs api | tail -100

# Verificar que los servicios dependientes estén saludables
docker compose ps postgres mongo auth
```

**b) Verificar health status:**
```bash
# Ver salud detallada de cada servicio
docker compose ps --format "table {{.Service}}\t{{.Status}}\t{{.Health}}"

# Si muestra "unhealthy" o "starting", esperar más o revisar logs
```

**c) Reintentos limitados:**
El docker-compose.yml ahora usa `restart: on-failure:3` para evitar loops infinitos:
- Intenta reiniciar máximo 3 veces
- Si falla, el contenedor se queda detenido (ve por qué en logs)
- Solución: `docker compose up -d [service]` para intentar manualmente

**d) Si la base de datos no está lista:**
```bash
# PostgreSQL
docker compose exec postgres pg_isready -U admin_colmado

# MongoDB
docker compose exec mongo mongosh --eval "db.adminCommand('ping')"

# Si falla, esperar o hacer restart
docker compose restart postgres
docker compose restart mongo
```

**e) Recriar servicio desde cero:**
```bash
# Parar e eliminar contenedor (data de volúmenes persiste)
docker compose rm -f api

# Reconstruir e iniciar
docker compose up -d api

# Ver logs mientras inicia
docker compose logs -f api
```

---

## 7. Services Starting Order

El `docker-compose.yml` usa `depends_on` con `condition: service_healthy` para asegurar orden correcto:

1. **PostgreSQL** (database) - con healthcheck `pg_isready`
2. **MongoDB** (auth-database) - con healthcheck `mongosh ping`
3. **Auth** (depende de Mongo) - con healthcheck HTTP
4. **API** (depende de PostgreSQL, espera migrations) - con healthcheck HTTP
5. **ECF Generator** - con healthcheck HTTP
6. **Gateway** (depende de Auth y API) - espera ambos saludables
7. **Landing & Web** - con healthchecks HTTP

Si un servicio de nivel inferior falla, los superiores no inician.

**Verificar orden correcto:**
```bash
docker compose ps --format "table {{.Service}}\t{{.RunningFor}}\t{{.Status}}"

# Esperado:
# postgres     2m ago       Up X seconds (healthy)
# mongo        2m ago       Up X seconds (healthy)  
# auth         1m 30s ago   Up X seconds (healthy)
# api          1m ago       Up X seconds (healthy)
# gateway      45s ago      Up X seconds (healthy)
```
