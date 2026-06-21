$log = "C:\SistemaVentas\tasklist.log"
Get-ScheduledTask -TaskName "SistemaVentas*" | Format-Table TaskName, State, TaskPath | Out-File $log
Get-Process -Name "SistemaVentas.WebAPI","node" -ErrorAction SilentlyContinue | Select-Object Id, ProcessName | Format-Table -AutoSize | Out-File $log -Append
try { $r = Invoke-WebRequest -Uri "http://localhost:3000" -UseBasicParsing -TimeoutSec 5; "WEB: $($r.StatusCode)" | Out-File $log -Append } catch { "WEB ERROR: $($_)" | Out-File $log -Append }
try { $r = Invoke-WebRequest -Uri "http://localhost:5062/api/configuracion/public" -UseBasicParsing -TimeoutSec 5; "API: $($r.StatusCode)" | Out-File $log -Append } catch { "API ERROR: $($_)" | Out-File $log -Append }
