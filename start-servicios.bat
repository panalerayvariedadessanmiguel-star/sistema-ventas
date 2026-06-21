@echo off
setlocal enabledelayedexpansion
set ASPNETCORE_URLS=http://localhost:5062
set API_PORT=5062
set WPP_PORT=3007
set WEB_PORT=3000

echo ========================
echo 1. API (ASP.NET Core)
echo ========================
tasklist /FI "IMAGENAME eq SistemaVentas.WebAPI.exe" 2>NUL | find /I /N "SistemaVentas.WebAPI.exe" >NUL
if "%ERRORLEVEL%"=="0" (
    echo API ya esta corriendo
) else (
    echo Iniciando API...
    start /B "" "C:\SistemaVentas\api-publish\SistemaVentas.WebAPI.exe"
)

echo Esperando a que la API responda en localhost:%API_PORT%...
set RETRIES=0
:wait_api
ping -n 2 127.0.0.1 >nul
curl -s http://localhost:%API_PORT%/api/productos >nul 2>&1
if errorlevel 1 (
    set /a RETRIES+=1
    if !RETRIES! LSS 15 goto wait_api
    echo ADVERTENCIA: API no respondio tras ~30s, continuando...
) else (
    echo API respondiendo correctamente.
)

echo ========================
echo 2. WhatsApp sidecar
echo ========================
netstat -ano | find ":%WPP_PORT% " >NUL
if "%ERRORLEVEL%"=="0" (
    echo WhatsApp ya esta corriendo
) else (
    echo Iniciando WhatsApp service...
    start /B "" cmd.exe /c "cd /d C:\SistemaVentas\whatsapp-service && node server.js"
)

ping -n 3 127.0.0.1 >nul

echo ========================
echo 3. Web frontend (Next.js)
echo ========================
netstat -ano | find ":%WEB_PORT% " >NUL
if "%ERRORLEVEL%"=="0" (
    echo Web frontend ya esta corriendo
) else (
    echo Iniciando Next.js...
    start /B "" cmd.exe /c "cd /d C:\SistemaVentas\tienda-web && npm run dev"
)

ping -n 15 127.0.0.1 >nul

echo ========================
echo 4. Abriendo navegador
echo ========================
start "" "http://localhost:%WEB_PORT%"
