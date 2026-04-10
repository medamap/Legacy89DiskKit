# Gemini Implementation Instruction

## Task ID
20260323-004139-m17b-xdos-fam-window-patterns

## Objective
Build a pattern table for raw FAM-area windows across representative files and across `XDOS_SYS.D88` / `XDOSUTIL.D88`, without assigning any bit-level semantics yet.

## Task Kind
analysis-only

## Slice Rule
This task stays one step before bit-level decoding. Do not recover packed meanings. Only compare the raw FAM windows already cataloged, expand them where necessary, and classify the observed relationships as same / different / repeated / unknown.

## Branch
- Base: `develop`
- Name: `codex/m17b-xdos-fam-window-patterns`
- Gemini may commit on this branch if the instruction requires implementation
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/collect_raw_catalog.py`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/dump_fam.py`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use evidence for every claim
- Mark uncertainty as `unknown`
- Do not edit C# production code
- Do not resume implementation work
- Do not add new helper scripts unless absolutely necessary
- Prefer reusing existing tracked helpers from `analysis/xdos-kernel/`
- Do not claim:
  - nibble meaning
  - bit packing meaning
  - allocation chain meaning
  - traversal semantics
- Allowed relationship labels are limited to:
  - `same`
  - `different`
  - `repeated`
  - `unknown`

## Steps
1. Start from the existing raw-byte catalog in `boot_and_io_notes.md`.
2. For the sampled files already present there, compare raw FAM windows:
   - same filename across `XDOS_SYS.D88` and `XDOSUTIL.D88`
   - different filenames within the same disk
3. If the current window size is too small to classify a pattern, widen the raw FAM window conservatively, but keep exact offsets and bytes.
4. Add a new section to `boot_and_io_notes.md` that summarizes only raw pattern relationships:
   - cross-disk same file: same/different
   - intra-disk neighboring files: same/different/repeated
   - obvious byte reuse motifs: repeated/unknown
5. Update `README.md` only if the critical unknowns need to be tightened or clarified based on these raw patterns.
6. Keep all conclusions below the semantic threshold. No bit-level interpretation is allowed in this task.

## Verification
- `git status --short`
- `python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/collect_raw_catalog.py`
- `python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/dump_fam.py`

## Acceptance
- A tracked notes section exists that compares raw FAM windows using only same/different/repeated/unknown
- Exact offsets and byte windows are preserved
- No bit-level or chain semantics are claimed
- No new temp helper is required

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

## User-Facing Handoff Block Rule
- If Codex also returns a copyable message for the user to forward to Gemini, do not nest code blocks inside that message
- Show commands as plain text list items instead
