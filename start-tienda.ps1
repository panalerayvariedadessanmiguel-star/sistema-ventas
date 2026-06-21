$log = "C:\SistemaVentas\startup.log"
$apiDir = "C:\SistemaVentas\api-publish"
$apiExe = "$apiDir\SistemaVentas.WebAPI.exe"
$webDir = "C:\SistemaVentas\tienda-web"
$apiPort = 5062
$webPort = 3000

function Write-Log { param($m) "$(Get-Date) $m" | Out-File $log -Append }

try {
    Write-Log "=== Iniciando Sistema Ventas ==="

    # --- API ---
    if (-not (Get-Process -Name "SistemaVentas.WebAPI" -ErrorAction SilentlyContinue)) {
        $env:ASPNETCORE_URLS = "http://localhost:$apiPort"
        Start-Process -FilePath $apiExe -WorkingDirectory $apiDir -WindowStyle Hidden
        Write-Log "API iniciada (puerto $apiPort)"
    } else {
        Write-Log "API ya estaba corriendo"
    }

    # --- Web (Next.js) ---
    $portInUse = Get-NetTCPConnection -LocalPort $webPort -ErrorAction SilentlyContinue
    if (-not $portInUse) {
        Start-Process -FilePath "cmd.exe" -ArgumentList "/c npm run dev" -WorkingDirectory $webDir -WindowStyle Hidden
        Write-Log "Next.js iniciado (puerto $webPort)"
    } else {
        Write-Log "Next.js ya estaba corriendo"
    }

    # Esperar y abrir navegador
    Start-Sleep -Seconds 15
    Start-Process "http://localhost:$webPort"
    Write-Log "Navegador abierto en http://localhost:$webPort"
}
catch {
    Write-Log "ERROR: $_"
}
