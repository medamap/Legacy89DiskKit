param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

if ($Version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+$') {
    throw "Version must be semantic version without leading v, for example: 2.0.0"
}

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$ProjectPath = Join-Path $RepoRoot "CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj"
$TestProjectPath = Join-Path $RepoRoot "CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj"
$ReleaseNotesPath = Join-Path $RepoRoot "RELEASE_NOTES_v$Version.md"
$PublishRoot = Join-Path $RepoRoot "publish/v$Version"
$ReleaseRoot = Join-Path $RepoRoot "release/v$Version"
$SampleImage = $env:LEGACY89_SAMPLE_IMAGE
$Rids = @("win-x64", "linux-x64", "osx-x64", "osx-arm64")

if (-not (Test-Path $ProjectPath)) { throw "CLI project not found: $ProjectPath" }
if (-not (Test-Path $TestProjectPath)) { throw "Test project not found: $TestProjectPath" }
if (-not (Test-Path $ReleaseNotesPath)) { throw "Release notes not found: $ReleaseNotesPath" }
Remove-Item $PublishRoot -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $ReleaseRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $PublishRoot | Out-Null
New-Item -ItemType Directory -Path $ReleaseRoot | Out-Null

dotnet test $TestProjectPath /p:UseAppHost=false

foreach ($Rid in $Rids) {
    $OutputDir = Join-Path $PublishRoot $Rid
    New-Item -ItemType Directory -Path $OutputDir | Out-Null

    dotnet publish $ProjectPath `
        -c Release `
        -r $Rid `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishAot=false `
        -o $OutputDir

    $ExecutablePath = if ($Rid -eq "win-x64") {
        Join-Path $OutputDir "Legacy89DiskKit.Cli.exe"
    } else {
        Join-Path $OutputDir "Legacy89DiskKit.Cli"
    }

    if (-not (Test-Path $ExecutablePath)) {
        throw "Expected executable not found for $Rid: $ExecutablePath"
    }
}

$HostArtifact = Join-Path (Join-Path $PublishRoot "win-x64") "Legacy89DiskKit.Cli.exe"
& $HostArtifact --help | Out-Null
& $HostArtifact disk --help | Out-Null
& $HostArtifact list --help | Out-Null
if ($SampleImage) {
    if (-not (Test-Path $SampleImage)) { throw "Sample image not found: $SampleImage" }
    & $HostArtifact list $SampleImage -e sjis | Out-Null
}

foreach ($Rid in $Rids) {
    $ArchivePath = Join-Path $ReleaseRoot "Legacy89DiskKit.Cli-v$Version-$Rid.zip"
    Compress-Archive -Path (Join-Path $PublishRoot $Rid) -DestinationPath $ArchivePath -Force
}

Get-ChildItem -File $ReleaseRoot | Sort-Object FullName | Select-Object -ExpandProperty FullName
