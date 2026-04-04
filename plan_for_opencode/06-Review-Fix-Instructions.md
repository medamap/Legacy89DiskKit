# Review Fix Instruction

## Score

Current score: 72 / 100

Reasoning:
- Project and folder restructuring is substantially complete.
- Build and filtered tests pass.
- However, the migration completion criteria are not met because active source still keeps the old bootstrap namespace and still contains old layer-first namespace references.

## Required Fixes

### 1. Remove active use of `Legacy89DiskKit.Application`

- Treat `Legacy89DiskKit.Application` as unfinished unless it is explicitly retained as a compatibility surface.
- Preferred direction: migrate active callers away from it.
- Update active callers such as CLI, tests, NativeInterop, and tools to reference the new responsibility-first namespaces directly.
- After migration, confirm there are no active `using Legacy89DiskKit.Application;` imports left under `CSharp/`.

Verification query:

```bash
rg -n 'using Legacy89DiskKit\.Application(;|\.)' CSharp -g '*.cs'
```

Expected result:
- zero hits in active source, or a consciously documented compatibility-only location with no active dependency chain.

### 2. Remove remaining active `Legacy89DiskKit.Domain.*` references

- Replace fully-qualified old layer-first domain references with responsibility-first names.
- Do not stop at namespace declarations; also remove fully-qualified usages.
- Representative known remaining area:
  - `CSharp/VerificationTool/Program.cs`

Verification query:

```bash
rg -n 'Legacy89DiskKit\.Domain\.' CSharp -g '*.cs'
```

Expected result:
- zero hits in active source.

### 3. Remove remaining active `Legacy89DiskKit.Infrastructure.*` references

- Replace old infrastructure namespace references in active code with responsibility-first equivalents.

Verification query:

```bash
rg -n 'Legacy89DiskKit\.Infrastructure\.' CSharp -g '*.cs'
```

Expected result:
- zero hits in active source.

### 4. Re-run full migration completion checks

Run all of the following after fixes:

```bash
rg -n '^namespace Legacy89DiskKit\.(Application|Domain|Infrastructure)(\.|;)' CSharp -g '*.cs'
rg -n 'using Legacy89DiskKit\.(Application|Domain|Infrastructure)(\.|;)|Legacy89DiskKit\.(Application|Domain|Infrastructure)\.' CSharp -g '*.cs'
dotnet build CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj
dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter "FullyQualifiedName!~ManagedToNativeValidationTest&FullyQualifiedName!~CliCheckUpdateTest"
```

Expected result:
- no active old namespace hits
- build success
- filtered tests green

## Completion Rule

This migration review may be considered complete only when:
- active source no longer depends on `Legacy89DiskKit.Application`
- active source no longer contains `Legacy89DiskKit.Domain.*` references
- active source no longer contains `Legacy89DiskKit.Infrastructure.*` references
- build and filtered tests still pass
