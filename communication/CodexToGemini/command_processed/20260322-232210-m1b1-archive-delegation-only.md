# Gemini Implementation Instruction

## Task ID
20260322-232210-m1b1-archive-delegation-only

## Objective
Make `ArchiveService.CloneBootable` delegate to `BootableDuplicationOrchestrationService` and do nothing more.

## Task Kind
implementation + verification

## Slice Rule
This task is intentionally minimal. Change only the delegation path in `ArchiveService.CloneBootable` so the dedicated orchestration service is actually used. Do not improve tests in this task beyond the minimum needed to keep the suite green. Do not modify X-DOS infrastructure, duplication internals, CLI, or encoding behavior.

## Branch
- Base: `develop`
- Name: `codex/m1b1-archive-delegation-only`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/Services/ArchiveService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/FileSystem/BootableDuplicationOrchestrationService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- C# first
- 2D first
- Intended layer ownership: `Application`
- Touch only:
  - `CSharp/Legacy89DiskKit.Application/Services/ArchiveService.cs`
  - optionally a tiny supporting edit in `BootableDuplicationOrchestrationService.cs` if strictly required
- Do not add new tests in this task unless the build requires it
- Do not reset, stash, revert, or otherwise clean unrelated local changes

## Steps
1. Create branch `codex/m1b1-archive-delegation-only` from `develop`.
2. Update `ArchiveService.CloneBootable` so it delegates to `BootableDuplicationOrchestrationService`.
3. Keep observable behavior materially equivalent.
4. Keep the diff as small as possible.

## Verification
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false`
- `git diff --stat develop...HEAD`
- `git status --short`

## Acceptance
- `ArchiveService.CloneBootable` no longer contains the full orchestration body
- the method delegates to `BootableDuplicationOrchestrationService`
- `git diff --stat develop...HEAD` is non-empty and includes `ArchiveService.cs`

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
- explicit `git diff --stat develop...HEAD` excerpt
- explicit note confirming that unrelated local changes were not reset or cleaned

## Advancement Rule
- Do not start the next task from within this task
