@echo off
cd /d "C:\SistemaVentas"
call pm2 resurrect
start "" "C:\SistemaVentas\SistemaVentas.Presentation\bin\Debug\net8.0-windows\SistemaVentas.Presentation.exe"
