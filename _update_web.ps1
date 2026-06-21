$log = "C:\SistemaVentas\update-web-task.log"
"=== $(Get-Date) ===" | Out-File $log
$action = New-ScheduledTaskAction -Execute "C:\Program Files\nodejs\node.exe" -Argument "C:\SistemaVentas\tienda-web\node_modules\next\dist\bin\next start" -WorkingDirectory "C:\SistemaVentas\tienda-web"
$trigger = New-ScheduledTaskTrigger -AtLogOn
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
$principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel Limited
Register-ScheduledTask -TaskName "SistemaVentas Web" -Action $action -Trigger $trigger -Settings $settings -Principal $principal -Description "Frontend Next.js tienda web (produccion)" -Force
"Web task updated to production mode" | Out-File $log -Append
Get-ScheduledTask -TaskName "SistemaVentas*" | Format-Table TaskName,State,Description | Out-File $log -Append
"Done" | Out-File $log -Append
