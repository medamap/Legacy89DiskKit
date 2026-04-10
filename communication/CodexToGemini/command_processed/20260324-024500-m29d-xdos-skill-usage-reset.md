# Gemini Implementation Instruction

## Task ID
20260324-024500-m29d-xdos-skill-usage-reset

## Objective
Refresh Gemini's local operating assumptions for `xdos-semantics-engine` and future X-DOS analysis tasks so that the skill is used only as a raw annotation helper unless a task explicitly cites accepted repository-local proof for a stronger semantic claim.

## Branch
- Base: `develop`
- Name: `codex/m29d-xdos-skill-usage-reset`
- Gemini may create the branch, but this task should not edit source code or analysis files
- Gemini must not merge to `develop`

## Required Inputs
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/xdos-semantics-engine/SKILL.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/xdos-semantics-engine/scripts/xdos_analyze_window.py
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/xdos-semantics-engine/scripts/z80_disasm_core.py
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/gemini-command-author/SKILL.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/gemini-command-author/references/instruction-template.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md

## Files To Read First
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/xdos-semantics-engine/SKILL.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/gemini-command-author/SKILL.md

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Evidence-based claims only
- Repo-local evidence only for semantic upgrades
- Treat `xdos-semantics-engine` as a raw annotation helper only
- Hardware-port matches, address-range hints, and helper-skill output alone are not sufficient to upgrade `unknown` to `provisional`
- If uncertainty remains, keep the grade at `unknown`
- Do not modify repository files in this task

## Steps
1. Read the updated skill and context files listed above.
2. Confirm you understand the new operating rule: `xdos-semantics-engine` may annotate windows, ports, and immediate values, but it cannot by itself justify a semantic upgrade.
3. Confirm you understand the review rule: stronger semantic claims require accepted repository-local proof already present in the repo or explicitly cited by a future task.
4. Write a Markdown acknowledgment report only.

## Verification
- No build or test commands are required.
- Confirm in the report that no repository files were modified.

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
- Explicit acknowledgment that future uses of `xdos-semantics-engine` will remain conservative unless a task explicitly authorizes a stronger repo-local proof chain

## User-Facing Handoff Block Rule
- If Codex also returns a copyable message for the user to forward to Gemini, do not nest code blocks inside that message
- Show commands as plain text list items instead
