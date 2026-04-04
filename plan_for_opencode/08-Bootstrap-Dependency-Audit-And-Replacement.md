# Bootstrap Dependency Audit And Replacement

## Goal

Remove all remaining real dependencies on the compatibility bootstrap project before deleting it.

## Scope

- `CSharp/Legacy89DiskKit.Application/Legacy89DiskKitApplication.cs`
- `CSharp/Legacy89DiskKit.Application/Legacy89DiskKit.Application.csproj`
- any active caller that still references `Legacy89DiskKitApplication`
- any test or helper that still depends on the old bootstrap assembly identity

## Tasks

1. Inventory all remaining bootstrap usages.
2. Replace each usage with direct responsibility-first references.
3. Replace any string-based identity checks that intentionally point at `Legacy89DiskKit.Application` if they are no longer required.
4. Keep the workspace buildable at milestone boundaries, but intermediate breakage is acceptable if the replacement plan is coherent.

## Required Searches

```bash
rg -n 'Legacy89DiskKitApplication|Legacy89DiskKit\.Application' CSharp -g '*.cs' -g '*.csproj' -g '*.sln'
```

## Expected End State

- active source no longer calls `Legacy89DiskKitApplication`
- active projects no longer reference `Legacy89DiskKit.Application.csproj`
- any remaining mention of `Legacy89DiskKit.Application` is either obsolete material or an intentionally updated test expectation that still has a documented reason

## Verification

```bash
rg -n 'Legacy89DiskKitApplication|Legacy89DiskKit\.Application' CSharp -g '*.cs' -g '*.csproj' -g '*.sln'
dotnet build CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj
```
