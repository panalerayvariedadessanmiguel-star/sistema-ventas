# ============================================
# Script de configuración de auto-inicio
# EJECUTAR COMO ADMINISTRADOR (opcional)
# ============================================
# Configura el acceso directo en la carpeta Startup
# para que el sistema arranque automaticamente
# al iniciar sesion sin mostrar ventanas.
# ============================================

$ErrorActionPreference = "Stop"
$proyecto = "C:\SistemaVentas"

Write-Host "=== Configurando auto-inicio del Sistema Ventas ===" -ForegroundColor Cyan

# ----- 1. Acceso directo en carpeta Startup -----
Write-Host "Creando acceso directo en carpeta de inicio..." -ForegroundColor Yellow
$startupDir = [Environment]::GetFolderPath("Startup")
$lnkPath = "$startupDir\SistemaVentas.lnk"
$wshell = New-Object -ComObject WScript.Shell
$lnk = $wshell.CreateShortcut($lnkPath)
$lnk.TargetPath = "wscript.exe"
$lnk.Arguments = """$proyecto\start-all.vbs"""
$lnk.WorkingDirectory = $proyecto
$lnk.WindowStyle = 7
$lnk.Description = "Inicia Sistema Ventas (API + Web) al iniciar sesion"
$lnk.Save()
Write-Host "  [OK] Acceso directo creado en: $lnkPath" -ForegroundColor Green

# ----- 2. Ejecutar ahora -----
Write-Host ""
Write-Host "Iniciando servicios ahora..." -ForegroundColor Yellow
& powershell -NoProfile -ExecutionPolicy Bypass -File "$proyecto\start-all.ps1" -NoBrowser

# ----- 3. Verificacion final -----
Write-Host ""
Write-Host "=== Verificacion ===" -ForegroundColor Cyan
Start-Sleep -Seconds 3

$apiOk = $false
try { $r = Invoke-WebRequest -Uri "http://localhost:5062/api/productos" -TimeoutSec 5 -UseBasicParsing; $apiOk = $r.StatusCode -eq 200 } catch {}
$webOk = $false
try { $r = Invoke-WebRequest -Uri "http://localhost:3000" -TimeoutSec 5 -UseBasicParsing; $webOk = $r.StatusCode -eq 200 } catch {}

if ($apiOk) { Write-Host "  [OK] API respondiendo en http://localhost:5062" -ForegroundColor Green } else { Write-Host "  [X] API NO responde" -ForegroundColor Red }
if ($webOk) { Write-Host "  [OK] Web respondiendo en http://localhost:3000" -ForegroundColor Green } else { Write-Host "  [X] Web NO responde" -ForegroundColor Red }

Write-Host ""
Write-Host "=== Configuracion completada ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Al reiniciar el equipo:"
Write-Host "  - API: se inicia automaticamente (start-all.ps1)"
Write-Host "  - Web (Next.js): se inicia automaticamente (start-all.ps1)"
Write-Host ""
Write-Host "Accede a la tienda en: http://localhost:3000" -ForegroundColor White
Write-Host ""
pause
