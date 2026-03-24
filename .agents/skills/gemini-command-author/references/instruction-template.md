# Gemini Instruction Template

```md
# Gemini Implementation Instruction

## Task ID
[timestamp-task-id]

## Objective
[one concrete goal]

## Branch
- Base: `develop`
- Name: `codex/[task-id]`
- Gemini may commit on this branch if the instruction requires implementation
- Gemini must not merge to `develop`

## Required Inputs
- [path or sample]

## Files To Read First
- [absolute path]

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use evidence for every implemented claim
- Mark uncertainty as `unknown`
- Use only repository-local evidence for semantic upgrades
- If `xdos-semantics-engine` is used, treat it as a raw annotation helper only unless the instruction explicitly cites an accepted repo-local proof

## Steps
1. [step]
2. [step]

## Verification
- [command]

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

## User-Facing Handoff Block Rule
- If Codex also returns a copyable message for the user to forward to Gemini, do not nest code blocks inside that message
- Show commands as plain text list items instead
```
