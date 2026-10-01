param(
  [ValidateSet("stable-1.42.1","latest")][string]$Profile = "stable-1.42.1",
  [Parameter(Mandatory=$true)][string]$BeatSaberDir,
  [switch]$Install
)
$ErrorActionPreference="Stop"
$root=Resolve-Path(Join-Path $PSScriptRoot "..")
$profiles=Get-Content(Join-Path $root "config\beatsaber-profiles.json")|ConvertFrom-Json
$p=$profiles.profiles|Where-Object{$_.id -eq $Profile}
if(-not $p){throw "Unknown profile: $Profile"}
if(-not(Test-Path(Join-Path $BeatSaberDir "Beat Saber_Data"))){throw "Not a Beat Saber installation: $BeatSaberDir"}

$ipaCandidates=@(
  (Join-Path $BeatSaberDir "Beat Saber_Data\Managed\IPA.Loader.dll"),
  (Join-Path $BeatSaberDir "IPA\Data\Managed\IPA.Loader.dll"),
  (Join-Path $BeatSaberDir "Beat Saber_Data\Managed\IPA.dll"),
  (Join-Path $BeatSaberDir "Libs\IPA.dll")
)
$ipa=$ipaCandidates|Where-Object{Test-Path $_}|Select-Object -First 1
$songCore=Join-Path $BeatSaberDir "Plugins\SongCore.dll"
if(-not $ipa){throw "No compatible BSIPA loader assembly found."}
if(-not(Test-Path $songCore)){throw "SongCore.dll is not active in Plugins."}

Write-Host "=== NEXUS adapter build ==="
Write-Host "Profile:  $($p.label)"
Write-Host "BSIPA:    $ipa"
Write-Host "SongCore: $songCore"

$out=Join-Path $root ("dist\plugins\"+$Profile)
New-Item -ItemType Directory -Path $out -Force|Out-Null
$project=Join-Path $root "src\NexusVeloraBSR.BeatSaber\NexusVeloraBSR.BeatSaber.csproj"
dotnet build $project -c Release -p:BeatSaberDir="$BeatSaberDir" -p:IpaDll="$ipa" -p:NexusGameVersion="$($p.gameVersion)"
if($LASTEXITCODE -ne 0){Write-Host "[FAIL] Adapter did not compile. Beat Saber was not changed.";exit $LASTEXITCODE}

$built=Join-Path $root "src\NexusVeloraBSR.BeatSaber\bin\Release\net472\NexusVeloraBSR.BeatSaber.dll"
if(-not(Test-Path $built)){throw "Build completed but adapter DLL was not found."}
$versioned=Join-Path $out ("NexusVeloraBSR.BeatSaber-"+$p.gameVersion+".dll")
Copy-Item $built $versioned -Force
Write-Host "[ OK ] Build archive: $versioned"

if($Install){
  $target=Join-Path $BeatSaberDir "Plugins\NexusVeloraBSR.BeatSaber.dll"
  Copy-Item $built $target -Force
  Write-Host "[ OK ] Installed: $target"
}else{
  Write-Host "[SAFE] Build-only mode. Beat Saber Plugins was not modified."
  Write-Host "       Re-run with -Install only after this build succeeds."
}
