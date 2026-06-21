# Requiere ejecutarse como Administrador
# Botón derecho > "Ejecutar como Administrador"

$serviceName = "SistemaVentasAPI"
$exePath = "C:\SistemaVentas\api-publish\SistemaVentas.WebAPI.exe"

# Detener y eliminar si ya existe
if (Get-Service $serviceName -ErrorAction SilentlyContinue) {
    sc.exe stop $serviceName
    sc.exe delete $serviceName
    Start-Sleep -Seconds 2
}

# Usar cmd como wrapper para establecer el directorio de trabajo correcto
# (El servicio corre como LocalSystem y el directorio por defecto es C:\Windows\System32)
$binPath = "cmd.exe /c ""cd /d C:\SistemaVentas\api-publish && SistemaVentas.WebAPI.exe"""

sc.exe create $serviceName binPath=$binPath start=auto DisplayName="Sistema Ventas API"

# Configurar para que se reinicie si falla
sc.exe failure $serviceName reset=86400 actions=restart/5000/restart/10000/restart/30000

# Iniciar el servicio
sc.exe start $serviceName
Start-Sleep -Seconds 3

# Verificar
$svc = Get-Service $serviceName -ErrorAction SilentlyContinue
if ($svc.Status -eq 'Running') {
    Write-Host "[OK] Servicio '$serviceName' creado e iniciado correctamente." -ForegroundColor Green
} else {
    Write-Host "[ADVERTENCIA] Servicio creado pero no iniciado. Estado: $($svc.Status)" -ForegroundColor Yellow
    Write-Host "Ejecuta manualmente: sc.exe start $serviceName"
}
