# Gemini Investigation Instruction

## Task ID
20260320-235810-m1-xdos-kernel-reanalysis-inventory

## Objective
Perform Milestone 1 of the X-DOS kernel reanalysis plan only. Build a trusted-source inventory, identify contradictions across the current X-DOS analysis assets, and produce an evidence ledger that separates direct evidence, secondary inference, and unknowns. Do not implement code changes in this task.

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
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/document_index.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/document_index.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_License_And_Sources.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_License_And_Sources.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/XDos_Infrastructure_Fix_Plan.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/XDos_Infrastructure_Fix_Plan.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosFileSystem.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosFileSystem.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamReader.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamReader.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs)

## Required Inputs
- Primary disk image:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- Secondary disk image:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`

## Constraints
- Analyze only Milestone 1
- No source code edits
- No markdown document edits
- No branch creation
- No implementation proposals beyond what is needed to describe contradictions and unknowns
- Every factual claim must be tied to one of:
  - current repo documents
  - current C# implementation
  - sample disk observation
  - salvaged-source references already present in repo documents
- If a claim is not directly proven, label it `secondary inference`
- If a claim cannot be proven from available evidence, label it `unknown`

## Required Work
1. Build a source inventory for the X-DOS reanalysis effort.
   - Classify each relevant asset as:
     - primary evidence
     - secondary analysis
     - implementation assumption
     - unknown / needs confirmation
   - Include both documents and executable/sample assets.

2. Extract the current mutually contradictory claims.
   - At minimum cover:
     - meaning of `FAM[N] = 0x00`
     - what one X-DOS cluster actually represents
     - meaning and role of `FirstSectorR`
     - FAT responsibility vs FAM responsibility
     - whether IPL / early boot depends on fixed physical records or only logical lookup
   - For each contradiction, identify:
     - the exact files or observations that support side A
     - the exact files or observations that support side B
     - whether the contradiction is direct evidence vs inference-level conflict

3. Build an evidence ledger.
   - Separate sections:
     - direct evidence
     - secondary inference
     - unknown
   - Keep each entry short and evidence-linked.

4. Produce a Milestone 1 conclusion.
   - Fix the priority order of sources for Milestone 2 onward.
   - State which questions are now ready for M2 read-path analysis.
   - State which questions remain blocked even after M1.

## Suggested Non-Mutating Commands
- `rg -n "X-DOS|XDOS|FAM|FAT|FirstSectorR|bdir|sys_rdd|sys_wrd|sys_ropen|sys_wopen" Documents CSharp csharp communication -g '!**/bin/**' -g '!**/obj/**'`
- `dotnet run --project CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -- list /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `dotnet run --project CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -- list /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `hexdump -C /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88 | head -n 80`
- `hexdump -C /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88 | head -n 80`

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
- A stable list of trusted X-DOS sources for the reanalysis
- A contradiction matrix covering the currently disputed X-DOS semantics
- An evidence ledger split into direct evidence, secondary inference, and unknowns
- A clean handoff into Milestone 2 without making implementation changes
