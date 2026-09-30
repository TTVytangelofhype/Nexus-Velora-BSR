param([string]$BeatSaberDir = $env:BEAT_SABER_DIR)
if (-not $BeatSaberDir) {
  $candidates=@("${env:ProgramFiles(x86)}\Steam\steamapps\common\Beat Saber","$env:ProgramFiles\Steam\steamapps\common\Beat Saber","C:\Program Files\Oculus\Software\Software\hyperbolic-magnetism-beat-saber")
  $BeatSaberDir=$candidates|Where-Object{Test-Path(Join-Path $_ "Beat Saber_Data")}|Select-Object -First 1
}
Write-Host "=== NEXUS Velora BSR prerequisite check ==="
if(-not $BeatSaberDir){Write-Host "[FAIL] Beat Saber not detected";exit 1}
Write-Host "[ OK ] Beat Saber: $BeatSaberDir"
$ipa=@((Join-Path $BeatSaberDir "Beat Saber_Data\Managed\IPA.dll"),(Join-Path $BeatSaberDir "Libs\IPA.dll"))|Where-Object{Test-Path $_}|Select-Object -First 1
if($ipa){Write-Host "[ OK ] BSIPA: $ipa"}else{Write-Host "[FAIL] IPA.dll missing"}
$song=Join-Path $BeatSaberDir "Plugins\SongCore.dll"
if(Test-Path $song){Write-Host "[ OK ] SongCore: $song"}else{Write-Host "[FAIL] SongCore.dll missing"}
try{$v=& dotnet --version;Write-Host "[ OK ] .NET SDK: $v"}catch{Write-Host "[FAIL] .NET SDK not found"}
if(-not $ipa -or -not(Test-Path $song)){exit 1}
