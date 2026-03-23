# Gemini Task Report

## Task ID
20260321-125154-m9-xdos-read-helper-windows

## Instruction Filename
20260321-125154-m9-xdos-read-helper-windows.md

## Branch Name
codex/m9-xdos-read-helper-windows

## Summary
Successfully extended the X-DOS read-path analysis by extracting and documenting direct byte windows for four key helper routines (`0xC934`, `0xC97E`, `0xC9BC`, `0xD6AF`) reached from the confirmed syscall entrypoints. Using the established `FileOffset = MemoryAddr - 0x7165` mapping, physical file offsets in `XDOS_SYS.D88` were verified, and the resulting Z80 instruction sequences were analyzed for their roles in the filesystem read/write lifecycle.

## Changed Files
- `analysis/xdos-kernel/read_path.asm`: Added extracted byte windows and initial disassembly for `helper_c934`, `helper_c97e`, `helper_c9bc`, and `helper_d6af`.
- `analysis/xdos-kernel/labels.tsv`: Added labels and descriptive notes for the new helper routines.
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added a dedicated "Read-Path Helper Routines" section summarizing the observed roles of each helper.

## Commands
```bash
# Extract bytes for helpers using hex offsets
od -t x1 -j 0x57CF -N 32 images/disk_org/x1/XDOS_SYS.D88 # 0xC934
od -t x1 -j 0x5819 -N 32 images/disk_org/x1/XDOS_SYS.D88 # 0xC97E
od -t x1 -j 0x5857 -N 32 images/disk_org/x1/XDOS_SYS.D88 # 0xC9BC
od -t x1 -j 0x654A -N 32 images/disk_org/x1/XDOS_SYS.D88 # 0xD6AF
```

## Evidence
The following helper routines were directly observed and analyzed:
- **`helper_c934`** (`0xC934`): Offset `0x57CF`. Bytes: `02 38 0D 0F 0F 0F 0F 4F 1A 13 CD EA C9 ...`. Appears to perform nibble swapping (`rrca` x4) and delegates to `0xC9EA`.
- **`helper_c97e`** (`0xC97E`): Offset `0x5819`. Bytes: `78 C1 B7 E1 C9 ...`. Handles register cleanup (`pop bc`, `pop hl`) for `sys_wopen`.
- **`helper_c9bc`** (`0xC9BC`): Offset `0x5857`. Bytes: `3E 50 CD 32 EB E1 D1 C1 C9 ...`. Sets up device I/O parameters (likely device/command `0x50`) via `0xEB32`.
- **`helper_d6af`** (`0xD6AF`): Offset `0x654A`. Bytes: `1B 1B CD 55 D1 CD 0E E0 D8 3E 08 37 C0 ...`. The primary implementation delegate for `sys_rdd`, performing cluster/sector calculations and low-level calls to `0xD155` and `0xE00E`.

## Risks
- **Far Delegation**: The primary read logic for `sys_rdd` is delegated to `0xD6AF`, which is physically located ~3.5KB away from the main implementation block. This confirms the kernel code is distributed across multiple logical records or memory blocks.
- **Side-Effect Return Patterns**: Consistent with `sys_file`, some helpers and syscalls (like `sys_wrd` and `sys_wopen`) use complex delegation chains that may involve modifying the stack or return addresses.

## Requested Review
Verify if the `0x50` value seen in `helper_c9bc` aligns with known X1 disk device identifiers or X-DOS command codes.

## Contradictions
None; the mapping remains robust across the `0xC800`, `0xC900`, and `0xD600` regions.

## Provisional Conclusions
The X-DOS kernel employs a modular design where syscall entrypoints (around `0xC800`) perform high-level validation before delegating to common helpers (around `0xC900`) and specialized "heavy lifter" routines (like `0xD6AF` for reads). The role of `0xD6AF` as the primary read delegate is a critical finding for implementing boot-compatible 2D clones.

## Unknown
- The specific meaning of the `0xC9EA`, `0xEB32`, `0xD155`, and `0xE00E` targets reached by these helpers.
- The exact hardware or logical device associated with the `0x50` ID in `sys_devi`.

## Notes
- Unrelated local changes (including `communication/` and `csharp/TestCylinders/`) were not reset or cleaned.
- Helper windows for `0xC934`, `0xC97E`, `0xC9BC`, and `0xD6AF` were directly observed and documented.
