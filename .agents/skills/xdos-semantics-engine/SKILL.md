# XDOS Semantics Engine Skill (Analysis-Only)

This skill provides automated Z80 disassembly and hardware mapping for X-DOS kernel analysis based on confirmed X1 series hardware specifications.

## Capabilities

1.  **Z80 Disassembly**: Converts raw HEX windows to Z80 mnemonics.
2.  **Hardware Port Mapping**: Automatically annotates `IN`/`OUT` instructions with X1 I/O port names (e.g., `0FF8H` -> `FDC Status/Command`).
3.  **FDC Command Decoding**: Decodes command codes sent to `0FF8H` or `0FE8H` into their FDC command equivalents (e.g., `1EH` -> `Seek with Verify`).
4.  **Memory Region Context**: Identifies target registers or buffers residing in Text VRAM (`3000H`) or Graphic RAM (`4000H-FFFFH`).

## Usage

When the user asks to analyze a memory target, or when you encounter a code window:

1.  Use `xdos_analyze_window.py` (when implemented) or simulate its logic using the provided `x1_metadata.json`.
2.  Assign semantic grades based on the level of interaction:
    *   **Provisional**: Interacts with hardware ports matching expected OS behavior (e.g., FDC I/O for file open).
    *   **Confirmed**: Matches a known OS syscall or routine entry point with proven behavior.

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
