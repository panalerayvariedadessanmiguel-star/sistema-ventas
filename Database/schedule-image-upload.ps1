$taskName = "SubirImagenesRender"
$scriptPath = "C:\SistemaVentas\Database\upload-images-to-render.ps1"

# Delete existing if any
schtasks /DELETE /TN $taskName /F 2>$null

# Create task using schtasks.exe
$xml = @"
<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <RegistrationInfo>
    <Description>Re-upload product images to Render API every 30 mins</Description>
  </RegistrationInfo>
  <Triggers>
    <CalendarTrigger>
      <StartBoundary>2026-06-21T18:00:00</StartBoundary>
      <Repetition>
        <Interval>PT30M</Interval>
        <Duration>P365D</Duration>
        <StopAtDurationEnd>false</StopAtDurationEnd>
      </Repetition>
      <Enabled>true</Enabled>
    </CalendarTrigger>
  </Triggers>
  <Principals>
    <Principal id="Author">
      <RunLevel>HighestAvailable</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <Enabled>true</Enabled>
    <AllowStartOnDemand>true</AllowStartOnDemand>
    <StartWhenAvailable>true</StartWhenAvailable>
  </Settings>
  <Actions Context="Author">
    <Exec>
      <Command>C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe</Command>
      <Arguments>-ExecutionPolicy Bypass -NoProfile -File "$scriptPath"</Arguments>
    </Exec>
  </Actions>
</Task>
"@

$xmlFile = "$env:TEMP\scheduled_task.xml"
Set-Content -Path $xmlFile -Value $xml -Encoding Unicode
schtasks /CREATE /TN $taskName /XML $xmlFile /F
Remove-Item $xmlFile -Force

Write-Host "Task '$taskName' created - runs every 30 min" -ForegroundColor Green
