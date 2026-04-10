# Gemini Implementation Instruction

## Task ID
20260322-230922-m1b-bootable-duplication-orchestration-retry2

## Objective
Complete the application-layer orchestration extraction by making `ArchiveService.CloneBootable` actually delegate to the dedicated service and by adding a more meaningful orchestration-focused test.

## Task Kind
implementation + verification

## Slice Rule
This retry remains narrow. Do not touch X-DOS infrastructure, forced placement logic, raw FAT/FAM/Directory restoration, or CLI E2E. Fix only the missing delegation and improve the orchestration test so the new service is not just dead code.

## Branch
- Base: `develop`
- Name: `codex/m1b-bootable-duplication-orchestration-retry2`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/Services/ArchiveService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/FileSystem/BootableDuplicationOrchestrationService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/Application/BootableDuplicationOrchestrationServiceTest.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- C# first
- 2D first
- Intended layer ownership: `Application`
- This retry is invalid unless the branch diff contains:
  - `ArchiveService.cs`
  - `BootableDuplicationOrchestrationService.cs`
  - `BootableDuplicationOrchestrationServiceTest.cs`
- Do not reset, stash, revert, or otherwise clean unrelated local changes

## Steps
1. Create branch `codex/m1b-bootable-duplication-orchestration-retry2` from `develop`.
2. Update `ArchiveService.CloneBootable` so it delegates to `BootableDuplicationOrchestrationService` instead of embedding the sequence inline.
3. Keep the new service as the orchestration owner for the current bootable duplication sequence.
4. Improve the test so it exercises meaningful public behavior of the orchestration service or its delegation path, not just constructor viability.
5. Run the required tests.
6. Before reporting, verify that `git diff --stat develop...HEAD` contains all three tracked files above.

## Verification
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter "DiskCloneServiceTest|BootableDuplicationOrchestrationServiceTest"`
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false`
- `git diff --stat develop...HEAD`
- `git status --short`

## Acceptance
- `ArchiveService.CloneBootable` is actually delegated
- the new service is not dead code
- the new test is stronger than a constructor smoke test

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
- explicit `git diff --stat develop...HEAD` excerpt showing all three files

## Advancement Rule
- Do not start the next milestone from within this task
