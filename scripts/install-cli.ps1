param(
    [string]$SourcePath,
    [string]$InstallRoot = "$env:LocalAppData\Programs\Legacy89DiskKit",
    [string]$Configuration = "Release",
    [string]$Rid = "win-x64",
    [switch]$Uninstall
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$ProjectPath = Join-Path $RepoRoot "CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj"

function Get-UserPathEntries {
    $current = [Environment]::GetEnvironmentVariable("PATH", "User")
    if ([string]::IsNullOrWhiteSpace($current)) {
        return @()
    }

    return $current.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)
}

function Set-UserPathEntries([string[]]$entries) {
    $normalized = $entries | Select-Object -Unique
    [Environment]::SetEnvironmentVariable("PATH", ($normalized -join ';'), "User")
}

$BinRoot = Join-Path $InstallRoot "bin"
$TargetExe = Join-Path $BinRoot "Legacy89DiskKit.Cli.exe"
$TargetCmd = Join-Path $BinRoot "l89.cmd"

if ($Uninstall) {
    if (Test-Path $InstallRoot) {
        Remove-Item $InstallRoot -Recurse -Force
    }

    $remaining = @(Get-UserPathEntries | Where-Object { $_ -ne $BinRoot })
    Set-UserPathEntries $remaining
    Write-Host "Removed l89 from $InstallRoot"
    exit 0
}

if ([string]::IsNullOrWhiteSpace($SourcePath)) {
    if (-not (Test-Path $ProjectPath)) {
        throw "CLI project not found: $ProjectPath"
    }

    $PublishRoot = Join-Path ([System.IO.Path]::GetTempPath()) "Legacy89DiskKit-install"
    $SourcePath = Join-Path $PublishRoot $Rid

    if (Test-Path $SourcePath) {
        Remove-Item $SourcePath -Recurse -Force
    }

    New-Item -ItemType Directory -Path $SourcePath -Force | Out-Null

    Write-Host "Publishing Legacy89DiskKit CLI for $Rid..."
    dotnet publish $ProjectPath `
        -c $Configuration `
        -r $Rid `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishAot=false `
        -o $SourcePath
}

$SourceExe = if (Test-Path $SourcePath -PathType Container) {
    Join-Path $SourcePath "Legacy89DiskKit.Cli.exe"
} else {
    $SourcePath
}

if (-not (Test-Path $SourceExe -PathType Leaf)) {
    throw "Executable not found: $SourceExe"
}

New-Item -ItemType Directory -Path $BinRoot -Force | Out-Null
if (Test-Path $SourcePath -PathType Container) {
    Copy-Item (Join-Path $SourcePath '*') $BinRoot -Recurse -Force
}
else {
    Copy-Item $SourceExe $TargetExe -Force
}

$CmdBody = "@echo off`r`n`"%~dp0Legacy89DiskKit.Cli.exe`" %*`r`n"
[System.IO.File]::WriteAllText($TargetCmd, $CmdBody, [System.Text.Encoding]::ASCII)

$entries = @(Get-UserPathEntries)
if ($entries -notcontains $BinRoot) {
    $entries += $BinRoot
    Set-UserPathEntries $entries
}

Write-Host "Installed:"
Write-Host "  $TargetExe"
Write-Host "  $TargetCmd"
