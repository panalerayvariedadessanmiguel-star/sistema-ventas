$log = "C:\SistemaVentas\api-publish\startup.log"
$exe = "C:\SistemaVentas\api-publish\SistemaVentas.WebAPI.exe"
$dir = "C:\SistemaVentas\api-publish"

"$(Get-Date) - Iniciando script start-api.ps1" | Out-File $log -Append

# Matar cualquier proceso que tenga el puerto 5062 ocupado
$port = 5062
$pidPort = netstat -ano | Select-String ":$port " | ForEach-Object { $_ -replace '.*\s+(\d+)$', '$1' } | Select-Object -First 1
if ($pidPort -and $pidPort -ne '0') {
    $proc = Get-Process -Id $pidPort -ErrorAction SilentlyContinue
    if ($proc) {
        "$(Get-Date) - Matando proceso $($proc.Name) PID $pidPort que ocupaba puerto $port" | Out-File $log -Append
        Stop-Process -Id $pidPort -Force
        Start-Sleep -Seconds 2
    }
}

$env:ASPNETCORE_URLS="http://localhost:$port"
Start-Process -FilePath $exe -WorkingDirectory $dir -WindowStyle Hidden
"$(Get-Date) - API iniciada en puerto $port" | Out-File $log -Append
