# Gemini Retry Instruction

## Task ID
20260320-212741-m1-xdos-discovery

## Reason For Retry

- The previous report did not answer the requested `Issue #5` and `Issue #6` correctly.
- It mapped `Issue #5` to a trailing-dot presentation issue, but the requested issue is the X-DOS FAM read semantics problem around `FAM[N]=0x00`.
- It concluded `file cross-copy` can still be correct on 2D without shared-cluster-aware writing, which conflicts with the current task framing and must be re-evaluated from the actual 2D capacity and shared-cluster evidence.
- It relied on conclusions that are not decision-complete for the next implementation milestone.

## Branch
- Base: `develop`
- Name: `none`
- This is an investigation-only retry
- Do not create or use an implementation branch for this task

## Objective
Produce a corrected, decision-complete investigation report for the first C# X-DOS fixes needed for reliable 2D bootable clone support.

## Files To Read First

- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamReader.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamReader.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamWriter.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamWriter.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosFileSystem.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosFileSystem.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/XDos_Infrastructure_Fix_Plan.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/XDos_Infrastructure_Fix_Plan.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88)

## Constraints

- Do not implement code
- `changed_files` must be `[]`
- Use evidence only
- Mark uncertainty as `unknown`
- Focus on C# first
- Focus on 2D first
- Do not drift into subdirectory support
- Do not replace the requested issue numbers with different bugs

## Required Corrections

1. Re-answer `Issue #5` as the FAM read-semantics question.
   - Determine whether the current repository should treat `FAM[N]=0x00` as:
     - end of chain
     - or implicit continuation to the next contiguous cluster
   - Base the answer on the repo's code, tests, current X-DOS docs, and the specific 2D problem statement.
   - If the docs and the observed 2D failure disagree, say so explicitly and identify which change should be implemented first in this repo.

2. Re-answer `Issue #6` as the shared-cluster write-support question.
   - Determine whether reliable 2D logical bootable clone of `XDOS_SYS.D88` is possible without reproducing shared-cluster placement and `FirstSectorR`.
   - If not, state the minimum implementation surface needed.

3. Produce a corrected “smallest code surface to change first”.
   - If more than one bug exists, order them for implementation.
   - Distinguish:
     - first correctness fix
     - first guardrail against false success
     - first minimum shared-cluster-aware write change

4. Re-evaluate `file cross-copy` on 2D.
   - Answer whether it can be trusted today
   - Answer whether it can become correct with only Issue #5 fixed
   - Answer whether it still requires Issue #6 for full 2D system-disk duplication

## Verification

Use these only if they strengthen the report:

```bash
dotnet test csharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter XDos
dotnet test csharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false
```

## Deliverable

Write one corrected Markdown report to:

- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_waiting`

## Report Requirements

- task id
- instruction filename
- branch_name: `none`
- summary
- changed_files
- commands
- evidence
- risks
- requested_review
- explicit `unknown` section if needed
