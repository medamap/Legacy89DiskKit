# Gemini Task Report

## Task ID
20260323-030400-m17g-xdos-fam-correlation-boundary

## Instruction Filename
20260323-030400-m17g-xdos-fam-correlation-boundary.md

## Branch Name
codex/m17g-xdos-fam-correlation-boundary

## Summary
Documented the current evidence boundary for X-DOS FAM correlation. Added a new section to `boot_and_io_notes.md` and updated `README.md` to clarify that while raw FAM bytes and read-path directory-pair handling are observed, a direct one-to-one mapping remains unproven.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/README.md`

## Commands
```bash
git checkout develop
git checkout -b codex/m17g-xdos-fam-correlation-boundary
# [Edited files]
git add analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md
git commit -m "docs: document X-DOS FAM correlation boundary"
```

## Evidence
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added `## FAM Correlation Boundary (Analysis-Only)` section summarizing observed facts (raw FAM windows, 4-bit range, byte consumption, addressing arithmetic) and the unresolved boundary.
- `analysis/xdos-kernel/README.md`: Appended "The direct correlation boundary remains unresolved." to the `FAM Window Pattern Semantics` bullet.

## Risks
None identified. This was a documentation-only task to record the current analysis boundary.

## Requested Review
Verify that the new section in `boot_and_io_notes.md` and the update in `README.md` correctly reflect the current evidence boundary without introducing speculative claims.

## Contradictions
None.

## Provisional Conclusions
The current evidence confirms that the directory `0x1D/0x1E` pair is used for indexing and is consumed during the read path, but how it maps to the raw FAM sector data is still not definitively proven.

## Unknown
- The specific mapping formula or lookup table between the directory metadata and FAM bytes.
