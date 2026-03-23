# Task Report: X-DOS Byte Window Reconstruction

## Task ID
20260321-004249-m5-xdos-byte-window-reconstruction

## Instruction Filename
20260321-004249-m5-xdos-byte-window-reconstruction.md

## Branch Name
`codex/m5-xdos-byte-window-reconstruction`

## Summary
Advanced the X-DOS kernel analysis workspace by reconstructing three evidence-backed byte windows. These windows provide physical anchors for filesystem detection, hardware side-selection, and FDC status handling. Extensive binary inspection of `XDOSUTIL.D88` and `XDOS_SYS.D88` confirmed consistent locations for these patterns across different primary artifacts.

## Changed Files
- `analysis/xdos-kernel/read_path.asm`: Added `volume_record`, `side_select_logic`, and `fdc_wait_loop` byte windows with mnemonics.
- `analysis/xdos-kernel/labels.tsv`: Added `side_latch_shadow` (`0xE691`) and promoted FDC ports to `confirmed`.
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added "Observed Byte Windows" section documenting the source and importance of each reconstructed region.

## Commands
```bash
# Inspection commands used during research
python3 -c '...' # Used for D88 track/sector walking and hex pattern matching
git checkout -b codex/m5-xdos-byte-window-reconstruction develop
git add analysis/xdos-kernel/read_path.asm analysis/xdos-kernel/labels.tsv analysis/xdos-kernel/boot_and_io_notes.md
git commit -m "docs(analysis): reconstruct X-DOS kernel byte windows"
```

## Evidence

### Reconstructed Byte Windows in `read_path.asm`

1.  **Volume Record**:
    - Location: Track 0, Sector 1 (offset 0x10 from sector data start).
    - Pattern: `01 58 2d 44 4f 53 20 20 20 20 20 20 20 20 53 79 73 ... 88 24 04 17`.
    - Significance: Reliable detection of X-DOS filesystem and creation date.

2.  **Side-Select Logic**:
    - Location: Track 2, Sector 8 (D88 offset `0x4bd9`).
    - Pattern: `21 91 E6 7E EE 10 77 E6 10 20 02 14 37 0E FC 7E ED 79`.
    - Significance: Confirms bit 4 side selection and identifies `0xE691` as a kernel state shadow RAM.

3.  **FDC Wait Loop**:
    - Location: Track 2, Sector 8 (D88 offset `0x4b3c`).
    - Pattern: `01 F8 0F ED 78 0F 38 FB`.
    - Significance: Confirms standard polling of MB8877A status port `0x0FF8`.

## Risks
- **Side Latch Shadow**: The address `0xE691` is inferred from the `ld hl, nn` pattern surrounding the side-select toggle. While highly probable, its exact role as a global latch shadow needs further validation during write-path analysis.
- **Interleaved Logic Physical Offset**: The pattern was found in Track 2 Sector 8 on both disks, but its memory address during kernel execution remains speculative (likely in the `0xC800+` range).

## Requested Review
- Verify the assembly mnemonics in `read_path.asm` against the provided raw hex bytes in comments.
- Confirm the `labels.tsv` additions for `side_latch_shadow`.
