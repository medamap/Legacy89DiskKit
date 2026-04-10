# Gemini Task Report

## Task ID
20260323-210653-m22c-xdos-downstream-control-transfer-catalog

## Instruction Filename
20260323-210653-m22c-xdos-downstream-control-transfer-catalog.md

## Branch Name
`codex/m22c-xdos-downstream-control-transfer-catalog`

## Summary
Cataloged directly observed control-transfer instructions (`call`, `jp`, `jr`, `ret`) for the four target windows `0xD155`, `0xD753`, `0xDEE8`, and `0xE00E`. Appended a new section to `boot_and_io_notes.md` and added raw comments to `read_path.asm`.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

## Commands
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`
- `git commit -m "docs(analysis): catalog downstream target control transfers"`

## Evidence
### `analysis/xdos-kernel/boot_and_io_notes.md` (Appended)
```markdown
## Downstream Target Control Transfers (Analysis-Only)

| target | observed transfer | evidence class | neutral note |
| :--- | :--- | :--- | :--- |
| `0xD155` | `ret` | confirmed | return observed in target window |
| `0xD753` | `jr nz` | confirmed | local jump observed in target window |
| `0xD753` | `call` | confirmed | local call observed in target window |
| `0xE00E` | `jr c` | confirmed | local jump observed in target window |
```

### `analysis/xdos-kernel/read_path.asm` (Comments)
- `0xD155`: `db 0x04, 0x42, 0x0E, 0x00, 0xC9 ; ret observed`
- `0xD753`: `db 0x40, 0x20, 0x0D, 0x13, 0xCD, 0xB5, 0xD1, 0x3E, 0x01 ; jr nz and call observed`
- `0xDEE8`: `db 0x01, 0x40, 0x01, 0x11, 0xA8, 0x00, 0x21, 0x00, 0xEE, 0x19 ; no control transfer observed`
- `0xE00E`: `db 0xEB, 0xDF, 0x38, 0x72, 0x06 ; jr c observed`

## Risks
- The `0xDEE8` window provided in the analysis assets does not contain any of the listed control-transfer instructions.
- `RST 18H` (`DF`) at `0xE00F` was observed but not cataloged as it was not in the required mnemonic list (`call`, `jp`, `jr`, `ret`).

## Requested Review
- Ensure the neutral notes and evidence classes align with the project's strict analysis-only requirements.
