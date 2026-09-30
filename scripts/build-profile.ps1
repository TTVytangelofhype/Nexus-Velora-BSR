param(
  [ValidateSet("stable-1.42.1","latest")][string]$Profile = "stable-1.42.1",
  [Parameter(Mandatory=$true)][string]$BeatSaberDir
)
$ErrorActionPreference="Stop"
$root=Resolve-Path(Join-Path $PSScriptRoot "..")
$profiles=Get-Content(Join-Path $root "config\beatsaber-profiles.json")|ConvertFrom-Json
$p=$profiles.profiles|Where-Object{$_.id -eq $Profile}
if(-not $p){throw "Unknown profile: $Profile"}
if(-not(Test-Path(Join-Path $BeatSaberDir "Beat Saber_Data"))){throw "Not a Beat Saber installation: $BeatSaberDir"}

$ipa=@((Join-Path $BeatSaberDir "Beat Saber_Data\Managed\IPA.dll"),(Join-Path $BeatSaberDir "Libs\IPA.dll"))|Where-Object{Test-Path $_}|Select-Object -First 1
$songCore=Join-Path $BeatSaberDir "Plugins\SongCore.dll"
if(-not $ipa){throw "BSIPA/IPA.dll missing from selected $Profile installation."}
if(-not(Test-Path $songCore)){throw "SongCore.dll missing from selected $Profile installation."}

$out=Join-Path $root ("dist\plugins\"+$Profile)
New-Item -ItemType Directory -Path $out -Force|Out-Null
$project=Join-Path $root "src\NexusVeloraBSR.BeatSaber\NexusVeloraBSR.BeatSaber.csproj"
dotnet build $project -c Release -p:BeatSaberDir="$BeatSaberDir" -p:IpaDll="$ipa" -p:NexusGameVersion="$($p.gameVersion)"
if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}

$built=Join-Path $root "src\NexusVeloraBSR.BeatSaber\bin\Release\net472\NexusVeloraBSR.BeatSaber.dll"
$versioned=Join-Path $out ("NexusVeloraBSR.BeatSaber-"+$p.gameVersion+".dll")
Copy-Item $built $versioned -Force
Copy-Item $built (Join-Path $BeatSaberDir "Plugins\NexusVeloraBSR.BeatSaber.dll") -Force
Write-Host "Built profile: $($p.label)"
Write-Host "Archive: $versioned"
Write-Host "Installed into: $BeatSaberDir\Plugins"
