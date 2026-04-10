# Gemini Investigation Instruction

## Task ID
20260320-235810-m1-xdos-kernel-reanalysis-inventory

## Objective
Retry Milestone 1 of the X-DOS kernel reanalysis. The previous report over-classified some conclusions as direct evidence and mixed implementation parity with kernel-spec proof. Redo the source inventory and contradiction matrix with strict evidence grading only.

## Branch
- Base: `develop`
- Name: `none`
- This is an investigation-only task
- Do not create a branch
- Do not modify source code
- Do not merge

## Files To Read First
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_License_And_Sources.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_License_And_Sources.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/XDos_Infrastructure_Fix_Plan.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/XDos_Infrastructure_Fix_Plan.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosDirWriter.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosDirWriter.cs)

## Required Inputs
- Primary disk image:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- Secondary disk image:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`

## Retry Reasons
- The previous report treated some document claims as direct evidence without showing a lower-level primary source
- The previous report treated a C# reconstruction parity test as proof of kernel semantics, which is too strong for M1
- The contradiction table included at least one weak or off-target contradiction (`FirstSectorR` row)
- The report drifted into implementation-review commentary (`XDosDirWriter` offsets) instead of keeping focus on source inventory and contradiction structure

## Constraints
- No code edits
- No markdown edits
- Keep Milestone 1 scope only
- Use strict evidence grades:
  - `primary evidence`
  - `secondary analysis`
  - `implementation assumption`
  - `unknown`
- Do not label something `primary evidence` unless you can point to:
  - direct disk observation
  - salvaged source text quoted/paraphrased from repo materials
  - or current code behavior observed directly from the repo
- A passing C# test is not by itself proof of original X-DOS kernel semantics
- If a claim is only stated in a document and not shown from the underlying artifact, keep it as `secondary analysis`

## Required Work
1. Rebuild the source inventory with strict grading.
   - For each source, give:
     - category
     - why it belongs there
     - what it can and cannot prove

2. Rebuild the contradiction matrix.
   - Only include contradictions that materially affect future M2 analysis.
   - At minimum include:
     - FAM `0x00` meaning
     - cluster unit / mapping
     - `FirstSectorR` meaning
     - FAT vs FAM responsibility
     - IPL / early boot physical dependency
   - For each contradiction, include:
     - side A claim
     - side B claim
     - evidence grade for each side
     - what is still missing to resolve it

3. Rebuild the evidence ledger.
   - Separate:
     - primary evidence
     - secondary analysis
     - implementation assumptions
     - unknown
   - Do not collapse these categories.

4. End with a Milestone 1 conclusion that is intentionally conservative.
   - List which claims are safe to carry into M2.
   - List which claims must remain open until read-path analysis.

## Suggested Non-Mutating Commands
- `rg -n "EE 10|FAM\\[2\\]=0x09|FAM\\[2\\]=9|sys_rdd|sys_wrd|sys_ropen|sys_wopen|FirstSectorR|bdir" Documents CSharp csharp`
- `sed -n '190,280p' CSharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs`
- `dotnet run --project CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -- list /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `hexdump -C /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88 | head -n 80`

## Deliverable
Write one Markdown report to:

- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_waiting`

After completion:

- Move this instruction file to:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/CodexToGemini/command_processed`

## Report Requirements
- task id
- instruction filename
- branch_name: `none`
- summary
- evidence
- contradictions
- provisional conclusions
- unknown
- next milestone inputs
- requested_review

## Expected Result
- A conservative, evidence-disciplined M1 report
- No overstatement that current C# behavior proves original kernel semantics
- A contradiction matrix that is directly useful for M2 read-path analysis
