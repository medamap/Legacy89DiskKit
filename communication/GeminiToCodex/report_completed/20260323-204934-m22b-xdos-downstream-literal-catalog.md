# Gemini Task Report

## Task ID
20260323-204934-m22b-xdos-downstream-literal-catalog

## Instruction Filename
20260323-204934-m22b-xdos-downstream-literal-catalog.md

## Branch Name
codex/m22b-xdos-downstream-literal-catalog

## Summary
Cataloged directly observed literal and immediate values from the downstream target windows (`0xD155`, `0xD753`, `0xDEE8`, `0xE00E`) reached from `helper_d6af`. The catalog was appended to `boot_and_io_notes.md` and short raw comments were added to `read_path.asm` to reflect these literals. No semantic assignments or behavioral inferences were made.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

## Commands
```bash
git checkout -b codex/m22b-xdos-downstream-literal-catalog develop
git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm
```

## Evidence
### Downstream Target Literal Catalog (from boot_and_io_notes.md)
| target | observed literal/immediate | evidence class | neutral note |
| :--- | :--- | :--- | :--- |
| `0xD155` | `0x00` | confirmed | immediate value observed in local window |
| `0xD753` | `0x0D` | confirmed | immediate value observed in local window |
| `0xD753` | `0xD1B5` | confirmed | absolute address literal observed in local window |
| `0xD753` | `0x01` | confirmed | immediate value observed in local window |
| `0xDEE8` | `0x0140` | confirmed | immediate value observed in local window |
| `0xDEE8` | `0x00A8` | confirmed | immediate value observed in local window |
| `0xDEE8` | `0xEE00` | confirmed | absolute address literal observed in local window |
| `0xE00E` | `0x72` | confirmed | immediate value observed in local window |

### Read Path Reconstruction (from read_path.asm)
- `0xD155`: `; literal: 0x00`
- `0xD753`: `; literals: 0x0D, 0xD1B5, 0x01`
- `0xDEE8`: `; literals: 0x0140, 0x00A8, 0xEE00`
- `0xE00E`: `; literal: 0x72`

## Risks
- The catalog is restricted to the specific 5-10 byte windows already documented. If the logic continues past these windows, additional literals may be present but are not captured here.
- Relative offsets (like `0x0D` or `0x72`) are treated as immediate literals per instruction, but their absolute target address is not calculated to avoid inferred semantics.

## Requested Review
- Verify that no semantics were accidentally introduced.
- Confirm the 16-bit literal endianness (e.g., `0xEE00` from `21 00 EE`) matches expected Z80 conventions.
