# Gemini Implementation Instruction

## Task ID
20260321-131911-m12-xdos-boot-clone-conditions

## Objective
Retry the final X-DOS boot/clone condition spec with stricter evidence grading, avoiding over-claiming what the current kernel evidence proves.

## Branch
- Base: `develop`
- Name: `codex/m12-xdos-boot-clone-conditions-retry`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-131911-m12-xdos-boot-clone-conditions-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260320-221906-m3-standalone-cli-2d-e2e.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260320-222412-m4-xdos-allocation-bounds-fix.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-130000-m10-xdos-read-path-spec-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-131146-m11-xdos-write-path-spec-report.md`

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
- Do not invent bytes, synthesized instructions, or guessed boot rules
- Keep the boot/clone spec conservative and evidence-graded
- Distinguish explicitly between:
  - direct-byte or direct-image observations
  - observed tool behavior
  - instruction-level inference
  - broader behavioral hypotheses

## Why The Previous Attempt Failed
1. It treated `shared-cluster mapping` as proven from the kernel evidence.
2. It escalated "Shared-Cluster Writer is required" from a strong hypothesis to a final decision.
3. It updated `README.md` to say the phase was decision-complete, which is too strong while clone-critical unknowns remain.

## Steps
1. Create branch `codex/m12-xdos-boot-clone-conditions-retry` from `develop`.
2. Rewrite the final boot/clone section so that it explicitly separates:
   - kernel-proven facts
   - image-level observations
   - tool-behavior observations
   - hypotheses
3. In particular:
   - `shared placement exists in source image` may be stated only as an image-level observation if backed by prior reports
   - `shared-cluster write support is required` must be phrased as a strong working hypothesis, not a proven decision
   - `FirstSectorR` behavior must remain `unknown` unless directly proven
4. Remove or soften any claim that the reanalysis phase is fully decision-complete if unresolved implementation-critical unknowns remain.
5. Update `analysis/xdos-kernel/boot_and_io_notes.md` and `analysis/xdos-kernel/README.md` only as needed.
6. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m12-xdos-boot-clone-conditions-retry`
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
- explicit note stating which clone/boot conditions are proven, disproven, image-observed, tool-observed, or unknown

## Advancement Rule
- This retry is allowed without new user approval because it is the same failed task
