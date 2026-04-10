# Communication Rule

This repository uses a Codex to Gemini handoff flow with Markdown instructions and Markdown reports.

## Queue Paths

- Waiting instructions: `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/CodexToGemini/command_waiting`
- Processing instructions: `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/CodexToGemini/command_processing`
- Processed instructions: `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/CodexToGemini/command_processed`
- Waiting reports: `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_waiting`
- Completed reports: `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed`
- Failed reports: `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed`

## File Naming

- Use one Markdown file per task or report
- Use `YYYYMMDD-HHMMSS-task-id.md`
- Keep the same task id across retries

## Instruction Rules

- Each instruction must define one concrete task
- Each instruction must include:
  - task id
  - objective
  - files to read first
  - constraints
  - verification commands
  - report requirements
- Implementation instructions must also define:
  - branch base
  - branch name
  - whether Gemini is allowed to commit
  - that Gemini must not merge to `develop`

## Report Rules

- Each report must include:
  - task id
  - instruction filename
  - branch name
  - summary
  - changed_files
  - commands
  - evidence
  - risks
  - requested_review
- If blocked, say so explicitly
- If uncertain, mark `unknown`

## Review Rules

- Codex reviews every file in `report_waiting`
- Pass:
  - move report to `report_completed`
  - if the task produced a valid branch, Codex may merge it to `develop` with `--no-ff` and push
- Fail:
  - move report to `report_failed`
  - queue a retry instruction

## Branch Rules

- Gemini starts each implementation task from `develop`
- Gemini creates a task-specific branch with the `codex/` prefix
- One instruction maps to one working branch unless the instruction explicitly says otherwise
- Gemini may implement and verify on that branch
- Gemini does not merge to `develop`
- Codex performs the final `develop` merge and push after review passes

## Evidence Rules

- Do not claim implementation without code evidence
- Do not claim verification without command evidence
- Do not rely on todo files for truth
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
