# Backend Identity String Cleanup

## Goal

Finish the last cleanup needed for a strict 100/100 migration review by removing the remaining legacy `Legacy89DiskKit.Application` identity strings from active source.

## Remaining Known Items

- `CSharp/Legacy89DiskKit/Native/Application/ManagedNativeBridgeBackend.cs`
- `CSharp/Legacy89DiskKit.Tests/NativeBackendIdentityTest.cs`

## Required Work

1. Replace the backend target label returned by `ManagedNativeBridgeBackend` with a responsibility-first or neutral identity string.
2. Update all affected tests accordingly.
3. Re-run the old-namespace search to confirm there are no remaining `Legacy89DiskKit.Application`, `Legacy89DiskKit.Domain`, or `Legacy89DiskKit.Infrastructure` references in active source unless they are outside the active `CSharp/` tree.

## Verification

```bash
rg -n 'Legacy89DiskKit\.(Application|Domain|Infrastructure)' CSharp -g '*.cs' -g '*.csproj' -g '*.sln'
dotnet build CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj
dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter "FullyQualifiedName!~ManagedToNativeValidationTest&FullyQualifiedName!~CliCheckUpdateTest"
```

## Completion Rule

This task is complete only when:
- the search above returns zero hits in active source
- build succeeds
- filtered tests succeed
