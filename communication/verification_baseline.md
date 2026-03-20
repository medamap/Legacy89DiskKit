# Verification Baseline

## Core Managed Test Suite

```bash
dotnet test csharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false
```

## Native And C++ Baseline

```bash
cmake -S Cpp -B /tmp/legacy89-cpp-build
cmake --build /tmp/legacy89-cpp-build
ctest --test-dir /tmp/legacy89-cpp-build/Legacy89DiskKit.Cpp --output-on-failure
```

Current note:

- C# tests are currently green
- C++ has a known failure in `Legacy89DiskKitCppNativeBridgeExportsSmoke`

## Standalone CLI Goal

Use a self-contained single-file publish, not `dotnet run`.

Reference:

- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Release_Process.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/scripts/release-cli.sh`

## Current Acceptance For This Work

- 2D only
- create a new 2D D88
- copy boot area
- copy required files
- reopen and verify result
- prefer evidence from the published standalone binary
