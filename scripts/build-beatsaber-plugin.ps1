param([string]$BeatSaberDir = $env:BEAT_SABER_DIR)
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($BeatSaberDir)) {
    $candidates = @(
        "${env:ProgramFiles(x86)}\Steam\steamapps\common\Beat Saber",
        "$env:ProgramFiles\Steam\steamapps\common\Beat Saber",
        "C:\Program Files\Oculus\Software\Software\hyperbolic-magnetism-beat-saber"
    )
    $BeatSaberDir = $candidates | Where-Object { Test-Path (Join-Path $_ "Beat Saber_Data") } | Select-Object -First 1
}
if (-not $BeatSaberDir) { throw "Beat Saber was not found. Re-run with -BeatSaberDir 'D:\Games\Beat Saber'." }

$ipaCandidates = @(
    (Join-Path $BeatSaberDir "Beat Saber_Data\Managed\IPA.dll"),
    (Join-Path $BeatSaberDir "Libs\IPA.dll")
)
$ipa = $ipaCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
$songCore = Join-Path $BeatSaberDir "Plugins\SongCore.dll"
if (-not $ipa) { throw "IPA.dll not found. Install BSIPA for Beat Saber 1.42.1 first." }
if (-not (Test-Path $songCore)) { throw "SongCore.dll not found in Plugins. Install SongCore first." }

Write-Host "Beat Saber: $BeatSaberDir"
Write-Host "IPA:        $ipa"
Write-Host "SongCore:   $songCore"

$project = Join-Path $PSScriptRoot "..\src\NexusVeloraBSR.BeatSaber\NexusVeloraBSR.BeatSaber.csproj"
dotnet build $project -c Release -p:BeatSaberDir="$BeatSaberDir" -p:IpaDll="$ipa"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$dll = Join-Path $PSScriptRoot "..\src\NexusVeloraBSR.BeatSaber\bin\Release\net472\NexusVeloraBSR.BeatSaber.dll"
$target = Join-Path $BeatSaberDir "Plugins\NexusVeloraBSR.BeatSaber.dll"
Copy-Item $dll $target -Force
Write-Host ""
Write-Host "NEXUS Velora BSR plugin installed:"
Write-Host $target
