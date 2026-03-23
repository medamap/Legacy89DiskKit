# Gemini Implementation Instruction

## Task ID
20260322-233101-m1c-duplication-strategy-hook

## Objective
Introduce an application-layer hook for filesystem-specific duplication strategies so that later X-DOS-specific 2D duplication behavior can be attached without hard-coding all logic into `BootableDuplicationOrchestrationService`.

## Task Kind
implementation + verification

## Slice Rule
This task is intentionally narrow. Add the strategy abstraction and wire the orchestration service to it, but do not implement X-DOS-specific forced placement, raw management-area restoration, or standalone CLI behavior. A generic/default path is enough for now.

## Branch
- Base: `develop`
- Name: `codex/m1c-duplication-strategy-hook`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/FileSystem/BootableDuplicationOrchestrationService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/FileSystem/DiskCloneService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/FileSystem/DuplicationRequest.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/Services/ArchiveService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/Application/BootableDuplicationOrchestrationServiceTest.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- C# first
- 2D first
- Intended layer ownership: `Application`
- Copy route: `image -> image`
- Encoding policy: `RawPreserve`
- Do not modify X-DOS infrastructure behavior in this task
- Do not add speculative encoding conversion logic
- Do not add new CLI commands in this task
- Keep the strategy abstraction small and current-phase oriented
- Do not reset, stash, revert, or otherwise clean unrelated local changes

## Steps
1. Create branch `codex/m1c-duplication-strategy-hook` from `develop`.
2. Introduce a small application-layer abstraction for filesystem-specific duplication handling.
3. Wire `BootableDuplicationOrchestrationService` to use that abstraction.
4. Provide a default/generic implementation that preserves current behavior.
5. If needed, add only minimal request metadata to support strategy selection.
6. Add focused tests that verify:
   - the orchestration service can use the default strategy path
   - behavior remains compatible with the current flow
7. Do not implement any X-DOS-specific special duplication steps in this task.

## Verification
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter "DiskCloneServiceTest|BootableDuplicationOrchestrationServiceTest"`
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false`
- `git diff --stat develop...HEAD`
- `git status --short`

## Acceptance
- application-layer duplication strategy hook exists
- orchestration service uses it
- default path preserves current behavior
- focused tests cover the hook

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
- explicit note of what was deferred to later X-DOS-specific milestones
- explicit note confirming that unrelated local changes were not reset or cleaned

## Advancement Rule
- Do not start the next milestone from within this task
