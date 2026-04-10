# Publication Reset Task List

## Safe Completed

- moved `RELEASE_NOTES_v1.2.0.md` through `RELEASE_NOTES_v1.6.0.md` into `Documents/old_release_notes/`
- moved root-level ad-hoc Python scripts and PNG analysis artifacts into `scripts/legacy_analysis/`
- marked `communication/` and `scripts/legacy_analysis/` as ignored for public branch cleanup

## Safe Next

- review remaining root-level ad-hoc files and move obvious analysis leftovers out of the repository root
- verify the working tree no longer treats `communication/` as publication content
- keep `RELEASE_NOTES_v2.0.0.md` as the only root release note

## Dangerous Pending

- decide the exact public file set for the new `develop`
- archive the current remote `develop` as `archived_develop`
- replace `develop` with the current working branch content
- delete remote branches other than `main`, `develop`, and `archived_develop`
- delete all existing GitHub releases, release assets, and release tags intended for reset
- sanitize `archived_develop` to remove personal paths, private filenames, and non-public archive references
- publish a new standalone CLI release as `v2.1.0`

## Stop Line

Do not perform remote branch deletion, release deletion, tag deletion, or history rewrite until the new public `develop` file set is explicitly confirmed.
