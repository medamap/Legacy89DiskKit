---
name: opencode-sync-runner
description: Run OpenCode synchronously against one instruction file, save stdout and stderr to a temporary log file, and return a compact JSON result with exit status, artifact paths, and webhook notification status.
---

# OpenCode Sync Runner

Use this skill when you want to run OpenCode in a strictly synchronous way without streaming its full stdout into the current model context.

## What it does

- Executes `opencode run --format <default|json> --model <provider/model> --file <instruction>` once
- Waits for completion
- Redirects OpenCode stdout and stderr into a temporary log file
- Returns a compact JSON result to stdout containing:
  - `status`
  - `exit_code`
  - `instruction_path`
  - `log_path`
  - `report_path`
  - `report_exists`
  - `cwd`
  - timestamps
  - webhook notification status

## Script

- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/opencode-sync-runner/scripts/run_opencode_sync.py`

## Recommended usage

```bash
python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/opencode-sync-runner/scripts/run_opencode_sync.py \
  --instruction /absolute/path/to/instruction.md \
  --cwd /absolute/path/to/repo \
  --model opencode-go/minimax-m2.5 \
  --opencode-format json \
  --timeout-seconds 3600 \
  --json-indent 2
```

## Notes

- The script does not parse OpenCode's stdout for meaning. It only stores it in the temp log and returns paths.
- `report_path` is inferred from the Legacy89DiskKit queue naming convention used for OpenCode handoffs.
- If OpenCode times out, the script returns `status = "timed_out"` and exit code `124`.
- If the instruction file is missing, the script returns `status = "invalid_instruction"` and exit code `2`.
- On macOS, `--notify-voice` uses `say` to speak a short completion or failure notification after the OpenCode process ends.
- If `MOSHI_WEBHOOK_TOKEN` is set, the script posts a completion notification to `MOSHI_WEBHOOK_URL` or the default Moshi webhook endpoint using `curl`.
- Optional environment overrides: `MOSHI_WEBHOOK_URL`, `MOSHI_WEBHOOK_TITLE`, `MOSHI_WEBHOOK_IMAGE`.
