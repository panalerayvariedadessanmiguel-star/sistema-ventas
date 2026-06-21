# Requiere ejecutarse como Administrador
# Botón derecho > "Ejecutar como Administrador"

$lnkName = "Iniciar Tienda Web"
$webDir = "C:\SistemaVentas\tienda-web"
$startupDir = [Environment]::GetFolderPath("Startup")
$lnkPath = "$startupDir\$lnkName.lnk"

# Crear acceso directo en la carpeta de inicio
$wshell = New-Object -ComObject WScript.Shell
$lnk = $wshell.CreateShortcut($lnkPath)
$lnk.TargetPath = "cmd.exe"
$lnk.Arguments = "/c pm2 resurrect"
$lnk.WorkingDirectory = "C:\SistemaVentas"
$lnk.WindowStyle = 7
$lnk.Description = "Inicia Next.js dev server de la tienda"
$lnk.Save()

Write-Host "Acceso directo creado en: $lnkPath"
Write-Host "Se iniciará automaticamente al iniciar sesión."
