#!/usr/bin/env python3
import argparse
import json
import os
import pathlib
import platform
import shutil
import subprocess
import sys
import tempfile
from datetime import datetime, timezone


def build_prompt(instruction_path: pathlib.Path) -> str:
    return f"Read and follow the instruction file exactly at this path: {instruction_path}"


def derive_report_path(instruction_path: pathlib.Path) -> pathlib.Path | None:
    parts = instruction_path.parts
    try:
        idx = parts.index("communication")
    except ValueError:
        return None

    prefix = pathlib.Path(*parts[:idx])
    name = instruction_path.stem
    return prefix / "communication" / "OpencodeToCodex" / "report_waiting" / f"{name}-report.md"


def summarize_instruction(instruction_path: pathlib.Path | None, task_name: str, status: str) -> str:
    if instruction_path is None or not instruction_path.exists():
        return f"OpenCode task {status}: {task_name}"

    try:
        lines = instruction_path.read_text(encoding="utf-8").splitlines()
    except Exception:
        return f"OpenCode task {status}: {task_name}"

    for index, line in enumerate(lines):
        if line.strip().lower() == "## objective":
            for candidate in lines[index + 1:]:
                text = candidate.strip()
                if text:
                    return f"{task_name}: {text}"
            break

    for line in lines:
        text = line.strip()
        if text and not text.startswith("#"):
            return f"{task_name}: {text}"

    return f"OpenCode task {status}: {task_name}"


def try_voice_notify(status: str, instruction_path: pathlib.Path) -> str | None:
    if platform.system() != "Darwin":
        return None

    say_path = shutil.which("say")
    if say_path is None:
        return None

    task_name = instruction_path.stem
    message = {
        "completed": f"OpenCode task completed. {task_name}",
        "failed": f"OpenCode task failed. {task_name}",
        "timed_out": f"OpenCode task timed out. {task_name}",
    }.get(status, f"OpenCode task finished. {task_name}")

    try:
        subprocess.Popen(
            [say_path, message],
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            start_new_session=True,
        )
        return message
    except Exception:
        return None


def default_webhook_title(status: str, task_name: str) -> str:
    return {
        "completed": f"Completed: {task_name}",
        "failed": f"Failed: {task_name}",
        "timed_out": f"Timed out: {task_name}",
    }.get(status, f"Finished: {task_name}")


