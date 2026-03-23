# Gemini Implementation Instruction

## Task ID
20260323-013258-m17c-xdos-fam-byte-nibble-stability-retry

## Objective
Add a concrete new analysis section that classifies sampled raw FAM-window positions at byte/nibble stability level, and make that section the only new deliverable.

## Task Kind
analysis-only

## Slice Rule
This retry must produce a non-empty diff in tracked analysis files. Do not return a report unless `boot_and_io_notes.md` gains a new section named exactly `## FAM Byte And Nibble Stability (Analysis-Only)`.

## Branch
- Base: `develop`
- Name: `codex/m17c-xdos-fam-byte-nibble-stability-retry`
- Gemini may commit on this branch if the instruction requires implementation
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260323-012946-m17c-xdos-fam-byte-nibble-stability.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use evidence for every claim
- Mark uncertainty as `unknown`
- Do not add helper scripts
- Do not edit C# production code
- Do not resume implementation work
- Allowed labels only:
  - `stable-byte`
  - `stable-high-nibble`
  - `stable-low-nibble`
  - `variable`
  - `unknown`
- Do not claim:
  - field meaning
  - packed meaning
  - bit meaning
  - chain meaning
  - allocation meaning
  - traversal meaning

## Steps
1. Append a new section at the end of `boot_and_io_notes.md` with this exact heading:
   - `## FAM Byte And Nibble Stability (Analysis-Only)`
2. In that section, add:
   - one short paragraph saying this is position-stability classification only
   - one compact sample table using the already-sampled windows
   - one compact classification table with rows for:
     - `X-DOS System` cross-disk same-file comparison
     - `same 1D offset 0x06` repeated-window comparison
     - `window positions B0..B7 high nibble`
     - `window positions B0..B7 low nibble`
3. Use only the allowed labels in that classification table.
4. Update `README.md` only if needed, by adding one short sentence to the existing `**FAM Window Pattern Semantics**` bullet:
   - stability can now be described at byte/nibble granularity for the sampled windows
   - semantics remain unknown
5. Do not edit any other section.

## Verification
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Acceptance
- `boot_and_io_notes.md` contains the new exact heading
- The diff is non-empty
- The new section uses only allowed labels
- No semantic interpretation is introduced

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
