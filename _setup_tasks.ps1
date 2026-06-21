$log = "C:\SistemaVentas\setup-tasks.log"
"=== $(Get-Date) ===" | Out-File $log

# Delete broken service if exists
sc.exe delete SistemaVentasAPI 2>&1 | Out-File $log -Append

# ---- Task 1: API Backend (system startup) ----
$action = New-ScheduledTaskAction -Execute 'C:\SistemaVentas\api-publish\SistemaVentas.WebAPI.exe' -WorkingDirectory 'C:\SistemaVentas\api-publish'
$trigger = New-ScheduledTaskTrigger -AtStartup
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 1)
$principal = New-ScheduledTaskPrincipal -UserId 'SYSTEM' -LogonType ServiceAccount -RunLevel Highest
Register-ScheduledTask -TaskName 'SistemaVentas API' -Action $action -Trigger $trigger -Settings $settings -Principal $principal -Description 'API Backend Sistema de Ventas' -Force
"API task created" | Out-File $log -Append

# ---- Task 2: Web Frontend (user login) ----
$node = (Get-Command node).Source
$action2 = New-ScheduledTaskAction -Execute $node -Argument 'C:\SistemaVentas\tienda-web\node_modules\next\dist\bin\next dev' -WorkingDirectory 'C:\SistemaVentas\tienda-web'
$trigger2 = New-ScheduledTaskTrigger -AtLogOn
$settings2 = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
$principal2 = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel Limited
Register-ScheduledTask -TaskName 'SistemaVentas Web' -Action $action2 -Trigger $trigger2 -Settings $settings2 -Principal $principal2 -Description 'Frontend Next.js tienda web' -Force
"Web task created" | Out-File $log -Append

# Start API now
Start-ScheduledTask -TaskName 'SistemaVentas API'
"API task started" | Out-File $log -Append

Get-ScheduledTask -TaskName "SistemaVentas*" | Format-Table TaskName,State | Out-File $log -Append
"Done" | Out-File $log -Append
