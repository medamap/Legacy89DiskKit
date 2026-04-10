# Gemini Implementation Instruction

## Task ID
20260323-022200-m17e-xdos-fam-kernel-nibble-ops-retry

## Objective
Correct the previous overclaim by documenting only kernel-side byte-vs-nibble handling patterns that are directly evidenced as FAM-adjacent in the currently reconstructed assets.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m17e-xdos-fam-kernel-nibble-ops-retry`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Evidence only; mark uncertainty as `unknown`
- Do not assign semantic meaning to any FAM value
- Do not claim traversal, allocation, chaining, or shared-placement meaning
- Do not mention `helper_c934` unless you can cite a direct reconstructed access path from that window to FAM-labeled memory or a FAM-addressed byte source in the same local window
- If such direct evidence is absent, state `write-path nibble handling remains unknown`
- Do not edit code outside `analysis/xdos-kernel/boot_and_io_notes.md` and `analysis/xdos-kernel/README.md`
- Do not create helper scripts

## Steps
1. Re-read the previous failed report and remove any claim that ties generic nibble operations to FAM without direct evidence.
2. Inspect the reconstructed assets and keep only patterns that are directly evidenced as FAM-adjacent.
3. Update `## FAM Kernel-Side Value Handling (Analysis-Only)` in `boot_and_io_notes.md`.
4. In that section, use only these categories:
   - `byte-consume`
   - `mask-low-nibble`
   - `shift-or-rotate`
   - `unknown`
5. If only directory-linked byte loading is directly observed, say so plainly and leave write-path nibble handling as `unknown`.
6. Update `README.md` only by correcting the one sentence in the `FAM Window Pattern Semantics` bullet so it matches the narrowed claim.

## Verification
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

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
- contradictions
- provisional conclusions
- unknown

## Acceptance Criteria
- The diff is limited to `boot_and_io_notes.md` and `README.md`
- No mention of `helper_c934` unless direct FAM-local evidence is shown
- If that evidence is absent, the result explicitly leaves write-path nibble handling as `unknown`
- No semantic claims are added
