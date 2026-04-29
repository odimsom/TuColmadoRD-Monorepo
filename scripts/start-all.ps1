# start-all.ps1
Write-Host "--- TuColmadoRD Development Environment Starter ---"

# Ports used by our applications
$portsToClear = @(3000, 5000, 8080, 4200, 5173)

Write-Host "`n[1/6] Deteniendo procesos en puertos conflictivos..."
foreach ($port in $portsToClear) {
    try {
        $connections = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
        foreach ($conn in $connections) {
            $pidToKill = $conn.OwningProcess
            if ($pidToKill -gt 0) {
                Write-Host " -> Matando PID $pidToKill ocupando el puerto $port"
                Stop-Process -Id $pidToKill -Force -ErrorAction SilentlyContinue
            }
        }
    } catch {
        # Ignorar errores si no hay procesos
    }
}
Start-Sleep -Seconds 2

Write-Host "`n[2/6] Levantando base de datos y contenedores de backend (Docker)..."
Set-Location -Path "$PSScriptRoot\.."
# Si no tiene docker compose, ignorará el error
try {
    docker-compose up -d postgres mongo
} catch {
    Write-Host " -> Advertencia: No se pudo ejecutar docker-compose. Verifica que Docker este corriendo."
}
Write-Host " -> Esperando 10 segundos para que las DBs se levanten..."
Start-Sleep -Seconds 10

Write-Host "`n[3/6] Generando y aplicando migraciones de base de datos..."
dotnet tool install --global dotnet-ef --version 9.0.2 -ErrorAction SilentlyContinue
dotnet ef database update --project backend/src/infrastructure/TuColmadoRD.Infrastructure.Persistence --startup-project backend/src/Presentations/TuColmadoRD.Presentation.API

Write-Host "`n[4/6] Levantando servicios en procesos separados..."

# 1. Auth Service (Node.js)
Write-Host " -> Levantando Auth API (Puerto 3000)..."
Start-Process powershell -ArgumentList "-NoExit -Command `"cd auth; pnpm install; npm run dev`"" -WindowStyle Normal

# 2. Core API (C#)
Write-Host " -> Levantando Core API (Puerto 8080)..."
Start-Process powershell -ArgumentList "-NoExit -Command `"cd backend/src/Presentations/TuColmadoRD.Presentation.API; dotnet run`"" -WindowStyle Normal

# 3. ECF Generator (Python)
Write-Host " -> Levantando Generador ECF (Puerto 5000)..."
Start-Process powershell -ArgumentList "-NoExit -Command `"cd generadordexmle-cf; pip install -r requirements.txt; python app.py`"" -WindowStyle Normal

# Esperar unos segundos para asegurar que los APIs esten arriba antes del Gateway y Seed
Start-Sleep -Seconds 5

# 4. API Gateway (C#)
Write-Host " -> Levantando API Gateway..."
Start-Process powershell -ArgumentList "-NoExit -Command `"cd backend/src/Presentations/TuColmadoRD.ApiGateway; dotnet run`"" -WindowStyle Normal

# 5. Web Admin (Angular)
Write-Host " -> Levantando Web Admin (Angular)..."
Start-Process powershell -ArgumentList "-NoExit -Command `"cd frontend/web-admin; npm install; npm start`"" -WindowStyle Normal

# 6. Landing Page (Vue)
Write-Host " -> Levantando Landing Page (Vue)..."
Start-Process powershell -ArgumentList "-NoExit -Command `"cd frontend/landing-page; npm install; npm run dev`"" -WindowStyle Normal

Write-Host "`n[5/6] Esperando que la infraestructura inicialice para ejecutar Seed..."
Start-Sleep -Seconds 10

Write-Host "`n[6/6] Ejecutando cuentas de prueba (Seed)..."
Start-Process powershell -ArgumentList "-NoExit -Command `"npx ts-node scripts/seed-test-accounts.ts`"" -WindowStyle Normal

Write-Host "`n¡Todo ha sido levantado correctamente! Revisa las ventanas de PowerShell individuales."
