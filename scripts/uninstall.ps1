$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$TargetScript = Join-Path $ScriptDir "install-cli.ps1"

& $TargetScript -Uninstall
exit $LASTEXITCODE
