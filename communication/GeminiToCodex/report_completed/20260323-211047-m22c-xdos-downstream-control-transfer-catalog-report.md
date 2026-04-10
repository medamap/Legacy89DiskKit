# Gemini Task Report

## Task ID
20260323-211047-m22c-xdos-downstream-control-transfer-catalog-retry

## Instruction Filename
20260323-211047-m22c-xdos-downstream-control-transfer-catalog-retry.md

## Branch Name
codex/m22c-xdos-downstream-control-transfer-catalog-retry

## Summary
Cataloged control transfers for the four downstream target windows identified in `helper_d6af`. Appended the analysis table to `boot_and_io_notes.md` and added transfer notes to `read_path.asm` while preserving all existing literal observations.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

## Commands
- `printf ... >> analysis/xdos-kernel/boot_and_io_notes.md`
- `replace` in `analysis/xdos-kernel/read_path.asm`
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Evidence
### Downstream Target Control Transfers (Analysis-Only) in `boot_and_io_notes.md`
| target | observed transfer | evidence class | neutral note |
| :--- | :--- | :--- | :--- |
| `0xD155` | `ret` | confirmed | window ends with C9 |
| `0xD753` | `none` | confirmed | no transfer in 9-byte window |
| `0xDEE8` | `none` | confirmed | no transfer in 10-byte window |
| `0xE00E` | `jr c` | confirmed | 38 72 at window offset 2 |

### `read_path.asm` Fragment
```asm
org 0xD155
; Target window cataloged from helper_d6af sub-call
    db 0x04, 0x42, 0x0E, 0x00, 0xC9 ; literal: 0x00; transfer: ret

...

org 0xD753
; Target window cataloged from helper_d6af final jump
    db 0x40, 0x20, 0x0D, 0x13, 0xCD, 0xB5, 0xD1, 0x3E, 0x01 ; literals: 0x0D, 0xD1B5, 0x01; transfer: none

org 0xDEE8
; Target window cataloged from helper_d6af sub-call after 1D/1E load
    db 0x01, 0x40, 0x01, 0x11, 0xA8, 0x00, 0x21, 0x00, 0xEE, 0x19 ; literals: 0x0140, 0x00A8, 0xEE00; transfer: none

org 0xE00E
; Target window cataloged from helper_d6af sub-call
    db 0xEB, 0xDF, 0x38, 0x72, 0x06 ; literal: 0x72; transfer: jr c
```

## Risks
None. The changes were strictly append-only or comment additions that preserved existing data.

## Requested Review
Verify that the `jr c` (38 72) observation at `0xE00E` and the `ret` (C9) at `0xD155` are correctly cataloged and aligned with the window offsets.
