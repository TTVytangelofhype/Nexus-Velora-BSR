param([Parameter(Mandatory=$true)][string]$BeatSaberDir)
$ErrorActionPreference="Stop"
if(-not(Test-Path(Join-Path $BeatSaberDir "Beat Saber_Data"))){throw "Not a Beat Saber installation: $BeatSaberDir"}

$version=""
$exe=Join-Path $BeatSaberDir "Beat Saber.exe"
if(Test-Path $exe){
  try{$version=(Get-Item $exe).VersionInfo.ProductVersion}catch{}
}
if(-not $version){
  $global=Join-Path $BeatSaberDir "Beat Saber_Data\globalgamemanagers"
  if(Test-Path $global){
    $bytes=[System.IO.File]::ReadAllBytes($global)
    $ascii=[System.Text.Encoding]::ASCII.GetString($bytes)
    $m=[regex]::Match($ascii,'\b1\.\d{1,3}\.\d{1,3}\b')
    if($m.Success){$version=$m.Value}
  }
}
if(-not $version){throw "Could not detect Beat Saber version from this installation."}
$m=[regex]::Match($version,'1\.\d+\.\d+')
if($m.Success){$version=$m.Value}
Write-Output $version
