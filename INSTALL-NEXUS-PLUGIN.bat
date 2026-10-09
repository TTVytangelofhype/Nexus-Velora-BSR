@echo off
setlocal
title NEXUS BSR - Install Beat Saber Plugin
set "ROOT=%~dp0"
echo.
echo Select Beat Saber version:
echo   1 - 1.40.8 (BSManager)
echo   2 - 1.44.1 (BSManager)
choice /c 12 /n /m "Choose 1 or 2: "
if errorlevel 2 (
 set "VERSION=1.44.1"
 set "PROFILE=stable-1.44.1"
) else (
 set "VERSION=1.40.8"
 set "PROFILE=stable-1.40.8"
)
set "GAME=%USERPROFILE%\BSManager\BSInstances\%VERSION%"
if not exist "%GAME%\Beat Saber_Data" (
 echo Could not find "%GAME%".
 echo Install that version through BSManager first.
 pause
 exit /b 1
)
echo.
echo Building for %VERSION% from installed game assemblies...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%ROOT%scripts\build-profile.ps1" -Profile "%PROFILE%" -BeatSaberDir "%GAME%" -Install
if errorlevel 1 (
 echo Build failed. Existing plugin was not replaced.
 pause
 exit /b 1
)
echo.
echo Plugin installed for %VERSION%.
echo Start the bridge with START-NEXUS-BSR.bat and launch the game from BSManager.
pause
