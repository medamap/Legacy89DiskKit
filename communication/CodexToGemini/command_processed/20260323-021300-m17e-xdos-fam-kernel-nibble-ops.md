# Gemini Implementation Instruction

## Task ID
20260323-021300-m17e-xdos-fam-kernel-nibble-ops

## Objective
Determine whether the currently reconstructed X-DOS kernel paths that touch FAM-related data consume those values as full bytes or with explicit nibble-oriented masking/shifting, without assigning semantic meaning.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m17e-xdos-fam-kernel-nibble-ops`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`

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
- Do not edit code outside `analysis/xdos-kernel/boot_and_io_notes.md` and `analysis/xdos-kernel/README.md`
- Do not create temporary helper scripts for this task
- Keep the scope narrow: only classify observed kernel-side byte-vs-nibble handling patterns

## Steps
1. Inspect the currently reconstructed read-path assets and note every observed instruction pattern that appears to consume FAM-related values after they are loaded from memory.
2. Classify only the directly observed operations into categories such as:
   - `byte-consume`
   - `mask-low-nibble`
   - `shift-or-rotate`
   - `unknown`
3. If the current reconstruction does not provide enough evidence for a category, keep it `unknown`.
4. Add one new section to `boot_and_io_notes.md` named `## FAM Kernel-Side Value Handling (Analysis-Only)`.
5. In that section, list only directly observed instruction patterns and their evidence class. Use absolute addresses or existing labels where available.
6. Update `README.md` only by adding one short sentence to the relevant critical-unknown bullet explaining whether kernel-side nibble-oriented handling is directly observed, partially observed, or still unknown.
7. Do not touch unrelated wording in existing sections.

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
- The new section stays at analysis-only wording
- No semantic claims are added
- Any nibble-related claim is backed by a directly observed instruction pattern

## User-Facing Handoff Block Rule
- If Codex also returns a copyable message for the user to forward to Gemini, do not nest code blocks inside that message
- Show commands as plain text list items instead
