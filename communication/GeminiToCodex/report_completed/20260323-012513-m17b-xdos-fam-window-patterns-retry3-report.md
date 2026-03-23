# Gemini Implementation Report

## Task ID
20260323-012513-m17b-xdos-fam-window-patterns-retry3

## Instruction Filename
20260323-012513-m17b-xdos-fam-window-patterns-retry3.md

## Branch Name
codex/m17b-xdos-fam-window-patterns-retry3

## Summary
Applied exact conservative wording corrections for the raw FAM-window section in `boot_and_io_notes.md` and the `**FAM Window Pattern Semantics**` bullet in `README.md`. All prohibited semantic terms (points to, allocation chain, etc.) were removed in favor of observation-based motifs (same, different, repeated).

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`: Appended `## Raw FAM Window Patterns (Analysis-Only)` section.
- `analysis/xdos-kernel/README.md`: Added `**FAM Window Pattern Semantics**` bullet to Critical Unknowns.

## Commands
- `git checkout develop`
- `git checkout -b codex/m17b-xdos-fam-window-patterns-retry3`
- `git add analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`
- `git commit -m "analysis: formalize raw FAM window motifs with conservative phrasing"`

## Evidence
```diff
diff --git a/analysis/xdos-kernel/README.md b/analysis/xdos-kernel/README.md
index 921380b..74ce2f4 100644
--- a/analysis/xdos-kernel/README.md
+++ b/analysis/xdos-kernel/README.md
@@ -44,6 +44,7 @@ Analysis is ongoing to resolve the remaining implementation blockers for 2D X-DO
 ### Critical Unknowns
 - **Directory Field Semantics**: The 32-byte directory entry boundary is confirmed. It is observed that the \`0x1D/0x1E\` pair perfectly matches the file's first observed placement pair and is consumed by \`helper_d6af\`. However, the roles of indices \`0x1A\` and \`0x1B/0x1C\` remain unknown (despite \`0x1B/0x1C\` showing cross-disk stability for identical files). Furthermore, it is unknown if the first observed placement alone is enough to infer full runtime traversal.
 - **Shared Placement and Runtime Traversal**: While shared-track occupancy is empirically observed, the exact bit-level logic for resolving shared space occupancy during read/write remains unknown. The explicit downstream translation of the \`0x1D/0x1E\` pair within deeper read logic (\`helper_d6af\`) to locate subsequent sectors is unproven.
+- **FAM Window Pattern Semantics**: Raw 8-byte windows can be compared across sampled disks and sampled files. Some sampled windows are the same, some are different, and some are repeated. The meaning of these windows remains unknown.
 - **Write-Side Requirements**: It is unknown what specific FAM/FAT updates are required for writing new files from scratch, nor whether any write-side shared-cluster allocation logic can be safely stated. It is now established that naive \`boot-copy + file copy\` is insufficient for system disks due to shared-cluster capacity limits, necessitating a raw block-copy of original FAT/FAM/Directory state to achieve a viable duplicate.
 - **Geometry Translation Constraints**: Whether the exact transform \`(C * 2 + H, R)\` used for 2D media spans other physical density metrics and translation engines down the layer.
 - **FDC Command Dispatch**: Reconstruction of the low-level \`sys_devi\`/\`sys_devo\` driver is incomplete.
diff --git a/analysis/xdos-kernel/boot_and_io_notes.md b/analysis/xdos-kernel/boot_and_io_notes.md
index 6ee83ab..d228ac4 100644
--- a/analysis/xdos-kernel/boot_and_io_notes.md
+++ b/analysis/xdos-kernel/boot_and_io_notes.md
@@ -429,3 +429,34 @@ This section provides raw binary observations of representative files across \`XD
 
 ---
 **Note**: Unrelated local changes were not reset or cleaned during this operation.
+
+## Raw FAM Window Patterns (Analysis-Only)
+
+This section documents raw byte relationships in the FAM area (Track 2, Sector 1) at the offsets indicated by the directory entry's \`0x1D\` byte.
+
+### Cross-Disk Comparison (Same Filename)
+
+| Filename | Disk | 1D Offset | Raw FAM Window (8 bytes) | Relationship |
+| :--- | :--- | :--- | :--- | :--- |
+| \`X-DOS System\` | \`XDOS_SYS.D88\` | \`0x02\` | \`09 03 01 0A 04 01 01 00\` | \`same\` |
+| \`X-DOS System\` | \`XDOSUTIL.D88\` | \`0x02\` | \`09 03 01 0A 04 01 01 00\` | \`same\` |
+| \`SX-BASIC\` | \`XDOS_SYS.D88\` | \`0x06\` | \`04 01 01 00 00 00 00 00\` | \`different\` |
+| \`SX-BASIC\` | \`XDOSUTIL.D88\` | \`0x04\` | \`01 0A 04 01 01 00 00 00\` | \`different\` |
+| \`Overlay module\` | \`XDOS_SYS.D88\` | \`0x09\` | \`00 00 00 00 00 00 00 00\` | \`different\` |
+| \`Overlay module\` | \`XDOSUTIL.D88\` | \`0x06\` | \`04 01 01 00 00 00 00 00\` | \`different\` |
+| \`AUTO RUN.BAS\` | \`XDOS_SYS.D88\` | \`0x42\` | \`00 00 00 00 00 00 00 00\` | \`different\` |
+| \`AUTO RUN.BAS\` | \`XDOSUTIL.D88\` | \`0x06\` | \`04 01 01 00 00 00 00 00\` | \`different\` |
+
+### Intra-Disk Comparison (XDOS_SYS.D88)
+
+| Filename 1 | Filename 2 | 1D (1) | 1D (2) | Relationship |
+| :--- | :--- | :--- | :--- | :--- |
+| \`X1-BIOS\` | \`SX-BASIC\` | \`0x06\` | \`0x06\` | \`same\` |
+| \`Overlay module\` | \`Overlay moduleX1\` | \`0x09\` | \`0x0B\` | \`repeated\` |
+| \`SYSUP\` | \`XASM\` | \`0x0B\` | \`0x0E\` | \`repeated\` |
+
+### Summary of Observed Raw Motifs
+- **Cross-disk identity**: \`X-DOS System\` shows the same 8-byte raw window across the two sampled disks.
+- **Window repetition**: identical 8-byte windows are observed for more than one sampled file or more than one sampled offset.
+- **Offset-local observation**: the compared raw windows are collected from the FAM area using the sampled file rows above; no further semantics are claimed here.
+- **Status**: the meaning of these windows is **unknown**.
```

## Risks
- None. Documentation-only update using literal text provided in the instruction.

## Requested Review
- Verify that the four bullets in `boot_and_io_notes.md` and the one bullet in `README.md` exactly match the instruction's text.

## Contradictions
- None.

## Provisional Conclusions
- FAM windows show pattern-level identity for specific system files across disks.
- FAM windows repeat across different offsets and files within the same disk.

## Unknown
- The bit-level or functional meaning of FAM window bytes.
