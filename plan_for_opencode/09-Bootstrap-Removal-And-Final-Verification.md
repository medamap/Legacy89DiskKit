# Bootstrap Removal And Final Verification

## Goal

Delete the compatibility bootstrap project from the active tree once all callers have been migrated.

## Tasks

1. Remove `Legacy89DiskKit.Application.csproj` from active solution usage.
2. Remove or retire `Legacy89DiskKitApplication.cs`.
3. If historical retention is desired, move the old project into `obsolete/` rather than keeping it active.
4. Re-run full verification after removal.
5. Re-run test image regeneration after the final source tree is stable.

## Required Final Checks

```bash
rg -n '^namespace Legacy89DiskKit\.(Application|Domain|Infrastructure)(\.|;)' CSharp -g '*.cs'
rg -n 'using Legacy89DiskKit\.(Application|Domain|Infrastructure)(\.|;)|Legacy89DiskKit\.(Application|Domain|Infrastructure)\.' CSharp -g '*.cs' -g '*.csproj' -g '*.sln'
dotnet build CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj
dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter "FullyQualifiedName!~ManagedToNativeValidationTest&FullyQualifiedName!~CliCheckUpdateTest"
dotnet run --project IgnoreTools/TestImageFactory/TestImageFactory.csproj /p:UseAppHost=false -- IgnoreTools/TestImageFactory/manifest.local.json
```

## Completion Rule

This bootstrap removal task is complete only when:

- no active `Legacy89DiskKit.Application` namespace declarations remain under `CSharp/`
- no active source, project, or solution file depends on `Legacy89DiskKit.Application`
- build succeeds
- filtered tests succeed
- `images/test` regeneration succeeds
