# Gemini Implementation Instruction

## Task ID
20260324-032200-m30a-xdos-write-chain-semantic-proof-after-c-provenance

## Objective
Re-evaluate the write-side semantic proof boundary after the accepted `0xC9EA` C-register provenance evidence and determine whether any write-side chain role can now be upgraded from `unknown` using repository-local evidence only.

## Branch
- Base: `develop`
- Name: `codex/m30a-xdos-write-chain-semantic-proof-after-c-provenance`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Required Inputs
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md

## Files To Read First
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use repository-local evidence only
- Mark uncertainty as `unknown`
- Do not use external hardware knowledge, external docs, or non-repo skill output as proof
- `xdos-semantics-engine` may be used only as a raw annotation helper and cannot by itself justify a semantic upgrade
- Limit changes to:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Steps
1. Read the currently accepted write-side evidence sections, including:
   - `Write-Side Chain Semantic Boundary (Analysis-Only)`
   - `Write Mutation Semantic Proof Attempt (Analysis-Only)`
   - `Write Mutation Semantic Proof Attempt Retry 2 (Analysis-Only)`
   - `Write-Side FDC I/O Catalog (Analysis-Only)`
   - `C9EA C-Register Provenance Catalog (Analysis-Only)`
2. Re-evaluate whether the accepted raw evidence now justifies any upgrade from `unknown` to `provisional` for:
   - `helper_c934` and `helper_c938`
   - `0xC9EA`
   - `0xCABA`
3. If no upgrade is justified, record that explicitly and conservatively.
4. Add one new section to `boot_and_io_notes.md`:
   - `## Write Mutation Semantic Proof Attempt After C Provenance (Analysis-Only)`
   - with columns:
     - `semantic concern`
     - `current evidence grade`
     - `current boundary`
5. Update `README.md` with one preserving-append sentence noting that this proof-attempt section now exists.

## Verification
- `git diff -- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

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
- state explicitly whether any role was upgraded or whether all roles remained `unknown`

## User-Facing Handoff Block Rule
- If Codex also returns a copyable message for the user to forward to Gemini, do not nest code blocks inside that message
- Show commands as plain text list items instead
