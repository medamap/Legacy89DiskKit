# Gemini Task Report

## Task ID
20260323-012946-m17c-xdos-fam-byte-nibble-stability

## Instruction Filename
20260323-012946-m17c-xdos-fam-byte-nibble-stability.md

## Branch Name
codex/m17c-xdos-fam-byte-nibble-stability

## Summary
Analyzed the stability of X-DOS FAM windows at byte and nibble granularity across `XDOS_SYS.D88` and `XDOSUTIL.D88`. Established that the FAM sector content at any given absolute offset is stable across disks. Discovered that the entire FAM sector (Track 2, Sector 1, 512 bytes) has a `stable-high-nibble` of `0x0`. Classified window positions (relative to directory index `0x1D`) as `stable-byte` when the index is identical, and `variable` when it changes. No semantic interpretation was introduced.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/README.md`

## Commands
- `python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/collect_raw_catalog.py`
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Evidence
- Comparison of `X-DOS System` (1D=0x02) across disks showed identical 8-byte windows.
- Comparison of `SX-BASIC` (SYS, 1D=0x06), `Overlay module` (UTIL, 1D=0x06), and `AUTO RUN.BAS` (UTIL, 1D=0x06) showed identical 8-byte windows.
- Global scan of FAM sector (Track 2, R=1) confirmed high nibbles are `0x0` for all 512 bytes in both sampled disks.
- Cross-disk same-file comparison for `SX-BASIC` (1D=0x06 vs 1D=0x04) confirmed that window content varies with the `0x1D` offset, while the underlying FAM bytes at specific absolute indices (e.g., FAM[0x06]=04) remain stable.

## Provisional Conclusions
- FAM absolute content is disk-invariant for the sampled system/utility images.
- FAM data is effectively 4-bit per byte or limited to values `0..15`.
- Window stability is a function of the directory index `0x1D`.

## Unknown
- Semantic meaning of the 4-bit values (low nibbles).
- Semantic meaning of FAM offsets beyond the sampled tracks.
- Whether the high nibble is reserved for future use or flags.

## Risks
- None identified for this analysis-only task.

## Requested Review
- Verify that the stability classification in `boot_and_io_notes.md` accurately reflects the provided evidence without crossing into semantics.
