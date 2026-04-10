# Gemini Implementation Instruction

## Task ID
20260322-224801-m1a-duplication-contract

## Objective
Introduce a narrow application-level duplication contract for the current 2D work so that later X-DOS-specific duplication behavior can plug into a formal request/options model instead of continuing to grow ad hoc parameters.

## Task Kind
implementation + verification

## Slice Rule
This task is intentionally narrow. It must formalize the duplication request and option surface in `Application` and adapt existing services to use it, but it must not implement X-DOS-specific forced placement logic, raw management-area overwrite sequencing, or standalone CLI E2E behavior. Those are deferred to later tasks.

## Branch
- Base: `develop`
- Name: `codex/m1a-duplication-contract`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/FileSystem/DiskCloneService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/Services/ArchiveService.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Application/Legacy89DiskKitApplication.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Tests/Application/DiskCloneServiceTest.cs`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/CSharp/Legacy89DiskKit.Cli/Program.cs`

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
- Do not wire new CLI commands yet unless a tiny plumbing change is strictly necessary
- Do not reset, stash, revert, or otherwise clean unrelated local changes
- Ignore unrelated modified or untracked files unless they block your target files

## Steps
1. Create branch `codex/m1a-duplication-contract` from `develop`.
2. Introduce a duplication request/options model in `CSharp/Legacy89DiskKit.Application/FileSystem/` suitable for the current 2D phase.
3. The model must make these concerns explicit without implementing future routes yet:
   - copy route
   - encoding policy
   - whether boot area transfer is requested
   - selected file set
4. Adapt `DiskCloneService` to expose one formal entrypoint that uses the new request/options model.
5. Keep current behavior compatible by delegating to the existing internal operations where possible.
6. Update `ArchiveService.CloneBootable` to use the new request/options model instead of ad hoc orchestration.
7. Add focused unit tests for the new contract and the service behavior it enables.
8. Do not add X-DOS-specific duplication internals in this task.

## Verification
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter DiskCloneServiceTest`
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false`
- `git diff --stat develop...HEAD`
- `git status --short`

## Acceptance
- Unit expectation:
  - duplication request/options contract has focused tests
  - `DiskCloneService` has a formal duplication entrypoint covered by tests
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
