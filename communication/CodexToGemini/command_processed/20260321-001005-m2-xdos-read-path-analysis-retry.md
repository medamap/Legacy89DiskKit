# Gemini Investigation Instruction

## Task ID
20260321-000703-m2-xdos-read-path-analysis

## Objective
Retry Milestone 2 of the X-DOS read-path analysis. The previous report was close, but it still over-classified some claims as primary evidence. Redo the read-path specification with stricter grading, especially around FAM termination values beyond `0x00`, current C# alignment, and the exact evidence for `FirstSectorR != 1`.

## Branch
- Base: `develop`
- Name: `none`
- This is an investigation-only task
- Do not create a branch
- Do not modify source code
- Do not merge

## Files To Read First
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-000112-m1-xdos-kernel-reanalysis-inventory-report.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-000112-m1-xdos-kernel-reanalysis-inventory-report.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamReader.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamReader.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosDirParser.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosDirParser.cs)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs)

## Retry Reasons
- The prior report treated current C# alignment as more strongly proven than the available evidence supports
- `0xFF` and `0xD5` as FAM terminators were described too confidently for an original-kernel read-path report
- The `FirstSectorR != 1` example needs to be explicitly backed by direct disk evidence, not just a summary statement

## Constraints
- No code edits
- No markdown edits
- Keep Milestone 2 scope only
- Use strict evidence grades:
  - primary evidence
  - secondary analysis
  - implementation assumption
  - unknown
- Treat current C# behavior as implementation assumption unless directly mirrored by primary artifacts

## Required Work
1. Rebuild the read-path report with stricter grading.
   - Explicitly separate:
     - what the original artifacts prove
     - what current C# does
     - where the two seem to align

2. Re-evaluate FAM termination.
   - It is acceptable to conclude `0x00` is the only primary-evidence terminator for now.
   - If `0xFF` and `0xD5` appear only in current code or directory-entry deletion logic, classify them accordingly.

3. Re-evaluate `FirstSectorR`.
   - If you claim an example with `FirstSectorR != 1`, cite the exact file and why the evidence is primary.
   - If the evidence is indirect, downgrade it.

4. Rebuild the current C# comparison.
   - Use statuses:
     - `supported by primary evidence`
     - `plausibly aligned`
     - `implementation-only`
     - `unknown`

5. Keep the representative file table and read-path algorithm, but ensure every row/step carries the correct evidence grade.

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
- A conservative, evidence-disciplined M2 report
- No overstatement that current C# behavior is fully proven by original artifacts
- A clean handoff into M3 write-path analysis
