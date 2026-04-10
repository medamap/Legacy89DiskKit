# Gemini Implementation Instruction

## Task ID
20260322-223940-m16-xdos-implementation-spec

## Objective
Convert the current X-DOS kernel analysis into a decision-complete implementation spec for the C# line, focused on what must change to make 2D X-DOS duplication viable and how those changes should be verified.

## Branch
- Base: `develop`
- Name: `codex/m16-xdos-implementation-spec`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/find_file_start.py`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosFileSystem.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFamReader.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosClusterReader.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFatWriter.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- This is an analysis/spec task; do not modify C# or C++ product code
- Do not reset, stash, revert, or otherwise clean unrelated local changes
- Ignore unrelated modified or untracked files unless they block your target files
- Do not create a broad roadmap document
- Keep changes inside tracked analysis assets only
- Be explicit about evidence class
- Separate what is implementation-ready from what still depends on unknowns

## Steps
1. Create branch `codex/m16-xdos-implementation-spec` from `develop`.
2. Re-read the current tracked X-DOS analysis and the current C# X-DOS implementation files.
3. Produce a decision-complete implementation section that answers:
   - what read-side behavior is already safe to implement or preserve
   - what write-side behavior must change for 2D duplication to become viable
   - what boot-related invariants must be preserved
   - what must still remain guarded behind `unknown` and not be implemented speculatively
4. Define the implementation sequence for C# as a short ordered list.
5. Define the required test layers:
   - unit
   - sample-image regression
   - standalone CLI E2E
6. Explicitly identify which previous assumptions should now be rejected, preserved, or deferred.
7. Update `analysis/xdos-kernel/boot_and_io_notes.md` with an evidence-graded implementation-spec section.
8. Update `analysis/xdos-kernel/README.md` only if the critical-unknown wording materially improves.
9. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m16-xdos-implementation-spec`
- `git diff --stat develop...HEAD`
- `git diff -- analysis/xdos-kernel/README.md analysis/xdos-kernel/boot_and_io_notes.md`
- `git status --short`

## Deliverable
- Markdown report in `communication/GeminiToCodex/report_waiting/`

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
- contradictions
- provisional conclusions
- unknown
- explicit ordered implementation sequence for C#
- explicit required test matrix
- explicit note confirming that unrelated local changes were not reset or cleaned

## Advancement Rule
- Do not start a new milestone from within this task
