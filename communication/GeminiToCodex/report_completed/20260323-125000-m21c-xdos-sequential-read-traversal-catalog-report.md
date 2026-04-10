# Task Report: M21c X-DOS Sequential Read Traversal Catalog

## Task Information
- **Task ID**: 20260323-125000-m21c-xdos-sequential-read-traversal-catalog
- **Instruction Filename**: `20260323-125000-m21c-xdos-sequential-read-traversal-catalog.md`
- **Branch Name**: `codex/m21c-xdos-sequential-read-traversal-catalog`

## Summary
Cataloged currently observed downstream read-traversal windows in `boot_and_io_notes.md` based on reconstructed `helper_d6af` logic in `read_path.asm`. The catalog identifies call and jump targets that are executed after the initial directory-linked placement pair is consumed, without assigning traversal semantics to these targets.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`

## Commands
```bash
git add analysis/xdos-kernel/boot_and_io_notes.md && git commit -m "docs: catalog downstream read traversal windows in boot_and_io_notes.md"
```

## Evidence
The following table was added to `analysis/xdos-kernel/boot_and_io_notes.md`:

| observed window | directly observed relation | evidence class |
| :--- | :--- | :--- |
| `0xD155` | `call target observed` | confirmed from `helper_d6af` |
| `0xE00E` | `call target observed` | confirmed from `helper_d6af` |
| `0xDEE8` | `call target observed` | confirmed from `helper_d6af` |
| `0xD753` | `jp target observed` | confirmed from `helper_d6af` |
| `0xD6AF` window | `downstream window cataloged` | confirmed from `sys_rdd_impl` |
| `0xDEE8` context | `control transfer observed` | confirmed from `helper_d6af` after 1D/1E load |

## Risks
- The targets `0xD155`, `0xE00E`, `0xDEE8`, and `0xD753` are confirmed to be called/jumped to, but their internal logic is not yet analyzed in detail.
- No semantic meaning (e.g., "next cluster") is assigned, so these windows are strictly for downstream traversal investigation.

## Requested Review
- Verify the physical placement of the new section in `boot_and_io_notes.md`.
- Confirm that the cataloged windows accurately represent the "downstream" logic after the placement pair handling.

## Contradictions
None observed.

## Provisional Conclusions
The `helper_d6af` routine acts as a central dispatcher that delegates work to at least three sub-calls (`0xD155`, `0xE00E`, `0xDEE8`) and one final jump (`0xD753`). These targets likely hold the key to understanding the full sequential read traversal (FAT/FAM usage).

## Unknown
- The specific functional roles of `0xD155`, `0xE00E`, `0xDEE8`, and `0xD753`.
- The exact bit-level logic inside these targets.
