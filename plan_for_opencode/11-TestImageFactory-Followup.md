# TestImageFactory Follow-up

## Goal

Restore `IgnoreTools/TestImageFactory` so test-image regeneration works after the responsibility-first namespace and project migration.

## Current Failure

`dotnet run --project IgnoreTools/TestImageFactory/TestImageFactory.csproj /p:UseAppHost=false -- IgnoreTools/TestImageFactory/manifest.local.json`

fails because:

1. `IgnoreTools/TestImageFactory/TestImageFactory.csproj` still references the deleted project:
   - `CSharp/Legacy89DiskKit.Application/Legacy89DiskKit.Application.csproj`
2. `IgnoreTools/TestImageFactory/Program.cs` still imports:
   - `using Legacy89DiskKit.Application;`

## Required Work

### 1. Fix project references

- Open `IgnoreTools/TestImageFactory/TestImageFactory.csproj`
- Remove the dead `Legacy89DiskKit.Application.csproj` reference
- Add direct responsibility-first project references required by the tool

At minimum, inspect actual code usage and wire the needed projects explicitly.

### 2. Fix namespaces and using directives

- Open `IgnoreTools/TestImageFactory/Program.cs`
- Remove old layer-first namespace usage
- Replace all references with responsibility-first namespaces
- Do not leave any `Legacy89DiskKit.Application`, `Legacy89DiskKit.Domain.*`, or `Legacy89DiskKit.Infrastructure.*` dependencies in this tool

### 3. Verify the tool itself

Run:

```bash
dotnet build IgnoreTools/TestImageFactory/TestImageFactory.csproj
dotnet run --project IgnoreTools/TestImageFactory/TestImageFactory.csproj /p:UseAppHost=false -- IgnoreTools/TestImageFactory/manifest.local.json
```

### 4. Verify regeneration output

Confirm that:

- `images/test/_generation_summary.json` is updated
- the generator exits with code `0`
- the cleanup/rebuild flow completes without namespace or project-reference errors

## Completion Rule

This task is complete only when:

- `TestImageFactory.csproj` contains no dead project references
- `Program.cs` contains no old layer-first namespace usage
- the tool builds successfully
- `images/test` regeneration succeeds successfully
