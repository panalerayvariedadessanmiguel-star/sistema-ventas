param([switch]$NoBrowser)

$log = "C:\SistemaVentas\logs\startup.log"
$lockFile = "C:\SistemaVentas\logs\startup.lock"
$apiDir = "C:\SistemaVentas\api-publish-v2"
$webDir = "C:\SistemaVentas\tienda-web"
$apiPort = 5063
$webPort = 3000
$presentationExe = "C:\SistemaVentas\SistemaVentas.Presentation\bin\Debug\net8.0-windows\SistemaVentas.Presentation.exe"
$ecosystemFile = "C:\SistemaVentas\ecosystem.config.js"

if (!(Test-Path "C:\SistemaVentas\logs")) { New-Item -ItemType Directory "C:\SistemaVentas\logs" -Force | Out-Null }

function Log { param($m) "$(Get-Date -Format 'HH:mm:ss') $m" | Out-File $log -Append }

# Evitar ejecución duplicada
if (Test-Path $lockFile) {
    $lockTime = (Get-Item $lockFile).LastWriteTime
    if (((Get-Date) - $lockTime).TotalMinutes -lt 5) {
        Log "=== Script ya ejecutado hace menos de 5min, saliendo ==="
        exit
    }
}
[System.IO.File]::WriteAllText($lockFile, (Get-Date).ToString('o'))

Log "=== Iniciando Sistema Ventas ==="

# 1. Liberar puerto 5062 (forzar cierre)
Log "Verificando puerto $apiPort..."
$foundPids = netstat -ano | Select-String ":$apiPort " | ForEach-Object { $_ -replace '.*\s+(\d+)$', '$1' } | Where-Object { $_ -ne '0' } | Select-Object -Unique
foreach ($foundPid in $foundPids) {
    try {
        $proc = Get-Process -Id $foundPid -ErrorAction Stop
        Log "Matando $($proc.Name) PID $foundPid (puerto $apiPort)..."
        Stop-Process -Id $foundPid -Force -ErrorAction SilentlyContinue
        taskkill /F /PID $foundPid 2>$null
        Start-Sleep -Seconds 1
    } catch { Log "No se pudo matar PID $foundPid (puede que ya no exista)" }
}

# 2. Iniciar servicios via PM2 (API + Web + WhatsApp)
Log "Iniciando servicios con PM2..."
& cmd /c "pm2 resurrect 2>nul"
$pm2Ok = $LASTEXITCODE -eq 0
if (!$pm2Ok) {
    Log "Resurrect falló, iniciando desde ecosystem.config.js..."
    & cmd /c "pm2 start $ecosystemFile 2>nul"
} else {
    Log "Resurrect OK"
}

# 3. Iniciar aplicación de escritorio (Presentation)
if (Test-Path $presentationExe) {
    $presRunning = Get-Process -Name "SistemaVentas.Presentation" -ErrorAction SilentlyContinue
    if (!$presRunning) {
        Log "Iniciando aplicación de escritorio..."
        Start-Process -FilePath $presentationExe -WorkingDirectory "C:\SistemaVentas\SistemaVentas.Presentation\bin\Debug\net8.0-windows"
        Log "Aplicación de escritorio iniciada"
    } else { Log "Aplicación de escritorio ya estaba corriendo" }
}

# 4. Esperar a que la API responda (max 45s con reintentos)
Log "Esperando API en http://localhost:$apiPort/api/productos ..."
$apiOk = $false
for ($i = 0; $i -lt 20; $i++) {
    Start-Sleep -Seconds 2
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:$apiPort/api/productos" -UseBasicParsing -TimeoutSec 4 -ErrorAction Stop
        if ($r.StatusCode -eq 200) { $apiOk = $true; break }
    } catch {
        if ($i % 5 -eq 0) { Log "Esperando API... intento $($i+1)/20" }
    }
}
if ($apiOk) { Log "API respondiendo OK" } else { Log "ERROR: API no respondió tras 45s" }

# 5. Guardar estado de PM2 para próximo reinicio
Log "Guardando estado de PM2..."
& cmd /c "pm2 save 2>nul"

# 6. Abrir navegador (solo si no es inicio automático)
if (!$NoBrowser -and $apiOk) {
    Start-Sleep -Seconds 2
    Start-Process "http://localhost:$webPort"
    Log "Navegador abierto en http://localhost:$webPort"
}

# Limpiar lock
Remove-Item $lockFile -Force -ErrorAction SilentlyContinue
Log "=== Inicio completado ==="
