$log = "C:\SistemaVentas\tienda-web\startup.log"
$dir = "C:\SistemaVentas\tienda-web"

if (Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -match "next" }) {
    "$(Get-Date) - Next.js ya esta corriendo" | Out-File $log -Append
    exit
}

# Limpiar build cache para evitar rutas 404 por compilaciones anteriores
$nextDir = "$dir\.next"
if (Test-Path $nextDir) {
    Remove-Item -Path $nextDir -Recurse -Force -ErrorAction SilentlyContinue
    "$(Get-Date) - Build cache (.next) limpiado" | Out-File $log -Append
}

Start-Process -FilePath "npm" -ArgumentList "run dev" -WorkingDirectory $dir -WindowStyle Hidden -NoNewWindow
"$(Get-Date) - Next.js iniciado" | Out-File $log -Append
