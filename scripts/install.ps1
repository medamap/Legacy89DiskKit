param(
    [string]$SourcePath,
    [string]$InstallRoot = "$env:LocalAppData\Programs\Legacy89DiskKit",
    [string]$Configuration = "Release",
    [string]$Rid = "win-x64"
)

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$TargetScript = Join-Path $ScriptDir "install-cli.ps1"

& $TargetScript -SourcePath $SourcePath -InstallRoot $InstallRoot -Configuration $Configuration -Rid $Rid
exit $LASTEXITCODE
