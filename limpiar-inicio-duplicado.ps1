# Auto-elevarse si no es Administrador
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    $args = "-NoProfile -ExecutionPolicy Bypass -File `"" + $MyInvocation.MyCommand.Path + "`""
    Start-Process powershell.exe -Verb RunAs -ArgumentList $args
    exit
}

Write-Host "Limpiando entradas de inicio duplicadas..." -ForegroundColor Cyan

try {
    Unregister-ScheduledTask -TaskName "SistemaVentas Web" -Confirm:$false
    Write-Host "  [OK] Tarea programada 'SistemaVentas Web' eliminada" -ForegroundColor Green
} catch {
    Write-Host "  [!] No se pudo eliminar la tarea programada: $_" -ForegroundColor Yellow
}

try {
    Stop-Service SistemaVentasAPI -Force -ErrorAction SilentlyContinue
    Set-Service SistemaVentasAPI -StartupType Disabled
    Write-Host "  [OK] Servicio 'SistemaVentasAPI' detenido y deshabilitado" -ForegroundColor Green
} catch {
    Write-Host "  [!] No se pudo deshabilitar el servicio: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Hecho. Ahora al reiniciar solo se ejecutará el script limpio." -ForegroundColor Cyan
Write-Host ""
Write-Host "Presiona cualquier tecla para salir..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
