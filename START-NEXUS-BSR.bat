@echo off
setlocal
title NEXUS Velora BSR - Start
set "ROOT=%~dp0"
set "BRIDGE=%ROOT%scripts\run-bridge.bat"
if not exist "%BRIDGE%" (
 echo Missing bridge launcher: %BRIDGE%
 pause
 exit /b 1
)
echo NEXUS Velora BSR
echo Starting local bridge in a separate window...
start "NEXUS Velora Bridge" /D "%ROOT%" cmd /k call "%BRIDGE%"
echo.
echo Launch Beat Saber 1.40.8 or 1.44.1 from BSManager as normal.
echo BSIPA loads the installed NEXUS plugin automatically.
echo No PowerShell build is required for ordinary launches.
timeout /t 4 >nul
endlocal
