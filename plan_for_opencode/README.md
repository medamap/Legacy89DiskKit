# Responsibility-First Namespace Completion Plan

## Goal

Complete the remaining responsibility-first migration work in this repository so that:

1. legacy `Legacy89DiskKit.Domain.*` namespaces are migrated to responsibility-first namespaces
2. legacy `Legacy89DiskKit.Infrastructure.*` namespaces are migrated to responsibility-first namespaces
3. the remaining `Legacy89DiskKit.Application` bootstrap/public-surface situation is intentionally finalized
4. all relevant projects build
5. all relevant tests pass
6. `images/test` is regenerated at the end

This plan is complete only when **all child tasks are complete**.

## Rules

- Work in order.
- Keep each task small enough to finish and verify.
- Prefer mechanical migration over ad hoc manual rewrites.
- Small compatibility shims are allowed only when they are necessary to keep the repository buildable during the transition.
- Do not rewrite markdown outside this plan folder unless explicitly required by a task.
- At the end of the whole plan, run:
  - `dotnet build CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj`
  - `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false`
  - regenerate `images/test`

## Child Tasks

1. [01-Inventory-And-Boundaries.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/plan_for_opencode/01-Inventory-And-Boundaries.md)
2. [02-Domain-Namespace-Migration.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/plan_for_opencode/02-Domain-Namespace-Migration.md)
3. [03-Infrastructure-Namespace-Migration.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/plan_for_opencode/03-Infrastructure-Namespace-Migration.md)
4. [04-Application-Bootstrap-Finalization.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/plan_for_opencode/04-Application-Bootstrap-Finalization.md)
5. [05-Final-Verification-And-Image-Regeneration.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/plan_for_opencode/05-Final-Verification-And-Image-Regeneration.md)

## Completion Definition

The work is complete only when all of the following are true:

- No active-source file under `CSharp/` uses legacy `Legacy89DiskKit.Domain.*` namespaces.
- No active-source file under `CSharp/` uses legacy `Legacy89DiskKit.Infrastructure.*` namespaces.
- Any remaining `Legacy89DiskKit.Application` namespace usage is either intentionally preserved as the final public bootstrap or migrated with a clear replacement.
- The repository builds successfully.
- Relevant tests pass successfully.
- `images/test` has been regenerated successfully using the current generator workflow.
