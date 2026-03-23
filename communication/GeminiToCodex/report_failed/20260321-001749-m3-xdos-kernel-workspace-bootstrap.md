# Gemini Work Report

## Task ID
20260321-001749-m3-xdos-kernel-workspace-bootstrap

## Instruction
20260321-001749-m3-xdos-kernel-workspace-bootstrap.md

## Branch Name
`codex/m3-xdos-kernel-workspace-bootstrap`

## Summary
Created a repo-tracked X-DOS kernel analysis workspace with initial assembly reconstruction and label seeding. This bootstrap focuses on filesystem-access relevant kernel areas (syscalls, buffer areas, and side-selection logic) grounded in primary evidence from `XDOS_SYS.D88` and `XDOSUTIL.D88`.

## Changed Files
- `analysis/xdos-kernel/README.md`
- `analysis/xdos-kernel/labels.tsv`
- `analysis/xdos-kernel/read_path.asm`
- `analysis/xdos-kernel/boot_and_io_notes.md`

## Commands
- `mkdir -p analysis/xdos-kernel`
- `git checkout -b codex/m3-xdos-kernel-workspace-bootstrap`
- `git add analysis/xdos-kernel/`
- `git commit -m "feat: bootstrap X-DOS kernel analysis workspace"`

## Evidence
- `analysis/xdos-kernel/labels.tsv`: Contains 17 seeded labels including `sys_file`, `sys_ropen`, and `fat_area` with confirmed evidence classes.
- `analysis/xdos-kernel/read_path.asm`: Reconstructs the `EE 10` side-select logic found at C1, H1, R8 on `XDOSUTIL.D88`.
- `analysis/xdos-kernel/boot_and_io_notes.md`: Documents the mapping formula for logical records >= 10 (`physical_track = (rec - 10) / 10 + 1`).

## Risks
- **Address Hypotheses**: The `kernel_base` address (`0xC800`) remains `probable` and requires further verification from boot-sector analysis.
- **Mapping Gaps**: The exact mapping of logical records 0-9 to Track 0 physical sectors remains unresolved.

## Requested Review
- Please verify the syscall addresses in `labels.tsv` against any available external documentation or additional salvaged headers.
- Confirm if the `EE 10` side-select pattern has been observed in other kernel versions or images.
