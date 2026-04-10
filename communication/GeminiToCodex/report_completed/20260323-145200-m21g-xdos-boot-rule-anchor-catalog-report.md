# Gemini Task Report

## Task ID
20260323-145200-m21g-xdos-boot-rule-anchor-catalog

## Instruction Filename
20260323-145200-m21g-xdos-boot-rule-anchor-catalog.md

## Branch Name
`codex/m21g-xdos-boot-rule-anchor-catalog`

## Summary
Cataloged the currently observed raw boot-rule anchors in `boot_and_io_notes.md` and updated `README.md` to reference the new catalog. The catalog includes observed spans for Track 0 and Track 1, along with cross-disk equality/difference split patterns. No invariant boot semantics were introduced, and wording was restricted to raw observations.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/README.md`

## Commands
```bash
git checkout develop
git checkout -b codex/m21g-xdos-boot-rule-anchor-catalog
# [Edit files]
git add analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md
git commit -m "docs: catalog raw boot-rule anchors in analysis notes"
```

## Evidence
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added `## Boot Rule Anchor Catalog (Analysis-Only)` section with raw spans for T0H0, T0H1, and T1H0.
- `analysis/xdos-kernel/README.md`: Appended reference sentence to the `Boot/early-area observations` bullet.
- Verified correct diff via `git diff`.

## Risks
- None. This was an investigation/documentation update task only.

## Requested Review
- Verify that the cataloged spans and equality patterns correctly reflect the raw data without asserting rules.

## Contradictions
- None.

## Provisional Conclusions
- Track 0 Head 0 and Track 1 Head 0 appear to be identical across system and utility disks, while Track 0 Head 1 contains disk-specific FAT/Directory data in its early sectors.

## Unknown
- The exact boot rule boundary remains unknown.
- The full exact extent of the directory area remains unresolved.
