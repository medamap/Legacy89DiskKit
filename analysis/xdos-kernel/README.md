# X-DOS Kernel Analysis Workspace

## Overview
This workspace is dedicated to the reverse engineering and assembly reconstruction of the X-DOS kernel, with a primary focus on filesystem-access logic.

## Analysis Conventions

### Source Priority
1.  **Primary Evidence**: Direct binary observations from `XDOS_SYS.D88`, `XDOSUTIL.D88`, or salvaged Z80 code snippets.
2.  **Secondary Analysis**: Technical documents, logical derivations, or patterns observed across multiple disks.
3.  **Implementation Assumptions**: Current C# code behaviors that haven't been strictly verified against primary artifacts.

### Evidence Classes
Every label and reconstruction must state its evidence class:
-   **confirmed**: Directly verified by binary analysis or salvaged source code.
-   **probable**: Highly likely based on surrounding code or secondary documentation, but not yet 100% verified (e.g., FDC ports on a known X1 system).
-   **placeholder**: Temporary label used to maintain structural integrity or hardware-known ports that are not yet confirmed to be used by the X-DOS kernel.

### Code vs Data Representation
-   Confirmed instructions are represented as Z80 assembly mnemonics.
-   Data tables, buffers, and uncertain code regions must remain as `db` (define byte) or `dw` (define word) statements.
-   **Strict Prohibition**: Do not synthesize or invent filler bytes (e.g., `db 0x00` or `ret`) for unknown code regions. If a region is known only by entry address, represent it as a label plus a comment, leaving the body empty or clearly marked as not yet reconstructed.
-   Mixed code/data regions must be clearly commented.

### Jump Table Convention
- Unknown jump entries in a confirmed jump table are represented as `ds 3`.
- This maintains structural alignment without synthesizing `jp` opcodes or unknown addresses.

### Scope
The current scope is limited to:
-   System call entrypoints (syscall table).
-   File read path logic (including FAM/FAT access).
-   Interleaved side-selection logic (`EE 10` pattern).
-   Device I/O (`devi`/`devo`) dispatch.

## File Structure
-   `labels.tsv`: Central repository for addresses, labels, and evidence classes.
-   `read_path.asm`: Assembly reconstruction of confirmed read-related kernel areas.
-   `boot_and_io_notes.md`: Technical notes on I/O constants and boot-time relationships.

## Status: Research-Active
Analysis is ongoing to resolve the remaining implementation blockers for 2D X-DOS cloning.

### Critical Unknowns
- **Directory Field Semantics**: The 32-byte directory entry boundary is confirmed. It is observed that the `0x1D/0x1E` pair perfectly matches the file's first observed placement pair and is consumed by `helper_d6af`. However, the roles of indices `0x1A` and `0x1B/0x1C` remain unknown (despite `0x1B/0x1C` showing cross-disk stability for identical files). Furthermore, it is unknown if the first observed placement alone is enough to infer full runtime traversal.
- **Shared Placement and Runtime Traversal**: While shared-track occupancy is empirically observed, the exact bit-level logic for resolving shared space occupancy during read/write remains unknown. The explicit downstream translation of the `0x1D/0x1E` pair within deeper read logic (`helper_d6af`) to locate subsequent sectors is unproven. Representative shared-placement cases are now cataloged at the raw observation level. Recent full-disk directory scans confirm that exact 0x1D/0x1E pair duplication within the same disk is not observed for valid files. Raw observations verify that all cataloged same-disk shared-track cases are characterized by a common first byte (`0x1D`) with differing second byte (`0x1E`). The current boundary remains unresolved beyond the raw catalog. The shared-placement summary boundary is now established for the raw observation pattern, while ownership and reconstruction rules remain unknown.
- **FAM Window Pattern Semantics**: Raw 8-byte windows can be compared across sampled disks and sampled files. Some sampled windows are the same, some are different, and some are repeated. Stability can now be described at byte/nibble granularity for the sampled windows. Direct inspection of the full 512-byte FAM sector confirms that all bytes stay within the `0x00..0x0F` range (max value `0x0A`), but semantics remain unknown. Read-path addressing arithmetic is directly observed in the reconstructed helper window. The direct correlation boundary remains unresolved.
- **Write-Side Requirements**: It is unknown what specific FAM/FAT updates are required for writing new files from scratch, nor whether any write-side shared-cluster allocation logic can be safely stated. It is now established that naive `boot-copy + file copy` is insufficient for system disks due to shared-cluster capacity limits, necessitating a raw block-copy of original FAT/FAM/Directory state to achieve a viable duplicate. Confirmed write-path entry windows are now cataloged at the raw observation level. The current write-path boundary remains unresolved beyond the raw catalog.
- **Geometry Translation Constraints**: Whether the exact transform `(C * 2 + H, R)` used for 2D media spans other physical density metrics and translation engines down the layer. Boot/early-area observations are now cataloged at the raw level in `boot_and_io_notes.md`. Raw early-area sector spans for Track 0 and Track 1 are now cataloged at the observation level. Cross-disk equality for sampled early-area regions is now cataloged at the raw level. The boot and early-area summary boundary is now established. The minimal implementation reconciliation matrix now exists in `boot_and_io_notes.md`.
- **FDC Command Dispatch**: Reconstruction of the low-level `sys_devi`/`sys_devo` driver is incomplete.
