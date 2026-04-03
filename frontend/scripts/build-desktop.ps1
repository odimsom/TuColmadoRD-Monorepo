# TuColmadoRD Desktop Build Script

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
# Default root is sibling of the frontend repo root, but allow override via env var
# Locally: $Root = C:\Users\Francisco C. Dev\source\repos
# CI: $Root = D:\a\TuColmadoRD.Frontend\TuColmadoRD.Frontend (if we clone as siblings there)
$Root = if ($env:TU_COLMADORD_ROOT) { Resolve-Path $env:TU_COLMADORD_ROOT } else { Resolve-Path "$ScriptDir/../.." }

Write-Host "--- Using Root Path: $Root ---" -ForegroundColor Gray

# 0. Detect repository folders (handle local vs CI differences)
$BackendDir = "$Root/TuColmadoRD"
$FrontendDir = "$Root/TuColmadoRD.Frontend"

if (-not (Test-Path $BackendDir)) {
    # Fallback for CI if cloned as 'backend' and 'frontend'
    $BackendDir = "$Root/backend"
    $FrontendDir = "$Root/frontend"
}

# 1. Read version and handle suffix
$package = Get-Content "$ScriptDir/../package.json" | ConvertFrom-Json
$baseVersion = $package.version
$suffix = if ($env:RELEASE_TYPE -eq "test") { "-test" } else { "" }
$appVersion = if ($env:APP_VERSION) { $env:APP_VERSION } else { "$baseVersion$suffix" }
$env:APP_VERSION = $appVersion # For Inno Setup

Write-Host "--- TuColmadoRD Build v$appVersion ---" -ForegroundColor Yellow

# 2. Build Angular with configuration 'local'
Write-Host "--- 1. Building Angular (Local) ---" -ForegroundColor Cyan
Set-Location "$FrontendDir/web-admin"
if (-not (Test-Path "node_modules")) { npm install }
# Use root base/deploy URLs so chunk files resolve correctly under /auth/login and other deep links.
npx ng build --configuration local --output-path ../dist-desktop --base-href / --deploy-url /

# 3. Copy dist to wwwroot of the Desktop project
Write-Host "--- 2. Copying Angular build to Desktop wwwroot ---" -ForegroundColor Cyan
$wwwroot = "$BackendDir/src/Presentations/TuColmadoRD.Desktop/wwwroot"
if (Test-Path $wwwroot) { Remove-Item $wwwroot -Recurse -Force }
New-Item -ItemType Directory -Path $wwwroot | Out-Null
$distBrowser = "$FrontendDir/dist-desktop/browser"
$distRoot = "$FrontendDir/dist-desktop"
$sourceDist = if (Test-Path $distBrowser) { $distBrowser } elseif (Test-Path $distRoot) { $distRoot } else { $null }

if (-not $sourceDist) {
    throw "Angular build output not found. Expected '$distBrowser' or '$distRoot'."
}

Copy-Item "$sourceDist/*" $wwwroot -Recurse -Force

# 4. Publish the .exe self-contained
Write-Host "--- 3. Publishing .NET Desktop App ---" -ForegroundColor Cyan
dotnet publish "$BackendDir/src/Presentations/TuColmadoRD.Desktop/TuColmadoRD.Desktop.csproj" `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:ExcludeAppSettingsFromPublish=true `
    -p:ErrorOnDuplicatePublishOutputFiles=false `
  --output "$BackendDir/publish/desktop"

Write-Host "EXE listo en $BackendDir/publish/desktop/TuColmadoRD.Desktop.exe" -ForegroundColor Green

# 5. Compilar installer con Inno Setup (si esta instalado)
$inno = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (Test-Path $inno) {
    Write-Host "--- 4. Generating Installer v$appVersion ---" -ForegroundColor Cyan
    & $inno /DAppVersion=$appVersion "$BackendDir/installer/setup.iss"
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup failed with exit code $LASTEXITCODE"
    }
    Write-Host "Installer generado en $BackendDir/publish/installer/TuColmadoRD-Setup-v$appVersion.exe" -ForegroundColor Green
} else {
    Write-Host "Inno Setup no encontrado - saltando generacion de installer" -ForegroundColor Yellow
}
