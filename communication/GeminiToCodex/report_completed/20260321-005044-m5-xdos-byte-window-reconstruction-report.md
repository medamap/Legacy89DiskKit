# Task Report: X-DOS Byte Window Reconstruction (Retry)

## Task ID
20260321-004249-m5-xdos-byte-window-reconstruction

## Instruction Filename
20260321-005044-m5-xdos-byte-window-reconstruction-retry.md

## Branch Name
`codex/m5-xdos-byte-window-reconstruction`

## Summary
Successfully corrected the X-DOS kernel byte-window reconstruction and reconciled FDC evidence classes. The FDC status wait loop branch target was recalculated and updated with precise offset notation, confirming the target is the `in a, (c)` instruction rather than the window start. FDC evidence grades were standardized across `labels.tsv` and `boot_and_io_notes.md`, with `0x0FF8` promoted to `confirmed` based on explicit byte evidence, while `0x0FFC` was demoted to `probable` to maintain a conservative stance.

## Changed Files
- `analysis/xdos-kernel/read_path.asm`: Corrected `fdc_wait_loop` branch comment to `jr c, -5` targeting `in a, (c)`.
- `analysis/xdos-kernel/labels.tsv`: Promoted `0x0FF8` to `confirmed`, set `0x0FFC` to `probable`, and updated sources.
- `analysis/xdos-kernel/boot_and_io_notes.md`: Reconciled FDC port usage table to match `labels.tsv` and improved description for `0FFCH`.

## Commands
```bash
# Branch was already checked out
git add analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/labels.tsv analysis/xdos-kernel/read_path.asm
git commit -m "docs(analysis): correct FDC wait loop branch and reconcile evidence classes"
```

## Evidence

### Corrected `fdc_wait_loop` snippet (`read_path.asm`)
```asm
fdc_wait_loop:
    db 0x01, 0xF8, 0x0F ; ld bc, 0x0FF8 (fdc_status_cmd)
    db 0xED, 0x78       ; in a, (c)
    db 0x0F             ; rrca
    db 0x38, 0xFB       ; jr c, -5 (Wait for Busy bit 0 to clear, targets: in a, (c))
```
- **Calculation**: `38 FB` is `jr c, -5`. Relative to the instruction after the branch (offset 8), `-5` points to offset 3 (`ED 78` / `in a, (c)`).

### Final FDC-related rows in `labels.tsv`
```tsv
0x0FF8	fdc_status_cmd	confirmed	XDOSUTIL.D88	MB8877A Status (R) / Command (W) (Seen in 01 F8 0F)
0x0FF9	fdc_track	probable	x1_io_ports_reference.md	FDC Track Register
0x0FFA	fdc_sector	probable	x1_io_ports_reference.md	FDC Sector Register
0x0FFB	fdc_data	probable	x1_io_ports_reference.md	FDC Data Register
0x0FFC	fdc_control	probable	XDOSUTIL.D88	Drive Select, Side Select (bit 4), Motor latch (Seen as 0E FC)
```

### Final FDC section in `boot_and_io_notes.md`
| Port Address | Label | Usage in X-DOS | Description |
| :--- | :--- | :--- | :--- |
| `0FF8H` | `fdc_status_cmd` | Confirmed | MB8877A Status (R) / Command (W) (Seen in 01 F8 0F) |
| `0FF9H` | `fdc_track` | Probable | FDC Track Register |
| `0FFAH` | `fdc_sector` | Probable | FDC Sector Register |
| `0FFBH` | `fdc_data` | Probable | FDC Data Register |
| `0FFCH` | `fdc_control` | Probable | Drive Select, Side Select (bit 4), Motor latch (Seen as 0E FC) |

## Risks
- **Side Latch Port Range**: While `0FFCH` is highly likely given the `0E FC` pattern and standard hardware knowledge, it remains marked as `probable` because the leading byte for `B` was not seen in the immediate window, maintaining the requested conservative discipline.

## Requested Review
- Verify the branch offset calculation (`-5` from `38 FB`).
- Confirm that the evidence grading in `labels.tsv` meets the conservative threshold requested.
