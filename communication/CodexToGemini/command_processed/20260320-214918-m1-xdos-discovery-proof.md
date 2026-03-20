# Gemini Retry Instruction

## Task ID
20260320-212741-m1-xdos-discovery

## Reason For Retry

- The previous retry report still made high-confidence conclusions without executable or sample-backed proof.
- It did not include the required `branch_name`.
- It asserted that current docs are wrong, but did not supply enough direct evidence from the actual sample disk and current code paths to close the question.

## Branch
- Base: `develop`
- Name: `none`
- This is an investigation-only proof task
- Do not create a working branch

## Objective
Produce a final proof-oriented discovery report that answers Issue #5 and Issue #6 with direct evidence from the current codebase and the actual 2D X-DOS sample disk.

## Files To Read First

- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamReader.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamReader.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosFileSystem.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosFileSystem.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/XDos_Infrastructure_Fix_Plan.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/XDos_Infrastructure_Fix_Plan.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88)

## Constraints

- Do not edit repo files
- `changed_files` must be `[]`
- `branch_name` must be `none`
- Prefer executable evidence over speculative reasoning
- If you cannot prove a conclusion from current artifacts, mark it `unknown`
- Keep focus on C# and 2D only

## Required Proof Work

1. Prove or disprove the `Issue #5` conclusion from current artifacts.
   - Use the actual sample disk and current code assumptions.
   - At minimum, show one concrete file or system payload where:
     - current `GetChain` output,
     - expected file size,
     - and actual readable payload size
     either agree or disagree.
   - If needed, run existing X-DOS tests or targeted commands, but do not patch code.

2. Prove or disprove the `Issue #6` conclusion from current artifacts.
   - Distinguish:
     - “bootable logical clone”
     - “file-level logical copy”
   - Show whether `bdir` or another fixed physical area must be reproduced for the 2D boot case.
   - If this cannot be proven from current code and sample evidence alone, say exactly what remains unknown.

3. Produce a final ordered implementation recommendation.
   - first correctness fix
   - first false-success guardrail
   - first minimum write-path enhancement

## Verification

Use any of these if they help you prove the answer:

```bash
dotnet test csharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter XDos
dotnet test csharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter "WriteFileInternal_DuplicateDisk_LogicalReconstruction|WriteFile_NewDisk2DD_CrossCopy"
dotnet run --project csharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -- list images/disk_org/x1/XDOS_SYS.D88
```

## Deliverable

Write one Markdown report to:

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
