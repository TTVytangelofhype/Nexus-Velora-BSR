param([string]$BeatSaberDir = $env:BEAT_SABER_DIR)

if (-not $BeatSaberDir) {
  $candidates=@(
    "${env:ProgramFiles(x86)}\Steam\steamapps\common\Beat Saber",
    "$env:ProgramFiles\Steam\steamapps\common\Beat Saber",
    "C:\Program Files\Oculus\Software\Software\hyperbolic-magnetism-beat-saber"
  )
  $BeatSaberDir=$candidates|Where-Object{Test-Path(Join-Path $_ "Beat Saber_Data")}|Select-Object -First 1
}

Write-Host "=== NEXUS Velora BSR prerequisite check ==="
if(-not $BeatSaberDir){Write-Host "[FAIL] Beat Saber not detected";exit 1}
$BeatSaberDir=(Resolve-Path $BeatSaberDir).Path
Write-Host "[ OK ] Beat Saber: $BeatSaberDir"

$ipaDll=@(
  (Join-Path $BeatSaberDir "Beat Saber_Data\Managed\IPA.dll"),
  (Join-Path $BeatSaberDir "Libs\IPA.dll")
)|Where-Object{Test-Path $_}|Select-Object -First 1
$ipaExe=Join-Path $BeatSaberDir "IPA.exe"
$ipaFolder=Join-Path $BeatSaberDir "IPA"

if($ipaDll){
  Write-Host "[ OK ] BSIPA library: $ipaDll"
}elseif((Test-Path $ipaExe) -and (Test-Path $ipaFolder)){
  Write-Host "[ OK ] BSIPA installation detected: $ipaExe"
  Write-Host "[INFO] No standalone IPA.dll found at the legacy build-reference paths."
}else{
  Write-Host "[FAIL] BSIPA not detected"
}

$songActive=Join-Path $BeatSaberDir "Plugins\SongCore.dll"
$songPending=Join-Path $BeatSaberDir "IPA\Pending\Plugins\SongCore.dll"
if(Test-Path $songActive){
  Write-Host "[ OK ] SongCore active: $songActive"
}elseif(Test-Path $songPending){
  Write-Host "[WAIT] SongCore is pending: $songPending"
  Write-Host "       Launch Beat Saber once, reach the main menu, then close it and run this check again."
}else{
  Write-Host "[FAIL] SongCore.dll not found (active or BSIPA pending)"
}

$dotnetOk=$false
try{$v=& dotnet --version;$dotnetOk=$true;Write-Host "[ OK ] .NET SDK: $v"}catch{Write-Host "[FAIL] .NET SDK not found"}

if(-not $dotnetOk){exit 1}
if(-not $ipaDll -and -not((Test-Path $ipaExe) -and (Test-Path $ipaFolder))){exit 1}
if(Test-Path $songPending -and -not(Test-Path $songActive)){exit 2}
if(-not(Test-Path $songActive)){exit 1}

Write-Host "[READY] Beat Saber prerequisites are active."
exit 0
