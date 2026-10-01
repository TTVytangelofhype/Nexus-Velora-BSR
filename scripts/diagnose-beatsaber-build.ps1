param([Parameter(Mandatory=$true)][string]$BeatSaberDir)
$ErrorActionPreference="Stop"
if(-not(Test-Path(Join-Path $BeatSaberDir "Beat Saber_Data"))){throw "Not a Beat Saber installation: $BeatSaberDir"}
Write-Host "=== NEXUS Beat Saber build diagnostics ==="
Write-Host "Game: $BeatSaberDir"

$exe=Join-Path $BeatSaberDir "Beat Saber.exe"
if(Test-Path $exe){
  $pv=(Get-Item $exe).VersionInfo.ProductVersion
  Write-Host "Game executable version: $pv"
}

$ipaExe=Join-Path $BeatSaberDir "IPA.exe"
Write-Host ("IPA.exe: " + $(if(Test-Path $ipaExe){"FOUND"}else{"MISSING"}))

$ipaCandidates=Get-ChildItem $BeatSaberDir -Recurse -File -ErrorAction SilentlyContinue |
  Where-Object { $_.Name -in @("IPA.dll","IPA.Loader.dll","IPA.Loader.Updater.dll") } |
  Select-Object -ExpandProperty FullName -Unique
if($ipaCandidates){
  Write-Host "BSIPA assembly candidates:"
  $ipaCandidates|ForEach-Object{Write-Host "  $_"}
}else{Write-Host "BSIPA assembly candidates: NONE"}

$songCandidates=Get-ChildItem $BeatSaberDir -Recurse -Filter "SongCore.dll" -File -ErrorAction SilentlyContinue |
  Select-Object -ExpandProperty FullName -Unique
if($songCandidates){
  Write-Host "SongCore candidates:"
  $songCandidates|ForEach-Object{Write-Host "  $_"}
}else{Write-Host "SongCore candidates: NONE"}

Write-Host "=== End diagnostics ==="
