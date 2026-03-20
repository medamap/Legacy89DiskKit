---
name: gemini-report-triage
description: Review pending Gemini reports in `communication/GeminiToCodex/report_waiting/`, decide pass or fail from the evidence, move the report to the correct archive folder, and queue a retry instruction in `communication/CodexToGemini/command_waiting/` when the work is incomplete or incorrect. Use when Codex is auditing Gemini output for this repository.
---

# Gemini Report Triage

Inspect each pending report, decide whether it proves the requested task, then route it.

## Workflow

1. Read the governing rules:
   - [communication/communication_rule.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md)
   - [communication/verification_baseline.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md)
2. Read each file in `communication/GeminiToCodex/report_waiting/`.
3. Evaluate the report using [references/report-review-checklist.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/gemini-report-triage/references/report-review-checklist.md).
4. If accepted, move the report to `communication/GeminiToCodex/report_completed/`.
5. If rejected:
   - move the report to `communication/GeminiToCodex/report_failed/`
   - create a new Markdown retry instruction in `communication/CodexToGemini/command_waiting/`

## Pass Criteria

- The report names the target instruction or task id
- The report includes the working branch name when the task involved implementation
- The claimed edits are supported by file references or command output
- Verification commands were actually run when required
- Residual risks are stated
- The task completion matches the original instruction

## Fail Criteria

- Missing evidence
- Missing verification
- Scope drift
- Claims contradicted by the repo state
- Partial completion without an explicit limitation

## References

- Use [references/report-review-checklist.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/gemini-report-triage/references/report-review-checklist.md) to decide pass or fail
- Use [references/retry-instruction-template.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/gemini-report-triage/references/retry-instruction-template.md) when retrying work
