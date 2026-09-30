@echo off
title NEXUS Velora BSR - Beat Saber Plugin Installer
cd /d "%~dp0.."
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-beatsaber-plugin.ps1"
echo.
pause
