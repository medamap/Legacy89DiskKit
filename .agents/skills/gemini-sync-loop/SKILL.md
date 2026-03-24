---
name: gemini-sync-loop
description: Run a synchronous local Gemini loop for this repository that authors one task, executes it, triages the report, and repeats until stopped or quota is exhausted. Use when the user wants unattended Gemini progress while away from the keyboard.
---

# Gemini Sync Loop

Use this skill when the user explicitly wants a local watcher that keeps Gemini moving without manual "finished" messages between iterations.

## What It Does

- Runs on the local machine in this repository.
- Calls `gemini` in headless mode and waits synchronously for each invocation to finish.
- Uses the existing communication folders:
  - `communication/CodexToGemini/command_waiting`
  - `communication/CodexToGemini/command_processing`
  - `communication/CodexToGemini/command_processed`
  - `communication/GeminiToCodex/report_waiting`
  - `communication/GeminiToCodex/report_completed`
  - `communication/GeminiToCodex/report_failed`
- Honors:
  - `communication/auto_agent_no_loop`
  - `communication/auto_agent_state.json`

## Safety Rules

- This loop is local only. It is not a hidden Codex background feature.
- It must not push.
- It must stop on Gemini quota/rate-limit errors.
- It must stop when `communication/auto_agent_no_loop` exists.
- It should manage only work created after the loop starts, so stale backlog is not accidentally reprocessed.

## Runner

- Main runner: `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/scripts/gemini_sync_loop.py`

## Decision Order Per Iteration

1. If a managed report is waiting, triage exactly one report.
2. Else if a managed instruction is waiting, process exactly one instruction.
3. Else author exactly one next conservative instruction.
4. Stop on repeated no-op iterations, explicit stop marker, or quota exhaustion.

## Output Expectations

- Update `communication/auto_agent_state.json`
- Append logs to `communication/auto_agent_loop.log`
- Keep each Gemini invocation single-purpose and synchronous
