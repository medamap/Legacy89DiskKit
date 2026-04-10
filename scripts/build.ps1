param(
    [string]$CppBuildRoot = "$env:TEMP\legacy89-cpp-build"
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$CliProject = Join-Path $RepoRoot "CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj"
$TestProject = Join-Path $RepoRoot "CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj"
$CppSource = Join-Path $RepoRoot "Cpp"
$CppBuildDir = Join-Path $CppBuildRoot "Legacy89DiskKit.Cpp"

Write-Host "[1/5] Building managed CLI"
dotnet build $CliProject /p:UseAppHost=false

Write-Host "[2/5] Running managed tests"
dotnet test $TestProject /p:UseAppHost=false

if (-not (Get-Command cmake -ErrorAction SilentlyContinue)) {
    Write-Host "[3/5] Native build skipped: cmake was not found."
    Write-Host "        C# is currently ahead of C++ integration."
    exit 0
}

Write-Host "[3/5] Configuring native build"
cmake -S $CppSource -B $CppBuildRoot

Write-Host "[4/5] Building native library"
cmake --build $CppBuildRoot

$NativeBin = $CppBuildDir
$env:PATH = "$NativeBin;$env:PATH"

Write-Host "[5/5] Running native tests and validation bridge"
ctest --test-dir $CppBuildDir --output-on-failure
dotnet test $TestProject /p:UseAppHost=false --filter "ManagedToNativeValidationTest"

Write-Host "Build and test workflow completed."
