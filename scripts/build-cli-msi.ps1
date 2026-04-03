param(
    [Parameter(Mandatory = $true)]
    [string]$Version,
    [string]$SourceDir,
    [string]$OutputDir
)

$ErrorActionPreference = "Stop"

if ($Version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+$') {
    throw "Version must be semantic version without leading v, for example: 2.1.0"
}

if (-not (Get-Command wix -ErrorAction SilentlyContinue)) {
    throw "WiX v4 CLI ('wix') is required to build the MSI."
}

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$PackagingRoot = Join-Path $RepoRoot "packaging/windows-msi"
$SourceDir = if ($SourceDir) { $SourceDir } else { Join-Path $RepoRoot "publish/v$Version/win-x64" }
$OutputDir = if ($OutputDir) { $OutputDir } else { Join-Path $RepoRoot "release/v$Version" }
$StageDir = Join-Path $PackagingRoot "stage"
$WxsPath = Join-Path $PackagingRoot "Product.wxs"
$SourceExe = Join-Path $SourceDir "Legacy89DiskKit.Cli.exe"
$AliasCmd = Join-Path $StageDir "l89.cmd"
$MsiPath = Join-Path $OutputDir "Legacy89DiskKit.Cli-v$Version-win-x64.msi"

if (-not (Test-Path $SourceExe)) {
    throw "Published Windows executable not found: $SourceExe"
}

New-Item -ItemType Directory -Path $StageDir -Force | Out-Null
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

Copy-Item $SourceExe (Join-Path $StageDir "Legacy89DiskKit.Cli.exe") -Force
[System.IO.File]::WriteAllText(
    $AliasCmd,
    "@echo off`r`n`"%~dp0Legacy89DiskKit.Cli.exe`" %*`r`n",
    [System.Text.Encoding]::ASCII)

& wix build $WxsPath `
    -define Version=$Version `
    -define SourceDir=$StageDir `
    -out $MsiPath

Write-Host $MsiPath
