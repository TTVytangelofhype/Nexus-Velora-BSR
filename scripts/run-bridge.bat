@echo off
title NEXUS Velora BSR
cd /d "%~dp0.."
echo Starting NEXUS Velora BSR...
dotnet run --project src\NexusVeloraBSR.Bridge\NexusVeloraBSR.Bridge.csproj
pause
