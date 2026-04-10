# Gemini Implementation Instruction

## Task ID
20260322-230304-m1b-bootable-duplication-orchestration-retry

## Objective
Retry the bootable duplication orchestration extraction. The previous report claimed success, but the branch had no diff from `develop`. This retry must produce actual tracked code changes and tests.

## Task Kind
implementation + verification

## Slice Rule
This remains a narrow application-layer refactor. Extract the orchestration from `ArchiveService.CloneBootable` into a dedicated service and add focused tests. Do not implement X-DOS-specific forced placement logic, raw FAT/FAM/Directory restoration, CLI E2E, or encoding conversion changes.

## Branch
- Base: `develop`
- Name: `codex/m1b-bootable-duplication-orchestration-retry`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/Services/ArchiveService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/FileSystem/DiskCloneService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/Application/DiskCloneServiceTest.cs`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- C# first
- 2D first
- Copy route: `image -> image`
- Encoding policy: `RawPreserve`
- Intended layer ownership: `Application`
- This retry is invalid unless `git diff --stat develop...HEAD` is non-empty
- Add a real tracked service file and a real tracked test file or test delta
- Do not reset, stash, revert, or otherwise clean unrelated local changes

## Steps
1. Create branch `codex/m1b-bootable-duplication-orchestration-retry` from `develop`.
2. Add a dedicated orchestration service for the bootable duplication sequence.
3. Update `ArchiveService.CloneBootable` to delegate to that service.
4. Add focused tests that exercise the new service or its public behavior.
5. Run the required tests.
6. Before reporting, verify that `git diff --stat develop...HEAD` is non-empty and mentions the service/test changes.

## Verification
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter "DiskCloneServiceTest|BootableDuplicationOrchestrationServiceTest"`
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false`
- `git diff --stat develop...HEAD`
- `git status --short`

## Acceptance
- Unit expectation:
  - dedicated bootable duplication orchestration exists in `Application`
  - `ArchiveService.CloneBootable` delegates to it
  - focused tests cover the orchestration slice
- Sample-image regression expectation:
  - not required in this retry
- Standalone CLI expectation:
  - not required in this retry

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
- explicit `git diff --stat develop...HEAD` output excerpt
- explicit note confirming that unrelated local changes were not reset or cleaned

## Advancement Rule
- Do not start the next milestone from within this task
