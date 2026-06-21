@echo off
title Limpiando inicio duplicado - Sistema Ventas

echo Eliminando tarea programada...
schtasks /Delete /TN "SistemaVentas Web" /F

echo Deteniendo y deshabilitando servicio...
sc stop SistemaVentasAPI >nul 2>&1
sc config SistemaVentasAPI start=disabled

echo.
echo === LIMPIEZA COMPLETADA ===
echo Ya no se abriran ventanas extras al iniciar el PC.
echo.
pause
