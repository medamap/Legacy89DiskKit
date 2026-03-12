# Handoff Summary: Legacy89DiskKit Progress

This folder contains a snapshot of the current implementation progress and plans for the next session.

## Current Status (End of Phase 16)
- **Character Encoding**: Fully implemented with support for X1 and Shift-JIS. The CLI `list` command now supports the `-e` / `--encoding` option.
- **Filename Normalization**: Implemented `FileNameNormalizationService`. It handles shortening (e.g., 6.3 for N88, 8.3 for MSX, 13 chars for Hu-BASIC) and collision resolution (`~1`).
- **File Injection**: The `inject` command is functional and automatically adapts host filenames for the target filesystem.
- **Architecture**: Domain-driven design is established. Infrastructure layers for Hu-BASIC, N88-BASIC, and MSX-DOS are updated to support raw name preservation.

## Files in this Handoff
- [task.md](./task.md): The full project checklist. Phase 17 is the current target.
- [walkthrough.md](./walkthrough.md): Documentation of the Phase 16 implementation results and verification.
- [implementation_plan.md](./implementation_plan.md): The technical plan followed for Phase 16.

## Priority for Next Session (Phase 17)
1. **Symbolic Attributes**: Replace hex attribute display with a string representation (e.g., `PBAGSIH`).
2. **Platform-Specific Metadata**: Add 16-bit binary addresses (Load/End/Exec) to the `list` command for Hu-BASIC disks.
3. **Attribute Management**: Design a per-filesystem command for toggling specific flags (e.g., setting the 'Hidden' or 'System' attribute).

## Verification Context
The CLI has been published to `bin/publish/osx-arm64/Legacy89DiskKit.Cli`.
To list files with Shift-JIS (for Japanese filenames):
`./bin/publish/osx-arm64/Legacy89DiskKit.Cli list <image> -e sjis`
