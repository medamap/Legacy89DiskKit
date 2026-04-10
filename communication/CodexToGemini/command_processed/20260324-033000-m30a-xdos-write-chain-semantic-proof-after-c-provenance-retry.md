# Gemini Implementation Instruction

## Task ID
20260324-033000-m30a-xdos-write-chain-semantic-proof-after-c-provenance-retry

## Objective
Keep the new write-side semantic proof-attempt section but remove wording that overstates the resolved identity of the `OUT (C), *` / `IN *, (C)` port after C-register provenance was cataloged.

## Branch
- Base: `develop`
- Name: `codex/m30a-xdos-write-chain-semantic-proof-after-c-provenance-retry`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Required Inputs
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md

## Files To Read First
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use repository-local evidence only
- Mark uncertainty as `unknown`
- Limit changes to:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- Do not modify `README.md`
- Do not use `port 1A`, `1A00`, `1Axx`, `PPI`, `Graphic RAM`, or any external machine-role wording
- Keep the section conservative: provenance is observed, semantic role remains unknown

## Steps
1. Start from `develop`.
2. Keep the section `## Write Mutation Semantic Proof Attempt After C Provenance (Analysis-Only)`.
3. Rewrite only the two boundary cells so they refer to:
   - observed C-register provenance
   - observed `OUT (C), *` / `IN *, (C)` patterns
   - unresolved downstream logical role
4. Do not modify any other sections.

## Verification
- `git diff -- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`

## Deliverable
- Markdown report in `communication/GeminiToCodex/report_waiting/`

## Report Requirements
- branch_name
- summary
- changed_files
- commands
- evidence
- risks
- requested_review
- state explicitly that the retry only softened wording and did not upgrade any grade

## User-Facing Handoff Block Rule
- If Codex also returns a copyable message for the user to forward to Gemini, do not nest code blocks inside that message
- Show commands as plain text list items instead
