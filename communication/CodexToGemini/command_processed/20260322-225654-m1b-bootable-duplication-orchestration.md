# Gemini Implementation Instruction

## Task ID
20260322-225654-m1b-bootable-duplication-orchestration

## Objective
Extract the current bootable image duplication sequence into a dedicated application-level orchestration service so that later X-DOS-specific duplication behavior can be attached without further growing `ArchiveService`.

## Task Kind
implementation + verification

## Slice Rule
This task is intentionally narrow. It must only reorganize application-layer orchestration for the current `image -> image` bootable duplication flow. It must not implement X-DOS-specific forced placement writes, raw FAT/FAM/Directory restoration, standalone CLI E2E, or new encoding conversion behavior.

## Branch
- Base: `develop`
- Name: `codex/m1b-bootable-duplication-orchestration`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/Services/ArchiveService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/FileSystem/DiskCloneService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/Legacy89DiskKitApplication.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/Application/DiskCloneServiceTest.cs`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- C# first
- 2D first
- Copy route for this task: `image -> image`
- Encoding policy for this task: `RawPreserve`
- Intended layer ownership: `Application`
- Do not modify X-DOS infrastructure behavior in this task
- Do not add speculative encoding conversion logic
- Do not add new CLI commands in this task
- Do not reset, stash, revert, or otherwise clean unrelated local changes
- Ignore unrelated modified or untracked files unless they block your target files

## Steps
1. Create branch `codex/m1b-bootable-duplication-orchestration` from `develop`.
2. Introduce a dedicated application service for bootable image duplication orchestration under `CSharp/Legacy89DiskKit.Application/FileSystem/` or another application-appropriate location.
3. Move the current sequence now embedded in `ArchiveService.CloneBootable` behind that dedicated service:
   - source image open
   - destination image create
   - boot-area preparation / transfer orchestration
   - destination format
   - duplication request dispatch
   - final boot-area write
4. Keep behavior materially equivalent to the current implementation.
5. Update `ArchiveService.CloneBootable` to delegate to the new orchestration service.
6. Add focused tests that verify the orchestration surface or sequencing assumptions at the application layer.
7. Do not implement X-DOS-specific duplication internals in this task.

## Verification
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter DiskCloneServiceTest`
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false`
- `git diff --stat develop...HEAD`
- `git status --short`

## Acceptance
- Unit expectation:
  - dedicated bootable duplication orchestration exists in `Application`
  - existing archive entrypoint delegates to it
  - focused tests cover the orchestration slice or its public behavior
- Sample-image regression expectation:
  - no sample-image regression is required in this slice
- Standalone CLI expectation if applicable:
  - none in this slice

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
- explicit note of what was intentionally deferred to later duplication milestones
- explicit note confirming that unrelated local changes were not reset or cleaned

## Advancement Rule
- Do not start the next milestone from within this task
