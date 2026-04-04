# Final Bootstrap Namespace Cleanup

## Goal

Remove or intentionally finalize the last remaining old bootstrap namespace declaration so the responsibility-first migration can be considered fully complete.

## Current Remaining Item

- `CSharp/Legacy89DiskKit.Application/Legacy89DiskKitApplication.cs`
- current declaration:
  - `namespace Legacy89DiskKit.Application;`

## Required Decision

Choose exactly one of the following and implement it cleanly.

### Option A: Full removal

- Remove the old bootstrap namespace entirely.
- Move the bootstrap surface into a responsibility-first location and namespace.
- Update all callers accordingly.
- After the change, active source must contain zero `Legacy89DiskKit.Application` namespace declarations and zero `using Legacy89DiskKit.Application` references.

### Option B: Intentional compatibility surface

- Keep `Legacy89DiskKitApplication` only as an explicit compatibility shim.
- Document that it is a compatibility-only bootstrap surface and not part of the responsibility-first core.
- Ensure no active code depends on old layer-first namespaces beyond this one intentional shim.
- If this option is chosen, completion reporting must explicitly say that the migration is complete except for one intentional compatibility surface.

## Verification

```bash
rg -n '^namespace Legacy89DiskKit\.(Application|Domain|Infrastructure)(\.|;)' CSharp -g '*.cs'
rg -n 'using Legacy89DiskKit\.(Application|Domain|Infrastructure)(\.|;)|Legacy89DiskKit\.(Application|Domain|Infrastructure)\.' CSharp -g '*.cs'
dotnet build CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj
dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter "FullyQualifiedName!~ManagedToNativeValidationTest&FullyQualifiedName!~CliCheckUpdateTest"
```

## Completion Rule

The branch can be scored 100/100 only if:
- Option A is completed and no old bootstrap namespace remains in active source, or
- Option B is completed and the remaining bootstrap is explicitly treated as a compatibility exception in the final report.
