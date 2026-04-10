# Gemini Implementation Instruction

## Task ID
20260321-131911-m12-xdos-boot-clone-conditions

## Objective
Retry the final X-DOS boot/clone condition spec without overwriting earlier analysis sections, and with stricter wording around geometry and logical mapping.

## Branch
- Base: `develop`
- Name: `codex/m12-xdos-boot-clone-conditions-retry2`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-131911-m12-xdos-boot-clone-conditions-report.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- This is an analysis-only task; do not modify C# or C++ product code
- Do not reset, stash, revert, or otherwise clean unrelated local changes
- Ignore unrelated modified or untracked files unless they block your target files
- Do not delete or replace earlier read/write analysis sections
- The final section must be appended, not substituted for prior material
- Avoid hard claims about physical sector geometry unless the exact claim is directly supported
- Distinguish clearly between:
  - physical sector layout observations
  - logical record addressing used by X-DOS
  - image-level observations
  - tool-observed behavior
  - hypotheses

## Why The Previous Attempt Failed
1. It removed earlier read/write spec content from `boot_and_io_notes.md`.
2. It mixed physical geometry claims and logical record mapping too strongly.
3. It still made some statements sound more settled than the current evidence supports.

## Steps
1. Create branch `codex/m12-xdos-boot-clone-conditions-retry2` from `develop`.
2. Restore the approach so that earlier read/write sections remain intact.
3. Add a new final section at the end of `boot_and_io_notes.md` that summarizes:
   - kernel-proven facts
   - image-level observations
   - tool-observed behavior
   - strong hypotheses
   - unknowns
4. When discussing geometry:
   - separate "physical sector headers seen in D88" from "logical records used by X-DOS kernel"
   - do not flatten them into a single proven statement unless the evidence directly supports it
5. Update `README.md` only if needed, and do not claim decision-complete status.
6. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m12-xdos-boot-clone-conditions-retry2`
- `git diff --stat develop...HEAD`
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`
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
- explicit note confirming that unrelated local changes were not reset or cleaned
- explicit note confirming that prior read/write sections were preserved

## Advancement Rule
- This retry is allowed without new user approval because it is the same failed task
