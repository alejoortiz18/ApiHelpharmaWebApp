@echo off
setlocal

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0publish.ps1" %*

if errorlevel 1 (
    echo.
    echo La publicacion fallo.
    exit /b 1
)

echo.
echo Publicacion finalizada correctamente.
exit /b 0
