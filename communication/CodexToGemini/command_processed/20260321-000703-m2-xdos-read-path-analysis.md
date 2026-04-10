# Gemini Investigation Instruction

## Task ID
20260321-000703-m2-xdos-read-path-analysis

## Objective
Perform Milestone 2 of the X-DOS kernel reanalysis plan only. Determine the X-DOS read path from directory entry to final file payload, and classify each conclusion by evidence grade. Focus on `FirstCluster`, `FirstSectorR`, FAM traversal, FAT involvement, and EOF/size handling. Do not implement code changes in this task.

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
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-000112-m1-xdos-kernel-reanalysis-inventory-report.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-000112-m1-xdos-kernel-reanalysis-inventory-report.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosFileSystem.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosFileSystem.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamReader.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamReader.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosDirParser.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs)

## Required Inputs
- Primary disk image:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- Secondary disk image:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`

## Constraints
- Analyze Milestone 2 only
- No code edits
- No markdown edits
- No branch creation
- Do not drift into write-path conclusions except where they are strictly needed to explain read-path evidence
- Keep evidence grades explicit:
  - primary evidence
  - secondary analysis
  - implementation assumption
  - unknown
- A current C# implementation result is not by itself proof of original kernel semantics

## Required Work
1. Identify the read-path entrypoints and reachable evidence.
   - At minimum analyze:
     - `sys_file`
     - `sys_ropen`
     - `sys_rdd`
     - `sys_devi` if needed
   - For each, say whether the evidence is direct or secondary.

2. Reconstruct the X-DOS read path as a state machine or ordered algorithm.
   - Directory lookup
   - Use of `FirstCluster`
   - Use of `FirstSectorR`
   - Whether FAT is referenced during read
   - How FAM is traversed
   - What ends the read
     - FAM marker
     - file size field
     - both
     - unknown

3. Build a representative file table from `XDOS_SYS.D88`.
   - Include at least:
     - one small file
     - one larger file
     - one system file
   - For each file, provide:
     - filename
     - directory values of interest
     - relevant FAM entries
     - starting physical location
     - observed or inferred final size
   - If you cannot find `FirstSectorR != 1` in the primary images, state that explicitly.

4. Compare the confirmed read path against current C# behavior.
   - Classify each difference as:
     - confirmed mismatch
     - plausible mismatch
     - currently aligned
     - unknown

5. End with an M2 conclusion.
   - Provide a conservative pseudocode sketch for the read path
   - State exactly which questions are resolved for M3 and which remain open

## Suggested Non-Mutating Commands
- `rg -n "sys_file|sys_ropen|sys_rdd|sys_devi|FirstSectorR|FAM\\[|ReadFileRaw|ReadFile\\(" Documents CSharp csharp`
- `dotnet run --project CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -- list /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `dotnet run --project CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -- list /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `hexdump -C /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88 | head -n 120`

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
- representative file table
- read-path algorithm
- current C# comparison
- provisional conclusions
- unknown
- next milestone inputs
- requested_review

## Expected Result
- A conservative, evidence-graded read-path specification for X-DOS
- Clear classification of what `FirstSectorR`, FAM, FAT, and size fields do during reads
- A concrete handoff into Milestone 3 without making implementation changes
