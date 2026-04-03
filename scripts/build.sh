#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
CLI_PROJECT="$REPO_ROOT/CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj"
TEST_PROJECT="$REPO_ROOT/CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj"
CPP_SOURCE="$REPO_ROOT/Cpp"
CPP_BUILD_ROOT="${LEGACY89_CPP_BUILD_ROOT:-/tmp/legacy89-cpp-build}"
CPP_BUILD_DIR="$CPP_BUILD_ROOT/Legacy89DiskKit.Cpp"

echo "[1/5] Building managed CLI"
dotnet build "$CLI_PROJECT" /p:UseAppHost=false

echo "[2/5] Running managed tests"
dotnet test "$TEST_PROJECT" /p:UseAppHost=false

if ! command -v cmake >/dev/null 2>&1; then
  echo "[3/5] Native build skipped: cmake was not found."
  echo "        C# is currently ahead of C++ integration."
  exit 0
fi

echo "[3/5] Configuring native build"
cmake -S "$CPP_SOURCE" -B "$CPP_BUILD_ROOT"

echo "[4/5] Building native library"
cmake --build "$CPP_BUILD_ROOT"

echo "[5/5] Running native tests and validation bridge"
ctest --test-dir "$CPP_BUILD_DIR" --output-on-failure
dotnet test "$TEST_PROJECT" /p:UseAppHost=false --filter "ManagedToNativeValidationTest"

echo "Build and test workflow completed."
