---
name: xdos-semantics-engine
description: Provide conservative X-DOS byte-window annotation and hardware-port hinting for repository-local analysis without upgrading semantic certainty on its own.
---

# XDOS Semantics Engine Skill (Analysis-Only)

This skill provides conservative Z80 byte-window annotation for X-DOS kernel analysis based on documented X1 series hardware facts already stored in this repository.

## Capabilities

1.  **Z80 Disassembly Draft**: Converts selected raw byte windows to tentative Z80 mnemonics.
2.  **Hardware Port Mapping**: Annotates directly observed `IN`/`OUT` instructions with X1 I/O port names (e.g., `0FF8H` -> `FDC Status/Command`).
3.  **FDC Command Hinting**: Matches command codes sent to `0FF8H` or `0FE8H` against known MB8877A/FD1791-family bit patterns.
4.  **Memory Region Hints**: Adds documented address-range hints for values like `3000H`, `4000H`, `8000H`, or `C000H`.

## Non-Goals

- Do not use this skill by itself to upgrade a semantic grade from `unknown` to `provisional` or `confirmed`.
- Do not infer runtime purpose from a hardware range match alone.
- Do not treat draft mnemonic output as authoritative when a local window is incomplete or ambiguous.
- Do not introduce facts that are not already recorded in repository-local files.

## Usage

When the user asks to analyze a memory target, or when you encounter a code window:

1.  Use `xdos_analyze_window.py` or `z80_disasm_core.py` only to annotate a raw byte window.
2.  Treat every note from this skill as a hint that still requires repository-local corroboration.
3.  If a report or instruction needs semantic grades:
    *   keep the grade at `unknown` unless an already accepted repository-local proof justifies a stronger grade
    *   state explicitly when a hardware match is only a context hint
    *   prefer `observed`, `cataloged`, or `hint` wording over behavioral claims

## Hardware Reference

Reference: `.agents/skills/xdos-semantics-engine/scripts/x1_metadata.json`

### FDC Port Layout
*   `0FF8`/`0FE8`: Status (IN) / Command (OUT)
*   `0FF9`/`0FE9`: Track
*   `0FFA`/`0FEA`: Sector
*   `0FFB`/`0FEB`: Data
*   `0FFC`/`0FEC`: Drive Control (Side, Drive No, Motor)

### VRAM Regions
*   `2000H - 27FFH`: Text Attribute
*   `3000H - 37FFH`: Text VRAM
*   `4000H - FFFFH`: G-RAM (B/R/G)

## Output Wording Rules

- Prefer phrases like `address-range hint`, `documented port match`, or `draft disassembly`.
- Avoid phrases like `buffer`, `graphics transfer`, `write-path data chain`, or `OS behavior` unless that behavior is already proven elsewhere in the repository.
