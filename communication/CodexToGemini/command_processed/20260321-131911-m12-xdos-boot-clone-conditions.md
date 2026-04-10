# Gemini Implementation Instruction

## Task ID
20260321-131911-m12-xdos-boot-clone-conditions

## Objective
Finalize the X-DOS kernel reanalysis by producing a conservative boot/clone condition spec for 2D media, clearly separating what is already proven from what remains unknown.

## Branch
- Base: `develop`
- Name: `codex/m12-xdos-boot-clone-conditions`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
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
- Scope is limited to 2D X-DOS boot/clone conditions
- Do not start implementation planning beyond what is needed to state the condition set

## Steps
1. Create branch `codex/m12-xdos-boot-clone-conditions` from `develop`.
2. Re-read the accepted reports and current analysis assets.
3. Produce a final conservative "Boot and Clone Conditions" section in the analysis notes.
4. The spec must explicitly answer:
   - What is proven about X-DOS boot layout on 2D media?
   - What is proven about logical record mapping for FAT / Directory / FAM / bdir?
   - What is proven about read-path dependence on those areas?
   - What is proven about write-path behavior?
   - What is proven vs unknown about why `boot-copy + file cross-copy` failed to yield a correct 2D clone?
   - Whether shared placement / `FirstSectorR`-like behavior is proven, disproven, or still unknown from the current kernel evidence
   - What minimum additional evidence would be needed before implementation can proceed safely
5. Prefer updating `analysis/xdos-kernel/boot_and_io_notes.md` with a dedicated final section.
6. Update `analysis/xdos-kernel/README.md` only if needed to reflect that this reanalysis phase is now decision-complete.
7. Keep conclusions conservative. If the kernel evidence does not settle an issue, mark it `unknown` instead of choosing a side.
8. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m12-xdos-boot-clone-conditions`
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
- explicit note stating which clone/boot conditions are proven, disproven, or unknown

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed to the next step
- This is the final milestone in the current X-DOS reanalysis sequence
