param([Parameter(Mandatory=$true)][string]$BeatSaberDir)
$ErrorActionPreference="Stop"
$root=Resolve-Path(Join-Path $PSScriptRoot "..")
$detector=Join-Path $PSScriptRoot "detect-beatsaber-version.ps1"
$version=& $detector -BeatSaberDir $BeatSaberDir
if(-not $version){throw "Version detection failed."}

$profiles=Get-Content(Join-Path $root "config\beatsaber-profiles.json")|ConvertFrom-Json
$profile=$profiles.profiles|Where-Object{$_.gameVersion -eq $version}|Select-Object -First 1

if(-not $profile){
  Write-Host "Detected Beat Saber $version"
  Write-Host "NEXUS does not have a validated adapter profile for this version."
  Write-Host "No plugin files were changed."
  exit 2
}

Write-Host "Detected: Beat Saber $version"
Write-Host "Profile:  $($profile.label)"
& (Join-Path $PSScriptRoot "build-profile.ps1") -Profile $profile.id -BeatSaberDir $BeatSaberDir
if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
