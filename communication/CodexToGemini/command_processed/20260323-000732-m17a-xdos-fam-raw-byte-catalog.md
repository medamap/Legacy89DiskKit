# Gemini Implementation Instruction

## Task ID
20260323-000732-m17a-xdos-fam-raw-byte-catalog

## Objective
Build a primary-evidence catalog that lines up representative X-DOS files with their directory entry bytes, `0x1D/0x1E` pair, first observed placement, and raw FAM-area bytes, without assigning bit-level semantics yet.

## Task Kind
analysis-only

## Slice Rule
This task is intentionally narrow. Do not recover FAM meanings, do not prove shared placement, and do not reconstruct write semantics here. Only collect and normalize raw observations into tracked analysis artifacts so later tasks can reason from the same byte evidence.

## Branch
- Base: `develop`
- Name: `codex/m17a-xdos-fam-raw-byte-catalog`
- Gemini may commit on this branch if the instruction requires implementation
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/find_file_start.py`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use evidence for every claim
- Mark uncertainty as `unknown`
- This is not a copy task; no encoding policy work is in scope
- Intended layer ownership: `Infrastructure` is not in scope; this is analysis-only
- Do not resume implementation work
- Do not edit C# production code
- Do not assign semantics to FAM bytes in this task
- Do not use convenience terms like “cluster semantics proven” unless directly demonstrated
- Keep the deliverable to tracked analysis artifacts only

## Steps
1. Create or reuse one tracked helper under `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/` if needed, but only if it is necessary to extract raw observations reproducibly.
2. Select a small representative set of files across the two disks:
   - at least one small file
   - at least one larger file
   - at least one file that exists on both disks
   - prefer files already referenced in prior notes, such as `SX-BASIC`, `Overlay module`, or core system files when observable
3. For each sampled file, collect only these observations:
   - disk name
   - filename
   - directory entry base offset
   - directory bytes `0x1A` through `0x1E`
   - `0x1D/0x1E` pair
   - first observed placement pair from the tracked helper or direct raw observation
   - raw FAM-area byte window that is plausibly associated with the file, with exact disk offsets and byte values
4. Record the evidence in tracked analysis files. Preferred targets:
   - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
   - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
5. If a helper is added, keep it single-purpose and tracked. Do not leave temp scripts or untracked evidence helpers behind.
6. In the notes, separate:
   - direct observation
   - stable cross-disk patterns
   - unknown
7. Stop before any bit-level interpretation. The acceptable end state is a byte catalog, not a semantic model.

## Verification
- `git status --short`
- `python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/find_file_start.py --help`
- If a new helper is added, run it on at least one sampled file from each disk and include the command lines in the report

## Acceptance
- A tracked artifact exists that lists sampled files with raw directory bytes, first observed placement, and raw FAM-area bytes
- The artifact uses exact offsets and byte values, not paraphrased summaries alone
- No FAM bit-level semantics are claimed
- No untracked temp helper is required to reproduce the observations

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
