' Redirigir al nuevo script unificado
Dim WshShell
Set WshShell = CreateObject("WScript.Shell")
WshShell.Run "powershell -NoProfile -ExecutionPolicy Bypass -File C:\SistemaVentas\start-all.ps1", 0, False
