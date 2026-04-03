# V2.1.0 Release Task List

## Completed

- public `develop` branch reset
- public release and tag cleanup
- public branch set reduced to `main` and `develop`
- old public release notes moved under `Documents/old_release_notes/`
- ad-hoc root analysis scripts moved under `scripts/legacy_analysis/`
- `communication/` excluded from publication flow
- Hu-BASIC empty-format parity aligned with true formatted disks
- X1 Hu-BASIC logical-copy behavior improved enough for direct emulator validation

## In Progress

- README refresh for the new public baseline
- public-facing support status split into dedicated guides
- public-facing common use-case guide split from README

## Pending Before Main Merge

- sanitize obvious absolute-path references in public governance documents
- review current `develop` for any remaining publication-unfriendly personal paths
- re-check whether public docs still rely on repository-private sample paths where generic placeholders would be better
- decide whether old duplicated top-level documents outside `Documents/governance/` and `Documents/guides/` should remain or be archived

## Pending Before V2.1.0 Release

- confirm release target commit
- merge the refreshed `develop` into `main`
- run release build for standalone CLI artifacts
- verify release archives locally
- create a fresh `v2.1.0` tag
- publish a fresh `v2.1.0` GitHub release with standalone CLI binaries

## Stop Line

Do not create the `v2.1.0` tag or GitHub release until the remaining public-doc sanitization review has been completed.
