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
- **Directory Field Semantics**: While the 32-byte directory entry boundary and indexing are confirmed, the exact roles of indices `0x1A` and the `0x1B/0x1C` pair remain open questions. It is proven that the `0x1D/0x1E` pair perfectly matches the file's observed placement pair and is consumed by `helper_d6af`, but its explicit downstream translation within the deeper read logic remains unknown. The `0x1B/0x1C` pair shows cross-disk stability for identical files, but its meaning remains unknown.
- **Observed Placement Pair Mapping**: Exact bit-level logic for resolving shared space occupancy.
- **Geometry Translation**: Analyzing whether the exact transform linking the physical D88 header tuple `(C, H, R)` to the observed placement pair spans other physical density metrics and translation engines down layer.
- **FDC Command Dispatch**: Reconstruction of the low-level `sys_devi`/`sys_devo` driver is incomplete.
