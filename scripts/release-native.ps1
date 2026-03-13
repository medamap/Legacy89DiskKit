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
$ProjectPath = Join-Path $RepoRoot "CSharp/Legacy89DiskKit.NativeInterop/Legacy89DiskKit.NativeInterop.csproj"
$TestAppPath = Join-Path $RepoRoot "CSharp/NativeInteropTestApp/NativeInteropTestApp.csproj"
$HeaderPath = Join-Path $RepoRoot "include/legacy89diskkit_native.h"
$ReleaseNotesPath = Join-Path $RepoRoot "RELEASE_NOTES_v$Version.md"
$SampleImage = Join-Path $RepoRoot "images/disk_org/x1/X1turboIIIDemo.d88"
$PublishRoot = Join-Path $RepoRoot "publish/v$Version/native"
$ReleaseRoot = Join-Path $RepoRoot "release/v$Version"
$Rid = "win-x64"
$TargetRoot = Join-Path $PublishRoot $Rid
$BuildRoot = Join-Path $TargetRoot "build"
$IncludeRoot = Join-Path $TargetRoot "include"
$LibRoot = Join-Path $TargetRoot "lib"
$InternalLibPath = Join-Path $BuildRoot "Legacy89DiskKit.NativeInterop.dll"
$PublicLibPath = Join-Path $LibRoot "Legacy89DiskKit.Native.dll"
$ArchivePath = Join-Path $ReleaseRoot "Legacy89DiskKit.Native-v$Version-$Rid.zip"

if (-not (Test-Path $ProjectPath)) { throw "Native project not found: $ProjectPath" }
if (-not (Test-Path $TestAppPath)) { throw "Native smoke test app not found: $TestAppPath" }
if (-not (Test-Path $HeaderPath)) { throw "Native public header not found: $HeaderPath" }
if (-not (Test-Path $ReleaseNotesPath)) { throw "Release notes not found: $ReleaseNotesPath" }
if (-not (Test-Path $SampleImage)) { throw "Sample image not found: $SampleImage" }

Remove-Item $TargetRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $BuildRoot | Out-Null
New-Item -ItemType Directory -Path $IncludeRoot | Out-Null
New-Item -ItemType Directory -Path $LibRoot | Out-Null
New-Item -ItemType Directory -Path $ReleaseRoot | Out-Null

dotnet publish $ProjectPath `
    -c Release `
    -r $Rid `
    -p:PublishAot=true `
    -p:NativeLib=Shared `
    -o $BuildRoot

if (-not (Test-Path $InternalLibPath)) {
    throw "Expected native library not found: $InternalLibPath"
}

Copy-Item $InternalLibPath $PublicLibPath -Force
Copy-Item $HeaderPath (Join-Path $IncludeRoot "legacy89diskkit_native.h") -Force

dotnet run --project $TestAppPath -- $PublicLibPath $SampleImage | Out-Null

Compress-Archive -Path $TargetRoot -DestinationPath $ArchivePath -Force

Get-ChildItem -File $ReleaseRoot | Sort-Object FullName | Select-Object -ExpandProperty FullName
