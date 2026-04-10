# Gemini Work Report

## Task ID
20260321-143641-m13b1-xdos-helper-input-pair

## Instruction
20260321-143641-m13b1-xdos-helper-input-pair.md

## Branch Name
codex/m13b1-xdos-helper-input-pair

## Summary
Successfully proved that X-DOS directory bytes at indices `0x1D` (29) and `0x1E` (30) are consumed as a 16-bit pair by the `helper_d6af` kernel routine. Documented the exact instruction sequence and execution flow in the analysis assets without assigning any further semantics.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added evidence-graded section for `0x1D/0x1E` pair consumption.
- `analysis/xdos-kernel/README.md`: Updated "Critical Unknowns" with the proven observation.

## Commands
- `git checkout -b codex/m13b1-xdos-helper-input-pair develop`
- `git add analysis/xdos-kernel/README.md analysis/xdos-kernel/boot_and_io_notes.md`
- `git commit -m "docs(xdos): prove directory bytes 0x1D/0x1E consumption by helper_d6af"`

## Evidence
- **`read_path.asm:154-158`**:
  ```asm
  db 0x11, 0x1D, 0x00 ; ld de, 0x001D
  db 0x19             ; add hl, de
  db 0x56             ; ld d, (hl)
  db 0x23             ; inc hl
  db 0x5E             ; ld e, (hl)
  ```
  This proves `D` is loaded from index `0x1D` and `E` from index `0x1E` relative to `HL` (directory entry base).
- **Sub-call**: The `DE` pair is immediately passed to `call 0xDEE8` (Traversal stage).

## Risks
- **Upstream HL source**: While the consumption is proven, the code that sets up `HL` before calling `helper_d6af` (upstream of `sys_rdd`) is not yet fully reconstructed.

## Requested Review
- Please verify the assembly-to-offset mapping (`0x1D` = 29, `0x1E` = 30) matches the confirmed 32-byte directory entry boundary.

## Contradictions
- None.

## Provisional Conclusions
- Directory bytes `0x1D` and `0x1E` are treated as a single 16-bit functional unit by the kernel's read engine.

## Unknown
- The specific bit-level semantics (e.g., Track/Sector/FAM/Cluster) of the `0x1D/0x1E` pair.

## Explicit Notes
- **Unrelated local changes were not reset or cleaned.**
- **No field semantics beyond "consumed as a pair" were assigned to the `0x1D/0x1E` pair.**
