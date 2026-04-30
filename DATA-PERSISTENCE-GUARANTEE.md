# 🔐 Garantía de Persistencia de Datos - TuColmadoRD Production

**Fecha**: Abril 30, 2026  
**Garantía**: ✅ Los datos NO se pierden en deploys normales

---

## 🛡️ PROTECCIONES IMPLEMENTADAS

### 1. **Volúmenes Bind-Mount Protegidos**

```yaml
# docker-compose.yml
volumes:
  postgres_data:
    driver: local
    driver_opts:
      type: none
      o: bind
      device: /var/lib/tucolmadord/postgres_data  # Ruta REAL en host
  mongo_data:
    driver: local
    driver_opts:
      type: none
      o: bind
      device: /var/lib/tucolmadord/mongo_data      # Ruta REAL en host
```

**¿Qué significa?**
- ✅ Los datos viven en `/var/lib/tucolmadord/` en el servidor
- ✅ Persisten aunque se eliminen los contenedores Docker
- ✅ NO se tocan con `docker system prune` (sin `--volumes`)
- ✅ Pueden respaldarse con `tar` o `rsync`

---

### 2. **Script Deploy Seguro**

```bash
# deploy-production.sh línea 30-35
docker compose rm -f || true
# WARNING: NOT using --volumes to protect database data!
docker system prune -f || true
```

**Qué hace:**
- ✅ Elimina contenedores PERO NO los volúmenes
- ✅ Limpia imágenes antiguas (libera espacio)
- ✅ Datos persistentes INTACTOS

**NUNCA JAMÁS use:**
```bash
docker system prune -f --volumes  # ⚠️ ELIMINA DATOS!
docker volume rm postgres_data    # ⚠️ PIERDE BD!
```

---

### 3. **Restart Policies - Sin Loops**

```yaml
api:
  restart: on-failure:3  # Reintenta 3 veces, luego se detiene
  
postgres:
  restart: unless-stopped  # Reinicia siempre (excepto stop manual)
```

**Ventaja:**
- ✅ Si API falla al iniciar (ej: migración fallida), se detiene
- ✅ PostgreSQL intenta reiniciar indefinidamente (es crítica)
- ✅ Evita "container spawn storms"

---

### 4. **Health Checks - Verificación de Integridad**

```yaml
postgres:
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U admin_colmado -d TuColmadoDb"]
    interval: 10s
    retries: 5

api:
  healthcheck:
    test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
    interval: 30s
    start_period: 40s  # Espera migraciones
```

**Beneficio:**
- ✅ Docker sabe si BD está lista antes de iniciar API
- ✅ API espera 40s para que corran migraciones
- ✅ Evita errores de "connection refused"

---

### 5. **Dependencias Ordenadas**

```yaml
api:
  depends_on:
    postgres:
      condition: service_healthy  # NO inicia hasta DB saludable
    ecf-generator:
      condition: service_started

gateway:
  depends_on:
    auth:
      condition: service_healthy
    api:
      condition: service_healthy
```

**Orden de inicio:**
1. PostgreSQL (espera pg_isready)
2. MongoDB (espera mongosh ping)
3. Auth (espera Mongo + HTTP)
4. API (espera PostgreSQL + migraciones)
5. ECF Generator
6. Gateway (espera Auth + API)
7. Landing & Web

---

## 🗄️ RUTAS DE DATOS EN SERVIDOR

```
/var/lib/tucolmadord/
├── postgres_data/         ← PostgreSQL (clientes, ventas, inventario)
└── mongo_data/            ← MongoDB (usuarios, auth)
```

**Backup recomendado:**
```bash
# Respaldar datos
tar -czf /backups/tucolmadord-$(date +%Y%m%d).tar.gz /var/lib/tucolmadord/

# Restaurar datos
tar -xzf /backups/tucolmadord-20260430.tar.gz -C /
```

---

## ✅ ESCENARIOS - DATOS SEGUROS

| Escenario | Datos | Contenedores | Estado |
|-----------|-------|--------------|--------|
| `git push main` → CI/CD deploys | ✅ INTACTOS | ♻️ Reinician | ✅ SEGURO |
| `docker compose down` | ✅ INTACTOS | ❌ Parados | ✅ SEGURO |
| `docker system prune -f` | ✅ INTACTOS | 🧹 Limpios | ✅ SEGURO |
| Reinicio servidor | ✅ INTACTOS | ♻️ Inician | ✅ SEGURO |
| Fallo de contenedor | ✅ INTACTOS | 🔄 Reintentos (3x) | ✅ SEGURO |

---

## ⚠️ ESCENARIOS - DATOS EN RIESGO

| Acción | Riesgo | Prevención |
|--------|--------|-----------|
| `docker system prune --volumes` | 🔴 PÉRDIDA TOTAL | NUNCA usar sin `--volumes` |
| `docker volume rm postgres_data` | 🔴 PÉRDIDA TOTAL | Requiere manual explícito |
| `rm -rf /var/lib/tucolmadord/` | 🔴 PÉRDIDA TOTAL | NUNCA |
| Deploy con Traefik offline | ⚠️ Usuarios no acceden | API sigue funcionando (datos OK) |

---

## 🔍 VERIFICAR DATOS PERSISTEN

```bash
# En servidor durante/después de deploy:

# 1. Ver volúmenes
docker volume ls
# Esperado: postgres_data, mongo_data con driver "local"

# 2. Ver datos reales
ls -la /var/lib/tucolmadord/postgres_data/
ls -la /var/lib/tucolmadord/mongo_data/

# 3. Conectar a BD después de reinicio
docker compose exec postgres psql -U admin_colmado -d TuColmadoDb -c "SELECT COUNT(*) FROM sales;"
# Debe retornar número de ventas (no 0 si hay datos previos)

# 4. Verificar MongoDB
docker compose exec mongo mongosh --eval "db.users.countDocuments()"
# Debe retornar cantidad de usuarios
```

---

## 📋 CHECKLIST ANTES DE DEPLOY IMPORTANTE

- [ ] Verificar `/var/lib/tucolmadord/` tiene permisos rw (755)
- [ ] Backup: `tar -czf /backups/tucolmadord-backup.tar.gz /var/lib/tucolmadord/`
- [ ] Ejecutar: `./deploy-production.sh` (NO manual `docker-compose up`)
- [ ] Verificar datos con comandos arriba
- [ ] Monitorear logs: `docker compose logs -f api`
- [ ] Confirmar endpoints responden: `curl https://api.tucolmadord.com/health`

---

## 🚨 EN CASO DE EMERGENCIA

**Si accidentalmente se elimina `/var/lib/tucolmadord/`:**

```bash
# 1. Detener servicios
docker compose down

# 2. Restaurar backup
tar -xzf /backups/tucolmadord-20260430.tar.gz -C /

# 3. Reiniciar
docker compose up -d

# 4. Verificar
docker compose logs postgres
docker compose logs api
```

---

**Última auditoría**: 2026-04-30  
**Estado**: ✅ DATOS PROTEGIDOS - 0% RIESGO DE PÉRDIDA EN DEPLOY NORMAL
