#!/usr/bin/env python3

import argparse
import json
import os
import shutil
import subprocess
import sys
import time
from dataclasses import dataclass
from datetime import datetime, timedelta
from pathlib import Path
from typing import List, Optional, Tuple


REPO_ROOT = Path(__file__).resolve().parent.parent
COMM_DIR = REPO_ROOT / "communication"
CMD_WAITING = COMM_DIR / "CodexToGemini" / "command_waiting"
RPT_WAITING = COMM_DIR / "GeminiToCodex" / "report_waiting"
STATE_FILE = COMM_DIR / "auto_agent_state.json"
STOP_MARKER = COMM_DIR / "auto_agent_no_loop"
LOG_FILE = COMM_DIR / "auto_agent_loop.log"


@dataclass
class LoopConfig:
    hours: float
    sleep_seconds: float
    max_idle_iterations: int
    prompt_timeout_seconds: int
    once: bool
    dry_run: bool


def now_jst() -> datetime:
    return datetime.now()


def ts_name(dt: datetime) -> str:
    return dt.strftime("%Y%m%d-%H%M%S")


def parse_name_timestamp(path: Path) -> Optional[str]:
    name = path.name
    if len(name) < 15:
        return None
    prefix = name[:15]
    try:
        datetime.strptime(prefix, "%Y%m%d-%H%M%S")
        return prefix
    except ValueError:
        return None


def managed_files(directory: Path, cutoff_ts: str) -> List[Path]:
    directory.mkdir(parents=True, exist_ok=True)
    files = []
    for path in sorted(directory.iterdir()):
        if not path.is_file():
            continue
        file_ts = parse_name_timestamp(path)
        if file_ts and file_ts >= cutoff_ts:
            files.append(path)
    return files


def append_log(message: str) -> None:
    line = f"[{datetime.now().isoformat(timespec='seconds')}] {message}\n"
    LOG_FILE.parent.mkdir(parents=True, exist_ok=True)
    with LOG_FILE.open("a", encoding="utf-8") as fh:
        fh.write(line)


def write_state(payload: dict) -> None:
    STATE_FILE.parent.mkdir(parents=True, exist_ok=True)
    with STATE_FILE.open("w", encoding="utf-8") as fh:
        json.dump(payload, fh, ensure_ascii=False, indent=2)


def snapshot(cutoff_ts: str) -> dict:
    return {
        "commands_waiting": [p.name for p in managed_files(CMD_WAITING, cutoff_ts)],
        "reports_waiting": [p.name for p in managed_files(RPT_WAITING, cutoff_ts)],
    }


def build_prompt(action: str, cutoff_ts: str) -> str:
    common = (
        f"Repository: {REPO_ROOT}\n"
        f"Managed timestamp cutoff: {cutoff_ts} and later only.\n"
        "Use only repository-local evidence.\n"
        "Do not push.\n"
        "Do not touch older queue items below the cutoff timestamp.\n"
    )

    if action == "triage":
        return (
            common
            + "Review exactly one pending managed report in "
            "communication/GeminiToCodex/report_waiting.\n"
            "Use the existing repo workflow used by gemini-report-triage.\n"
            "If accepted, move the report to report_completed.\n"
            "If rejected, move the report to report_failed and create exactly one retry instruction in "
            "communication/CodexToGemini/command_waiting.\n"
            "Do not review any report older than the managed cutoff.\n"
        )

    if action == "work":
        return (
            common
            + "Process exactly one managed instruction in communication/CodexToGemini/command_waiting.\n"
            "Use the existing repo workflow used by gemini-command-worker.\n"
            "Move the instruction to command_processing before work, then to command_processed after writing the report.\n"
            "Write the report to communication/GeminiToCodex/report_waiting.\n"
            "Do not process any instruction older than the managed cutoff.\n"
        )

    return (
        common
        + "Author exactly one next conservative X-DOS analysis instruction.\n"
        "Use the repo workflow used by gemini-command-author.\n"
        "Prioritize the smallest next step that can improve evidence on unresolved themes.\n"
        "Do not upgrade semantic claims without explicit repository-local proof.\n"
        "Write exactly one new instruction file to communication/CodexToGemini/command_waiting.\n"
        f"The new instruction filename must use a timestamp at or after {cutoff_ts}.\n"
    )


def run_gemini(prompt: str, timeout_seconds: int) -> Tuple[int, str]:
    gemini_path = shutil.which("gemini")
    if not gemini_path:
        return 127, "gemini command not found"

    proc = subprocess.run(
        [gemini_path, "-y", "-p", prompt],
        cwd=str(REPO_ROOT),
        text=True,
        capture_output=True,
        timeout=timeout_seconds,
    )
    output = (proc.stdout or "") + ("\n" + proc.stderr if proc.stderr else "")
    return proc.returncode, output


def looks_like_quota_error(text: str) -> bool:
    lowered = text.lower()
    markers = [
        "quota",
        "resource_exhausted",
        "rate limit",
        "429",
        "terminalquotaerror",
        "exhausted",
    ]
    return any(marker in lowered for marker in markers)


