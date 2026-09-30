@echo off
title NEXUS Velora BSR - Multi-Version Installer
set /p BS_DIR=Paste your Beat Saber installation folder: 
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install-profile.ps1" -BeatSaberDir "%BS_DIR%"
echo.
pause
