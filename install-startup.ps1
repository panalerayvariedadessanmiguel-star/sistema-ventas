$startup = [Environment]::GetFolderPath('Startup')
$wsh = New-Object -ComObject WScript.Shell
$shortcut = $wsh.CreateShortcut("$startup\SistemaVentas Servicios.lnk")
$shortcut.TargetPath = "wscript.exe"
$shortcut.Arguments = "C:\SistemaVentas\start-servicios.vbs"
$shortcut.WorkingDirectory = "C:\SistemaVentas"
$shortcut.WindowStyle = 7
$shortcut.Description = "Inicia API y WhatsApp notification service al iniciar sesion"
$shortcut.Save()

Write-Output "Acceso directo creado exitosamente en:"
Write-Output $startup
Write-Output ""
Write-Output "Los servicios iniciaran automaticamente al next inicio de sesion."
Write-Output "Para iniciarlos ahora, ejecute:"
Write-Output "  wscript.exe C:\SistemaVentas\start-servicios.vbs"
