---
name: gemini-command-worker
description: Process queued Markdown instructions from `communication/CodexToGemini/command_waiting/`, move them through `command_processing/` and `command_processed/`, perform the requested repository work, and submit a structured Markdown report to `communication/GeminiToCodex/report_waiting/`. Use when Gemini is the implementation or investigation worker for this repository.
---

# Gemini Command Worker

Pick up one queued instruction, execute it, then publish a structured report.

## Workflow

1. Read:
   - [communication/communication_rule.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md)
   - [communication/legacy89_context.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md)
   - [communication/verification_baseline.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md)
2. Take one file from `communication/CodexToGemini/command_waiting/`.
3. Move it to `communication/CodexToGemini/command_processing/` before doing any work.
4. Execute only the instructed task.
5. When done:
   - move the instruction to `communication/CodexToGemini/command_processed/`
   - write a Markdown report to `communication/GeminiToCodex/report_waiting/`

## Report Rules

- Use the template in [references/report-template.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/gemini-command-worker/references/report-template.md)
- Include:
  - branch_name
  - summary
  - changed_files
  - commands
  - evidence
  - risks
  - requested_review
- Identify the instruction filename and task id
- If blocked, say exactly where and why

## Safety Rules

- Do not claim completion without code or command evidence
- Do not change unrelated files
- Keep C# and 2D priorities ahead of later backlog items unless the instruction says otherwise
- For implementation tasks, create the instructed `codex/` branch from `develop`
- Do not merge to `develop`

## References

- Use [references/report-template.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/gemini-command-worker/references/report-template.md) for output shape
- Use [references/queue-state-rules.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/gemini-command-worker/references/queue-state-rules.md) for file moves
