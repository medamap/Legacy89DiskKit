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
-   **probable**: Highly likely based on surrounding code or secondary documentation, but not yet 100% verified.
-   **placeholder**: Temporary label used to maintain structural integrity during reconstruction.

### Code vs Data Representation
-   Confirmed instructions are represented as Z80 assembly mnemonics.
-   Data tables, buffers, and uncertain code regions must remain as `db` (define byte) or `dw` (define word) statements.
-   Mixed code/data regions must be clearly commented.

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
