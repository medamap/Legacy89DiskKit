# Task Report: X-DOS Syscall Boundary Mapping (Retry)

## Task ID
20260321-083740-m6-xdos-syscall-boundary-mapping

## Instruction Filename
20260321-084159-m6-xdos-syscall-boundary-mapping-retry.md

## Branch Name
`codex/m6-xdos-syscall-boundary-mapping-retry`

## Summary
Successfully refined the X-DOS kernel syscall jump table boundaries and expanded filesystem-relevant labels. The syscall jump table is now represented as a structured 41-entry region (Entry 0 at `0xED78` to Entry 40 at `0xEDF0`), with unknown entries consistently marked as `ds 3` to maintain alignment. Added `defdev` and `bdir_pt` labels to support filesystem analysis. Documented the "Jump Table Convention" in the workspace README to ensure consistent future representation.

**Note on Constraint Compliance**: Explicitly confirmed that no unrelated local changes were reset or cleaned during this task.

## Changed Files
- `analysis/xdos-kernel/read_path.asm`: Consolidated separate syscall `org` blocks into a structured jump table starting at `0xED78`. Added FAM sample bytes to `fam_area`.
- `analysis/xdos-kernel/labels.tsv`: Added `defdev` and `bdir_pt` labels; sorted entries by address for maintainability.
- `analysis/xdos-kernel/README.md`: Added "Jump Table Convention" section.

## Commands
```bash
git checkout develop
git checkout -b codex/m6-xdos-syscall-boundary-mapping-retry
# [Edits to analysis files]
git add analysis/xdos-kernel/read_path.asm analysis/xdos-kernel/labels.tsv analysis/xdos-kernel/README.md
git commit -m "docs(analysis): X-DOSのシステムコールジャンプテーブル境界の修正とラベルの拡充"
```

## Evidence

### Syscall Jump Table Structure (`read_path.asm`)
```asm
; --- Syscall Jump Table (Confirmed from x-dos.h) ---
; Pattern: 3-byte jump table entries (likely 'jp addr').
; Observed range: 0xED78 to 0xEDFF (boundary with fat_area at 0xEE00).

org 0xED78
sys_wopen:  ds 3    ; Entry 0: Open file for write
sys_wrd:    ds 3    ; Entry 1: Write data from memory
...
sys_rdd:    ds 3    ; Entry 3: Read data into memory
...
sys_call:   ds 3    ; Entry 40: Generic OS call dispatcher (DE=entry)
            ds 0xEE00 - $    ; Remainder of table area before fat_area
```
- **Rationale**: Consolidated mapping confirms the 3-byte interval and boundary alignment with `fat_area` (`0xEE00`).

### FAM Sample Bytes (`read_path.asm`)
```asm
fam_area:
    ; Sample bytes from XDOS_SYS.D88 FAM (Track 2, R=1)
    db 0x02, 0x02, 0x09, 0x03, 0x01, 0x0A, 0x04, 0x01
    db 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    ds 512-16
```
- **Primary Evidence**: Confirmed from `XDOS_SYS.D88` (Track 2, R=1) as described in the instruction.

### Label Justifications (`labels.tsv`)
| Address | Label | Source | Rationale |
| :--- | :--- | :--- | :--- |
| `0xED1E` | `defdev` | `make_BGM` | Default drive register, relevant for filesystem selection. |
| `0x7220` | `bdir_pt` | `make_BGM` | Pointer within the system binary area (`bdir_area`). |

## Risks
- **Table Remainder**: The range from `0xEDF3` (end of `sys_call`) to `0xEDFF` contains 13 bytes, which could accommodate 4 more entries plus one padding byte. These remain unmapped.

## Requested Review
- Verify the jump table entry numbering (Entry 0 to 40).
- Confirm the sorting of `labels.tsv` by address.

## Contradictions
- None observed.

## Provisional Conclusions
1. The X-DOS jump table effectively ends at `0xEDFF`, just before the `fat_area`.
2. The kernel variables (`sys_dtadr`, `defdev`, etc.) are clustered just before the jump table.

## Unknown
- The purpose of the 13-byte remainder after `sys_call` at the end of the jump table region.
- The contents of Entry 2 (ED7E), Entry 5 (ED87), etc., which remain unmapped in `x-dos.h`.

## Unrelated Local Changes
- Unrelated uncommitted local changes (e.g., in `communication/CodexToGemini/command_processed/`, etc.) were **not** reset, stashed, or modified.
