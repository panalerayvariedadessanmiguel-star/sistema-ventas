$log = "C:\SistemaVentas\verify-tasks.log"
"=== $(Get-Date) ===" | Out-File $log
Get-ScheduledTask -TaskName "SistemaVentas*" | Format-List TaskName,State,Description | Out-File $log -Append
$info = Get-ScheduledTask -TaskName "SistemaVentas API" | Get-ScheduledTaskInfo
"LastRun: $($info.LastRunTime)" | Out-File $log -Append
"LastResult: $($info.LastTaskResult)" | Out-File $log -Append
"NextRun: $($info.NextRunTime)" | Out-File $log -Append
"TaskPath: $((Get-ScheduledTask -TaskName 'SistemaVentas API').TaskPath)" | Out-File $log -Append
Get-ScheduledTask -TaskName "SistemaVentas Web" | Get-ScheduledTaskInfo | Format-List TaskName,LastRunTime,LastTaskResult,NextRunTime | Out-File $log -Append
"Done" | Out-File $log -Append
