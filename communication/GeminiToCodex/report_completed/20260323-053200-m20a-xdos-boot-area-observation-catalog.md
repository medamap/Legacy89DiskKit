# Gemini Task Report

## Task ID
20260323-053200-m20a-xdos-boot-area-observation-catalog

## Instruction Filename
20260323-053200-m20a-xdos-boot-area-observation-catalog.md

## Branch Name
`codex/m20a-xdos-boot-area-observation-catalog`

## Summary
Cataloged the currently observed boot-area and early management-area facts for the sampled 2D X-DOS disks (`XDOS_SYS.D88` and `XDOSUTIL.D88`). The observations cover the physical locations and raw content of the Volume Record, FAT Area, Directory Area, FAM Area, and the Boot Copy Region (IPL sectors). No boot semantics or rules were assigned to these areas.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added `## Boot And Early-Area Observations (Analysis-Only)` section.
- `analysis/xdos-kernel/README.md`: Updated `Geometry Translation Constraints` to mention the new catalog.

## Commands
```bash
# Verify sector locations and content
python3 -c "..." (custom D88 sector dump script)
# Verify changes
git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md
```

## Evidence
The newly added table in `boot_and_io_notes.md` summarizes the raw observations:

| observed area | sampled disks | directly observed fact | evidence note |
| :--- | :--- | :--- | :--- |
| Volume Record | `XDOS_SYS`, `XDOSUTIL` | Located at Track 0, Head 0, Sector 1 (256 bytes) | Offset 0x01: `58 2d 44 4f 53` ("X-DOS") |
| FAT Area | `XDOS_SYS`, `XDOSUTIL` | Located at Track 0, Head 1, Sector 1 (512 bytes) | Physical R=1 on Head 1 (Logical Rec 10) |
| Directory Area | `XDOS_SYS`, `XDOSUTIL` | Starts at Track 0, Head 1, Sector 2 (512 bytes) | Physical R=2 on Head 1 (Logical Rec 11) |
| FAM Area | `XDOS_SYS`, `XDOSUTIL` | Located at Track 1, Head 0, Sector 1 (512 bytes) | Physical R=1 on Track 1 (Logical Rec 20) |
| Boot Copy Region | `XDOS_SYS`, `XDOSUTIL` | Track 0, Head 0, Sectors 1-10 (Logical Rec 0-9) | Standard 256-byte IPL sector span |

## Risks
None identified. The task was restricted to documentation/analysis updates and followed all "no boot semantics" constraints.

## Requested Review
Verify that the cataloged physical locations (C/H/R) align correctly with the "Logical Record Numbers" mentioned earlier in `boot_and_io_notes.md` (e.g., Logical Rec 10 -> Track 0, Head 1, R=1 assuming 10 sectors per side).

## Contradictions
None observed. The two sampled disks show identical early-area mapping.

## Provisional Conclusions
The physical mapping of management structures (FAT, DIR, FAM) appears strictly consistent across the sampled 2D X-DOS system and utility disks.

## Unknown
- The specific meaning of many bytes within the Volume Record (other than "X-DOS" ID and BCD date) remains unknown.
- The exact size of the directory area (number of sectors) is not yet definitively proven, though the first 512-byte sector (R=2) is confirmed.
