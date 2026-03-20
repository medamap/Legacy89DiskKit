---
name: gemini-command-author
description: Create Markdown implementation or retry instructions for Gemini work in this repository and place them in `communication/CodexToGemini/command_waiting/`. Use when Codex needs to hand off a concrete task, include repo-specific constraints, attach file links, and queue the work without doing the implementation locally.
---

# Gemini Command Author

Write one Markdown instruction per task and queue it in `communication/CodexToGemini/command_waiting/`.

## Workflow

1. Read the repo-specific context from:
   - [communication/communication_rule.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md)
   - [communication/legacy89_context.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md)
   - [communication/verification_baseline.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md)
2. If the task touches samples or documents, load:
   - [communication/sample_images.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/sample_images.md)
   - [communication/document_index.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/document_index.md)
3. Draft a single-task instruction using the template in [references/instruction-template.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/gemini-command-author/references/instruction-template.md).
4. Use a unique filename: `YYYYMMDD-HHMMSS-<task-id>.md`.
5. Save only to `communication/CodexToGemini/command_waiting/`.

## Required Content

- Clear task goal and completion criteria
- Exact file paths Gemini may need to read or edit
- Commands Gemini must run for proof
- Expected report contents
- Branch instructions:
  - branch from `develop`
  - use a `codex/` branch name
  - do not merge to `develop`
  - report the branch name back to Codex
- Constraints:
  - C# first
  - 2D first
  - Codex does not implement the task
  - Evidence-based claims only

## Writing Rules

- Keep the instruction decision-complete
- Link directly to important repo files when they matter
- State unknowns explicitly instead of guessing
- Do not combine unrelated work in one instruction
- When generating a user-facing handoff code block for Gemini, never include nested code blocks inside it
- If commands must be shown inside that user-facing code block, render them as plain text bullet items

## References

- Use [references/instruction-template.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/gemini-command-author/references/instruction-template.md) for the instruction body shape
- Use [references/task-id-guidelines.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/gemini-command-author/references/task-id-guidelines.md) for stable task naming
