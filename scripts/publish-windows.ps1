param([string]$Runtime = "win-x64")
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$project = Join-Path $root "src\NexusVeloraBSR.Bridge\NexusVeloraBSR.Bridge.csproj"
$out = Join-Path $root "dist\NexusVeloraBSR"

if (Test-Path $out) { Remove-Item $out -Recurse -Force }
New-Item -ItemType Directory -Path $out -Force | Out-Null

dotnet publish $project -c Release -r $Runtime --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $out
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Copy-Item (Join-Path $root "config\nexus-velora-bsr.example.json") (Join-Path $out "nexus-velora-bsr.example.json") -Force
Write-Host ""
Write-Host "Standalone NEXUS bridge published to:"
Write-Host $out
Write-Host "Run NexusVeloraBSR.exe to start the bridge."