def try_webhook_notify(status: str, task_name: str, summary: str, report_path: pathlib.Path | None) -> dict:
    token = os.environ.get("MOSHI_WEBHOOK_TOKEN")
    if not token:
        return {"enabled": False, "status": "disabled", "reason": "MOSHI_WEBHOOK_TOKEN is not set"}

    url = os.environ.get("MOSHI_WEBHOOK_URL", "https://api.getmoshi.app/api/webhook")
    title = os.environ.get("MOSHI_WEBHOOK_TITLE") or default_webhook_title(status, task_name)
    image = os.environ.get("MOSHI_WEBHOOK_IMAGE", "")
    message = summary.strip()
    if report_path is not None:
        message = f"{message}\n{report_path}"

    payload = {
        "token": token,
        "title": title,
        "message": message,
    }
    if image:
        payload["image"] = image

    curl_path = shutil.which("curl")
    if curl_path is None:
        return {"enabled": True, "status": "error", "reason": "curl is not available on PATH"}

    try:
        completed = subprocess.run(
            [
                curl_path,
                "-sS",
                "-o",
                "/dev/null",
                "-w",
                "%{http_code}",
                "-X",
                "POST",
                url,
                "-H",
                "Content-Type: application/json",
                "-d",
                json.dumps(payload),
            ],
            check=False,
            capture_output=True,
            text=True,
            timeout=15,
        )
    except Exception as exc:
        return {
            "enabled": True,
            "status": "error",
            "reason": str(exc),
        }

    http_code = completed.stdout.strip()
    if completed.returncode == 0 and http_code.startswith("2"):
        return {
            "enabled": True,
            "status": "sent",
            "http_status": int(http_code),
        }

    return {
        "enabled": True,
        "status": "error",
        "http_status": int(http_code) if http_code.isdigit() else None,
        "reason": completed.stderr.strip() or f"curl exited with {completed.returncode}",
    }


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Run OpenCode synchronously against one instruction file and save stdout/stderr to a temp log."
    )
    parser.add_argument("--instruction", required=True, help="Absolute path to the instruction markdown file.")
    parser.add_argument("--cwd", default=None, help="Working directory for OpenCode. Defaults to the current directory.")
    parser.add_argument("--timeout-seconds", type=int, default=3600, help="Hard timeout in seconds.")
    parser.add_argument("--model", default="opencode-go/minimax-m2.5", help="OpenCode model name.")
    parser.add_argument(
        "--opencode-format",
        choices=("default", "json"),
        default="json",
        help="Output format passed through to `opencode run`.",
    )
    parser.add_argument("--json-indent", type=int, default=None, help="Pretty-print JSON with the given indent.")
    parser.add_argument(
        "--notify-voice",
        action="store_true",
        help="Speak a completion notification on supported systems.",
    )
    args = parser.parse_args()

    instruction_path = pathlib.Path(args.instruction).expanduser().resolve()
    if not instruction_path.exists():
        result = {
            "status": "invalid_instruction",
            "exit_code": None,
            "instruction_path": str(instruction_path),
            "log_path": None,
            "report_path": str(derive_report_path(instruction_path)) if derive_report_path(instruction_path) else None,
            "report_exists": False,
            "cwd": str(pathlib.Path(args.cwd).resolve()) if args.cwd else str(pathlib.Path.cwd()),
            "error": "Instruction file was not found.",
        }
        print(json.dumps(result, indent=args.json_indent))
        return 2

    opencode_path = shutil.which("opencode")
    if opencode_path is None:
        result = {
            "status": "opencode_not_found",
            "exit_code": None,
            "instruction_path": str(instruction_path),
            "log_path": None,
            "report_path": str(derive_report_path(instruction_path)) if derive_report_path(instruction_path) else None,
            "report_exists": False,
            "cwd": str(pathlib.Path(args.cwd).resolve()) if args.cwd else str(pathlib.Path.cwd()),
            "error": "opencode binary was not found on PATH.",
        }
        print(json.dumps(result, indent=args.json_indent))
        return 127

    run_cwd = pathlib.Path(args.cwd).expanduser().resolve() if args.cwd else pathlib.Path.cwd()
    run_cwd.mkdir(parents=True, exist_ok=True)

    temp_dir = pathlib.Path(tempfile.mkdtemp(prefix="opencode-sync-runner-"))
    log_path = temp_dir / "opencode.log"
    report_path = derive_report_path(instruction_path)

    command = [
        opencode_path,
        "run",
        "--model",
        args.model,
        "--format",
        args.opencode_format,
        "--dir",
        str(run_cwd),
        build_prompt(instruction_path),
    ]

    started_at = datetime.now(timezone.utc).isoformat()

    try:
        with log_path.open("wb") as log_file:
            completed = subprocess.run(
                command,
                cwd=str(run_cwd),
                stdout=log_file,
                stderr=subprocess.STDOUT,
                timeout=args.timeout_seconds,
                check=False,
            )
        status = "completed" if completed.returncode == 0 else "failed"
        exit_code = completed.returncode
        timed_out = False
    except subprocess.TimeoutExpired as exc:
        with log_path.open("ab") as log_file:
            log_file.write(b"\n[opencode-sync-runner] Process timed out.\n")
            if exc.stdout:
                log_file.write(exc.stdout)
            if exc.stderr:
                log_file.write(exc.stderr)
        status = "timed_out"
        exit_code = None
        timed_out = True

    result = {
        "status": status,
        "exit_code": exit_code,
        "instruction_path": str(instruction_path),
        "log_path": str(log_path),
        "report_path": str(report_path) if report_path else None,
        "report_exists": report_path.exists() if report_path else False,
        "cwd": str(run_cwd),
        "started_at_utc": started_at,
        "finished_at_utc": datetime.now(timezone.utc).isoformat(),
        "timed_out": timed_out,
        "command": command,
        "model": args.model,
        "opencode_format": args.opencode_format,
    }

    if args.notify_voice:
        result["voice_notification"] = try_voice_notify(status, instruction_path)

    task_name = instruction_path.stem
    webhook_summary = summarize_instruction(instruction_path, task_name, status)
    result["webhook_notification"] = try_webhook_notify(status, task_name, webhook_summary, report_path)

    print(json.dumps(result, indent=args.json_indent))
    if status == "completed":
        return 0
    if status == "timed_out":
        return 124
    return 1


if __name__ == "__main__":
    sys.exit(main())