def choose_action(cutoff_ts: str) -> str:
    reports = managed_files(RPT_WAITING, cutoff_ts)
    commands = managed_files(CMD_WAITING, cutoff_ts)
    if reports:
        return "triage"
    if commands:
        return "work"
    return "author"


def main() -> int:
    parser = argparse.ArgumentParser(description="Synchronous local Gemini loop for Legacy89DiskKit.")
    parser.add_argument("--hours", type=float, default=8.0)
    parser.add_argument("--sleep-seconds", type=float, default=2.0)
    parser.add_argument("--max-idle-iterations", type=int, default=3)
    parser.add_argument("--prompt-timeout-seconds", type=int, default=1800)
    parser.add_argument("--once", action="store_true")
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    cfg = LoopConfig(
        hours=args.hours,
        sleep_seconds=args.sleep_seconds,
        max_idle_iterations=args.max_idle_iterations,
        prompt_timeout_seconds=args.prompt_timeout_seconds,
        once=args.once,
        dry_run=args.dry_run,
    )

    started_at = now_jst()
    cutoff_ts = ts_name(started_at)
    deadline = started_at + timedelta(hours=cfg.hours)
    idle_iterations = 0
    iteration = 0

    append_log(f"loop-start cutoff={cutoff_ts} deadline={deadline.isoformat(timespec='seconds')}")

    while now_jst() < deadline:
        if STOP_MARKER.exists():
            append_log("loop-stop marker detected")
            write_state(
                {
                    "status": "stopped",
                    "reason": "stop-marker",
                    "started_at": started_at.isoformat(timespec="seconds"),
                    "managed_since": cutoff_ts,
                    "iteration": iteration,
                    **snapshot(cutoff_ts),
                }
            )
            return 0

        iteration += 1
        before = snapshot(cutoff_ts)
        action = choose_action(cutoff_ts)
        prompt = build_prompt(action, cutoff_ts)
        append_log(f"iteration={iteration} action={action} before={before}")

        write_state(
            {
                "status": "running",
                "started_at": started_at.isoformat(timespec="seconds"),
                "managed_since": cutoff_ts,
                "iteration": iteration,
                "last_action": action,
                **before,
            }
        )

        if cfg.dry_run:
            code, output = 0, f"dry-run action={action}"
        else:
            try:
                code, output = run_gemini(prompt, cfg.prompt_timeout_seconds)
            except subprocess.TimeoutExpired:
                append_log(f"iteration={iteration} action={action} timeout")
                write_state(
                    {
                        "status": "stopped",
                        "reason": "timeout",
                        "started_at": started_at.isoformat(timespec="seconds"),
                        "managed_since": cutoff_ts,
                        "iteration": iteration,
                        "last_action": action,
                        **snapshot(cutoff_ts),
                    }
                )
                return 1

        after = snapshot(cutoff_ts)
        output_tail = "\n".join(output.strip().splitlines()[-20:])
        append_log(f"iteration={iteration} action={action} exit={code} after={after}")

        if looks_like_quota_error(output) or code != 0:
            append_log(f"loop-stop quota-or-error detected exit={code}")
            write_state(
                {
                    "status": "stopped",
                    "reason": "quota-or-error",
                    "started_at": started_at.isoformat(timespec="seconds"),
                    "managed_since": cutoff_ts,
                    "iteration": iteration,
                    "last_action": action,
                    "last_exit_code": code,
                    "last_output_tail": output_tail,
                    **after,
                }
            )
            return code or 1

        if before == after:
            idle_iterations += 1
        else:
            idle_iterations = 0

        write_state(
            {
                "status": "running",
                "started_at": started_at.isoformat(timespec="seconds"),
                "managed_since": cutoff_ts,
                "iteration": iteration,
                "last_action": action,
                "last_exit_code": code,
                "last_output_tail": output_tail,
                "idle_iterations": idle_iterations,
                **after,
            }
        )

        if idle_iterations >= cfg.max_idle_iterations:
            append_log(f"loop-stop idle limit reached idle_iterations={idle_iterations}")
            write_state(
                {
                    "status": "stopped",
                    "reason": "idle-limit",
                    "started_at": started_at.isoformat(timespec="seconds"),
                    "managed_since": cutoff_ts,
                    "iteration": iteration,
                    "last_action": action,
                    "last_exit_code": code,
                    "last_output_tail": output_tail,
                    "idle_iterations": idle_iterations,
                    **after,
                }
            )
            return 0

        if cfg.once:
            append_log("loop-stop once flag satisfied")
            write_state(
                {
                    "status": "stopped",
                    "reason": "once",
                    "started_at": started_at.isoformat(timespec="seconds"),
                    "managed_since": cutoff_ts,
                    "iteration": iteration,
                    "last_action": action,
                    "last_exit_code": code,
                    "last_output_tail": output_tail,
                    **after,
                }
            )
            return 0

        time.sleep(cfg.sleep_seconds)

    append_log("loop-stop deadline reached")
    write_state(
        {
            "status": "stopped",
            "reason": "deadline",
            "started_at": started_at.isoformat(timespec="seconds"),
            "managed_since": cutoff_ts,
            "iteration": iteration,
            **snapshot(cutoff_ts),
        }
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
