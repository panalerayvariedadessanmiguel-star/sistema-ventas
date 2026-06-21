$log = "C:\SistemaVentas\whatsapp-service\service.log"
$dir = "C:\SistemaVentas\whatsapp-service"
$port = 3007

if (Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -like "*whatsapp-service*" }) {
    "$(Get-Date) - WhatsApp service ya esta corriendo" | Out-File $log -Append
    exit
}

$env:PORT = $port
Start-Process -FilePath "node" -ArgumentList "server.js" -WorkingDirectory $dir -WindowStyle Hidden -RedirectStandardOutput "$dir\stdout.log" -RedirectStandardError "$dir\stderr.log"
"$(Get-Date) - WhatsApp service iniciado en puerto $port" | Out-File $log -Append
