# Gemini Implementation Instruction

## Task ID
20260320-222412-m4-xdos-allocation-bounds-fix

## Objective
Fix the next C# X-DOS correctness issue exposed by the standalone 2D CLI E2E run: `XDosFatWriter.AllocateClusters` must not allocate beyond the physical cluster capacity of the destination disk. After the fix, the standalone CLI must fail gracefully with `Disk full.` instead of crashing with `Sector not found`.

## Branch
- Base: `develop`
- Name: `codex/m4-xdos-allocation-bounds-fix`
- Gemini may commit on this branch if needed
- Gemini must not merge to `develop`

## Files To Read First
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFatWriter.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFatWriter.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosMediaGeometry.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosMediaGeometry.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260320-221906-m3-standalone-cli-2d-e2e.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260320-221906-m3-standalone-cli-2d-e2e.md)

## Required Inputs
- Source image:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- Test output directory:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/test`

## Constraints
- C# first
- 2D first
- Fix only the physical-capacity bounds bug in allocation
- Do not implement shared-cluster support in this task
- Do not change directory semantics or subdirectory support
- Keep the patch narrow and directly tied to the failing standalone flow
- Do not alter unrelated CLI behavior

## Required Changes
1. Bound X-DOS cluster allocation to the actual physical data-cluster capacity of the target disk.
   - `AllocateClusters` must not iterate to `_fat.Length` when the disk cannot physically address that many clusters.
   - Derive the correct upper bound from `XDosMediaGeometry` or another existing disk-geometry source already available in this layer.

2. Preserve the existing reserved-cluster behavior.
   - Do not regress the earlier fix that prevents cluster `0` allocation.
   - Keep existing handling for reserved or already-used clusters intact.

3. Add the smallest useful regression coverage.
   - Add or adjust a test that proves allocation stops at the physical limit and throws `Disk full.` instead of permitting an out-of-range cluster.
   - Prefer a test that is tied to current X-DOS write behavior, not a synthetic helper-only unit unless that is the narrowest stable option.

4. Re-run the standalone 2D publish flow far enough to prove the crash mode changed.
   - It is acceptable if `file cross-copy ... all` still fails because shared-cluster support is missing.
   - After this fix, the expected failure mode is a graceful capacity failure such as `Disk full.`, not `Sector not found`.

## Verification
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter "XDos|WriteFile_NewDisk2DD_CrossCopy|WriteFile_DoesNotAllocateCluster0Or2"`
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false`
- `dotnet publish CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:PublishAot=false -o /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/test/publish-m4-standalone-cli`
- Run the published binary with:
  - `disk create` for a new 2D X-DOS image
  - `disk boot-copy` from `XDOS_SYS.D88`
  - `file cross-copy ... all`
- Capture the exact post-fix error text for the cross-copy step

## Deliverable
Write one Markdown report to:

- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_waiting`

After completion:

- Move this instruction file to:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/CodexToGemini/command_processed`

## Report Requirements
- task id
- instruction filename
- branch_name
- summary
- changed_files
- commands
- evidence
- risks
- requested_review

## Expected Result
- The allocation path no longer returns out-of-range clusters for 2D media
- The standalone CLI no longer crashes with `Sector not found: C=40, H=0, R=1`
- If the full fileset still does not fit, the CLI should fail cleanly with a capacity error
