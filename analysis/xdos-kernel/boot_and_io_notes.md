# X-DOS Boot and I/O Analysis Notes

## Logical Record Constants
The following logical record numbers are used for system I/O, as confirmed by `make_BGM` SLANG source:

| Record Number | Physical Mapping | Content |
| :--- | :--- | :--- |
| 10 | Track 1, R=1 | FAT bitmap |
| 11 | Track 1, R=2 | Directory (first sector) |
| 20 | Track 2, R=1 | FAM (File Allocation Map) |
| 21 | Track 2, R=2 | bdir (binary system code) |

### Mapping Formula
For logical record numbers `rec >= 10`:
- `physical_track = (rec - 10) / 10 + 1`
- `physical_R = (rec - 10) % 10 + 1`

## Directory Entry Boundary (Confirmed)

The X-DOS directory entry is a fixed 32-byte block. Entries are arranged contiguously starting from the directory area (Track 1, Sector 2).

### Raw Evidence Table (XDOS_SYS.D88)

| Entry | Start (Offset) | Filename (Offset +2) | Filename Bytes | Length | 0x1A | 0x1B | 0x1D | 0x1E |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | `0x1650` | `0x1652` | `58 2D 44 4F 53 20 53 79 73 74 65 6D 20 20 20 20` | `0x20` | `BA` | `D5` | `02` | `01` |
| 2 | `0x1670` | `0x1672` | `58 2D 44 4F 53 20 53 79 73 74 65 6D 20 58 31 20` | `0x20` | `12` | `A8` | `04` | `02` |
| 3 | `0x1690` | `0x1692` | `58 31 2D 42 49 4F 53 20 20 20 20 20 20 20 20 20` | `0x20` | `12` | `B4` | `06` | `03` |

### Boundary and Indexing Summary
- **Entry Base**: Start of any 32-byte block in the directory area.
- **Entry Length**: 32 bytes (0x20).
- **Filename Span**: Bytes 2 through 17 (16 bytes).
- **Index 0x1A (26)**: Byte at `Base + 26`.
- **Index 0x1B (27)**: Byte at `Base + 27`.
- **Index 0x1D (29)**: Byte at `Base + 29`.
- **Index 0x1E (30)**: Byte at `Base + 30`.

*Note: No field semantics are assigned to these indices in this analysis. This section only proves their physical location and the fixed entry boundary.*

## Geometry Translation (Evidence-Graded)

- **Methodology**: To translate the raw D88 header tuple `(C, H, R)` into a flat observed placement pair, the exact transform `(C * 2 + H, R)` is applied.
- **D88 Header Decoding**: As shown in tracked helper `find_file_start.py`, the D88 sector header comprises a tuple of 4 bytes `(C, H, R, N)` read directly from every physical sector start block within the image track bodies. 
  - `C` (Cylinder): Represents the physical actuator position.
  - `H` (Head): Represents the side of the disk.
  - `R` (Record/Sector): Represents the sector ID within the track.
- **Exact Transform Justification**: The calculation `C * 2 + H` flattens the physical actuator and head position into a continuous, linear metric. Because the sampled images (`XDOS_SYS.D88` and `XDOSUTIL.D88`) are standard 2D formats defined by double-sided media (using 2 heads, `H=0` or `1`), each contiguous physical cylinder houses exactly 2 sides. Multiplying the physical cylinder coordinate by 2 and adding the active head securely flattens the C/H axes into a single monolithic value spanning `0..79`.
- **Side Layout Assumptions**: This exact transform inherently depends on a double-sided layout defined for 2D media. If the geometry assumed single-sided (1D) properties, the multiplier would drop to 1.

### Directory Byte Pair 0x1B/0x1C Analysis (Evidence-Graded)

- **Constraints Note**: No directory field comparison was performed in this detection method.
- **Observation**: A cross-disk comparison of identical files demonstrates cross-disk stability for the byte pair at `0x1B` (27) and `0x1C` (28).

| Filename | Disk | 0x1B/0x1C |
| :--- | :--- | :--- |
| `SX-BASIC` | `XDOS_SYS.D88` | `67 80` |
| `SX-BASIC` | `XDOSUTIL.D88` | `67 80` |
| `Overlay module` | `XDOS_SYS.D88` | `A6 80` |
| `Overlay module` | `XDOSUTIL.D88` | `A6 80` |

- **Conclusion**: The same pair observed for identical files across different disks proves cross-disk stability.
- **Status**: The semantic meaning of the `0x1B/0x1C` pair remains unknown.

### Directory Byte Pair 0x1D/0x1E Analysis (Evidence-Graded)

- **Observation**: Directory bytes at indices `0x1D` (29) and `0x1E` (30) are consumed as a contiguous 16-bit pair by the `helper_d6af` kernel routine (Memory Address: `0xD6AF`).
- **Instruction Evidence**:
  ```asm
  ld de, 0x001D
  add hl, de    ; Point HL to offset 29 of the entry
  ld d, (hl)    ; Load 0x1D into D
  inc hl
  ld e, (hl)    ; Load 0x1E into E
  ```
- **Execution Flow**: The `DE` pair is subsequently passed into the next helper/traversal stage via `call 0xDEE8`.
- **Status**: The internal bit-level semantics and external meaning (e.g., whether these represent a cluster, sector, or other metadata) of this pair remain **unknown**.

### Directory Byte Pair 0x1D/0x1E vs Observed Placement Pair Comparison (Evidence-Graded)

- **Observation**: An independent raw hex scan of the disk images (via tracked helper `find_file_start.py`) identifies the observed placement pair for the beginning of a file's payload. This observed placement pair exactly matches the `0x1D/0x1E` pair found in the directory for all sampled files.

| Disk | Filename | `0x1D/0x1E` Pair | Observed Placement Pair | Match Status |
| :--- | :--- | :--- | :--- | :--- |
| `XDOS_SYS.D88` | `X-DOS System X1` | `(04, 02)` | `(04, 02)` | exact match |
| `XDOS_SYS.D88` | `SX-BASIC` | `(06, 08)` | `(06, 08)` | exact match |
| `XDOSUTIL.D88` | `Overlay module` | `(06, 06)` | `(06, 06)` | exact match |

- **Conclusion**: The `0x1D/0x1E` pair equals the observed placement pair for all sampled files. There are no contradictions across system files and utility files.
- **Status**: The explicit downstream translation of these values within the deeper read logic remains **unknown**.

## Device I/O Calls
Device-level I/O is performed via `sys_devi` (input) and `sys_devo` (output).

- **Registers**:
  - `HL`: Memory buffer address.
  - `DE`: Logical record number (see above).
  - `A`: Record count (number of sectors).
  - `CY` (carry flag): 1 = Error.

## Boot Sequence
- **Track 0, R=1 (256B)**: Volume Record and initial IPL entry point.
- **Records 0-9**: Map to first 10 sectors of Track 0 (IPL code).
- **BCD Date**: Found at Track 0, R=1, offset 25-27 (YY MM DD).

## Observed Byte Windows

### Volume Record (Confirmed)
- **Source Disk**: `XDOSUTIL.D88` / `XDOS_SYS.D88`
- **Physical Location**: Track 0, Sector 1 (offset 0x10 from sector start)
- **Importance**: Confirms format identifier (`0x88`), disk label location, and BCD creation date location. This is the primary anchor for filesystem detection.

### Interleaved Side-Select (Confirmed)
- **Source Disk**: `XDOSUTIL.D88`
- **Physical Location**: Track 2, Sector 8 (offset 0x4bd9 in D88 file)
- **Importance**: Confirms the use of bit 4 for side selection and identified a probable shadow RAM location (`0xE691`) for the drive control latch.

### FDC Status Wait (Confirmed)
- **Source Disk**: `XDOSUTIL.D88`
- **Physical Location**: Track 2, Sector 8 (offset 0x4b3c in D88 file)
- **Importance**: Confirms standard MB8877A I/O port usage (`0x0FF8`) and typical status-polling loop pattern in the kernel.

### Syscall Jump Table (Confirmed)
- **Source Disk**: `XDOS_SYS.D88`
- **Physical Location**: Track 6, Sector 1 (offset 0x7c13 in D88 file)
- **Memory Address**: `0xED78`
- **Observations**:
    - Confirmed `C3 xx yy` (jp) pattern for 40+ entries.
    - Matches syscall addresses from `x-dos.h` (e.g., `sys_wopen` at Entry 0, `sys_rdd` at Entry 3).
    - **Extraction Limit**: This table was NOT found at the same physical offset in `XDOSUTIL.D88`, which contains BASIC-like strings in that region. This suggests the kernel is not identically mapped across all disks or `XDOSUTIL.D88` is not a bootable system disk.
    - **Mapping Gap**: The logical record mapping for the syscall table (`0xED78`) does not align with the `Record 10 = Track 1, R=1` rule if `0xEE00` is the `fat_area`. This suggests either `fat_area` is mapped differently or the kernel code is loaded from a much higher record number.

### Syscall Implementation Region (Confirmed from XDOS_SYS.D88)
- **Source Disk**: `XDOS_SYS.D88`
- **Memory Base**: Implementation code starts around `0xC860`.
- **Mapping**: FileOffset = MemoryAddr - `0xED78` + `0x7c13`.
- **Entrypoints**:
    - `sys_wopen_impl` (`0xC876`): Offset `0x5711`. Starts with `17 CD 34 C9`.
    - `sys_rdd_impl` (`0xC86C`): Offset `0x5707`. Jump to `0xD6AF`.
    - `sys_file_impl` (`0xC898`): Offset `0x5733`. Returns to `HL` via `E3 C9`.
    - `sys_devi_impl` (`0xC8C4`): Offset `0x575F`. Starts with `CD BC C9`.
    - `sys_ropen_impl` (`0xC914`): Offset `0x57AF`. Starts with `38 07 FE 11`.

## Read Path Spec (Conservative Reconstruction)

This section consolidates the directly observed X-DOS read-path evidence into a role-split specification.

### 1. `sys_file` (Entry: 0xED84, Impl: 0xC898)
- **Direct Observation**:
    - Instruction sequence: `E3 C9` (`ex (sp), hl` / `ret`).
    - This pattern pops the return address into `HL` and "returns" to the original `HL` (or whatever was on the stack before).
- **Instruction-Level Inference**:
    - This is a standard Z80 technique for skipping inline parameters.
    - The caller likely places the filename string immediately after the `call sys_file` instruction.
    - `sys_file` reads the filename from `HL` and then increments `HL` to point past the string before returning via `jp (hl)` (effectively simulated by `ret` after `ex (sp), hl`).
- **Behavioral Hypothesis**:
    - Sets the "active" filename for subsequent `sys_ropen` or `sys_wopen` calls.
    - Likely copies the filename to a internal kernel buffer (e.g., `0x7200` area or similar).

### 2. `sys_ropen` (Entry: 0xED96, Impl: 0xC914)
- **Direct Observation**:
    - Instruction sequence: `38 07 FE 11 D8 D6 07 FE 10 3F C9`.
    - Disassembly:
        ```asm
        jr c, +7      ; Error exit if carry set at entry?
        cp 0x11       ; Compare A with 0x11
        ret c         ; Return if A < 0x11
        sub 0x07      ; Subtract 7
        cp 0x10       ; Compare with 0x10
        ccf           ; Complement carry flag
        ret
        ```
- **Instruction-Level Inference**:
    - Performs validation on the value in register `A`.
    - Returns with `CY` flag indicating success/failure.
- **Behavioral Hypothesis**:
    - Opens a file for reading based on the filename set by `sys_file`.
    - The validation in `A` might relate to drive numbers or file types.

### 3. `sys_rdd` (Entry: 0xED81, Impl: 0xC86C)
- **Direct Observation**:
    - Instruction sequence: `FD B7 C0 C3 AF D6`.
    - Disassembly:
        ```asm
        iy prefix (or dummy)
        or a          ; Check A?
        ret nz        ; Return if A != 0
        jp 0xD6AF     ; Delegate to helper_d6af
        ```
- **Instruction-Level Inference**:
    - `sys_rdd` is a thin wrapper that immediately delegates to `helper_d6af` at `0xD6AF`.
- **Behavioral Hypothesis**:
    - Reads data from the currently open file into memory at `sys_dtadr`.

### 4. Downstream Delegate: `helper_d6af` (Impl: 0xD6AF)
- **Direct Observation**:
    - Instruction sequence: `1B 1B CD 55 D1 CD 0E E0 D8 3E 08 37 C0 7E FE 80 3E 08 37 C0 11 1D 00 19 56 23 5E CD E8 DE C3 53 D7`.
    - Disassembly:
        ```asm
        dec de
        dec de
        call 0xD155   ; Sub-call 1
        call 0xE00E   ; Sub-call 2
        ret c         ; Error return
        ld a, 0x08
        scf
        ret nz        ; Conditional return?
        ld a, (hl)
        cp 0x80
        ...
        ld de, 0x001D
        add hl, de    ; Offset HL by 29
        ld d, (hl)
        inc hl
        ld e, (hl)    ; Load DE from (HL+29)
        call 0xDEE8   ; Sub-call 3
        jp 0xD753     ; Final jump
        ```
- **Instruction-Level Inference**:
    - Deeply nested execution path.
    - Manages register-based state (especially `HL` and `DE`).
    - The `add hl, 0x001D` pattern strongly suggests accessing a fixed-size structure (FCB or Directory entry).
- **Behavioral Hypothesis**:
    - This is the core engine for sequential or random record reading.
    - `0xD155`, `0xE00E`, `0xDEE8`, and `0xD753` likely handle cluster-to-logical-sector translation, FAM traversal, and physical FDC command dispatch.

### Summary of Evidence Grades
- **Directly Observed**: All byte sequences at `0xED78` (jump table), `0xC898`, `0xC914`, `0xC86C`, and `0xD6AF`.
- **Inference**: Parameter-skipping via `E3 C9`, stack-cleanup patterns in helpers, and structure-offsetting (`0x1D`).
- **Hypothesis**: Specific functional roles (e.g., "Set active filename") based on traditional Z80 OS conventions (CP/M-like) and existing `x-dos.h` documentation.

## Write Path Entry Windows (Analysis-Only)

| label or address | observed bytes | evidence class | neutral note |
| :--- | :--- | :--- | :--- |
| sys_wopen_impl | 17 CD 34 C9 FE 13 20 17 CD 34 C9 B7 20 FA CD 7E C9 | confirmed | documented entry window |
| sys_wrd_impl | CD 34 C9 B7 CA 38 C9 C9 | confirmed | documented entry window |
| helper_c934 | 02 38 0D 0F 0F 0F 0F 4F 1A 13 CD EA C9 38 01 B1 C1 C9 | confirmed | documented helper window |
| helper_c938 | 0F 0F 0F 4F 1A 13 CD EA C9 38 01 B1 C1 C9 | confirmed | documented helper window |
| helper_c97e | 78 C1 B7 E1 C9 | confirmed | documented helper window |

## Boot And Early-Area Observations (Analysis-Only)

| observed area | sampled disks | directly observed fact | evidence note |
| :--- | :--- | :--- | :--- |
| Volume Record | `XDOS_SYS`, `XDOSUTIL` | Located at Track 0, Head 0, Sector 1 (256 bytes) | Offset 0x01: `58 2d 44 4f 53` ("X-DOS") |
| FAT Area | `XDOS_SYS`, `XDOSUTIL` | Located at Track 0, Head 1, Sector 1 (512 bytes) | Physical R=1 on Head 1 (Logical Rec 10) |
| Directory Area | `XDOS_SYS`, `XDOSUTIL` | Starts at Track 0, Head 1, Sector 2 (512 bytes) | Physical R=2 on Head 1 (Logical Rec 11) |
| FAM Area | `XDOS_SYS`, `XDOSUTIL` | Located at Track 1, Head 0, Sector 1 (512 bytes) | Physical R=1 on Track 1 (Logical Rec 20) |
| Boot Copy Region | `XDOS_SYS`, `XDOSUTIL` | Track 0, Head 0, Sectors 1-10 (Logical Rec 0-9) | Standard 256-byte IPL sector span |

## Early-Area Span Catalog (Analysis-Only)

| observed region | sampled disks | directly observed span | evidence note |
| :--- | :--- | :--- | :--- |
| Track 0 Head 0 | `XDOS_SYS`, `XDOSUTIL` | R=1-16 (256B) | Observed continuous 16-sector span |
| Track 0 Head 1 | `XDOS_SYS`, `XDOSUTIL` | R=1-10 (512B) | Observed continuous 10-sector span |
| Track 1 Head 0 | `XDOS_SYS`, `XDOSUTIL` | R=1-10 (512B) | Observed continuous 10-sector span |

## Early-Area Cross-Disk Equality (Analysis-Only)

| observed region | comparison result | evidence note |
| :--- | :--- | :--- |
| Track 0 Head 0 R=1-16 | `same` | Bit-for-bit identical across sampled disks |
| Track 0 Head 1 R=1-6 | `different` | Binary mismatch (includes FAT and Dir start) |
| Track 0 Head 1 R=7-10 | `same` | Bit-for-bit identical (end of Dir area) |
| Track 1 Head 0 R=1-10 | `same` | Bit-for-bit identical (includes FAM and bdir) |

## Filesystem-Relevant X1 Ports
The following ports are documented as being directly involved in disk I/O, boot ROM mapping, or DMA-based transfer.

| Port Address | Label | Usage in X-DOS | Description |
| :--- | :--- | :--- | :--- |
| `0FF8H` | `fdc_status_cmd` | Confirmed | MB8877A Status (R) / Command (W) (Seen in 01 F8 0F) |
| `0FF9H` | `fdc_track` | Probable | FDC Track Register |
| `0FFAH` | `fdc_sector` | Probable | FDC Sector Register |
| `0FFBH` | `fdc_data` | Probable | FDC Data Register |
| `0FFCH` | `fdc_control` | Probable | Drive Select, Side Select (bit 4), Motor latch (Seen as 0E FC) |
| `1D**H` | `ipl_rom_on` | Probable (boot) | Enable IPL ROM mapping (0000-7FFF) |
| `1E**H` | `ipl_rom_off` | Probable (boot) | Restore RAM at 0000-7FFF |
| `1F80H` | `dma_ctrl` | Probable (turbo+) | Z80 DMA for fast data transfer |
| `0B00H` | `bank_switch` | Probable (turbo+) | Extended memory/buffer switching |

**Classification Note**:
- **X-DOS-usage-confirmed**: Directly seen in salvaged assembly or primary artifacts.
- **X-DOS-usage-probable**: Logically necessary for the target hardware platform or hinted at by patterns (e.g., bit 4 toggle at `side_select_logic`).
- **Hardware-known**: Documented for the machine but not yet linked to X-DOS kernel code.

## 2D X-DOS Boot and Clone Conditions (Evidence-Graded Reconciliation)

This section evaluates the conditions currently proven necessary or sufficient for a bootable 2D X-DOS clone, separating verified facts from working assumptions based on tracked analysis assets.

### 1. Confirmed Facts (Re-stated)
- **Directory Entry Boundary**: Fixed at 32 bytes, with filename at offset 2 (length 16).
- **0x1D/0x1E Pair Handling**: The 16-bit word at `0x1D/0x1E` is confirmed to perfectly match the file's first observed placement pair. The kernel accesses this pair via `add hl, 0x1D` in `helper_d6af`.
- **Observed Placement Detection**: The file payload's physical location on disk can be empirically found and maps directly to the `0x1D/0x1E` metadata.
- **Geometry Translation**: The observed placement pair derives from the raw D88 header tuple using the exact flat transform `(C * 2 + H, R)`. This assumes the double-sided layout defined for 2D media.

### 2. Confirmed Clone Conditions
*(These conditions are definitively proven as necessary based on raw evidence)*
- **Boot Tracks Requirement**: Track 0 (IPL) and Track 1 (FAT/Dir) must be exactly reproduced at their physical record positions. A clone cannot be recognized as bootable without this structural anchor.
- **Placement Metadata Consistency**: Any written file must have its starting physical location `(C, H, R)` translated via `(C * 2 + H, R)` and stored accurately at directory offset `0x1D/0x1E`.

### 3. Provisional Clone Conditions
*(These conditions are highly likely based on indirect evidence or tool behavior, but lack direct kernel-level proof)*
- **Shared Placement Requirements**: The empirical evidence strongly suggests that dense system disks (like `XDOS_SYS.D88`) require shared-cluster logic because a naive "one cluster per file" clone exceeds the 80-track limit of 2D media.
- **Write-Side FAM Operations**: The kernel's use of nibble-swapping (`helper_c934`) implies a packed FAM structure. It is provisional that recreating this packed allocation map perfectly is required for the kernel to write reliably.

### 4. Unknown Clone Conditions
*(These aspects remain unproven and block a guaranteed fully-functional clone)*
- **Is `boot-copy + file copy` proven sufficient?**: **Unknown**. While we can copy files and update the `0x1D/0x1E` pair, we do not yet know if the kernel requires additional FAT/FAM manipulation or specific metadata at `0x1A` or `0x1B/0x1C` to consider the files fully valid.
- **Is the first observed placement alone enough to infer full runtime traversal?**: **Unknown**. The `0x1D/0x1E` pair proves the starting location, but the explicit downstream translation for file continuation (how `helper_d6af` calculates subsequent clusters/sectors) remains unproven.
- **Are shared placement requirements proven?**: **Unknown**. We observe multiple files occupying sectors in the same logical track, but the exact bit-level logic for resolving shared space occupancy in the FAM/FAT during a read or write operation is not yet proven.
- **Can any write-side requirement be stated safely?**: **Unknown**. Beyond the necessity of creating a valid directory entry and updating the FAT/FAM, the exact constraints and safe assumptions for shared-cluster allocation on write are fully unknown.

---
**Note**: Unrelated local changes were not reset or cleaned during this analysis. Prior read/write sections were preserved.

## C# Implementation Specification for 2D X-DOS Duplication

This section outlines the decision-complete specification for achieving viable 2D X-DOS duplication in the C# line, based on evidence-graded kernel analysis and current filesystem limitations.

### 1. Read-Side Behavior (Safe to Implement/Preserve)
- **Preserve Current Mapping**: The exact flat translation `(C * 2 + H, R)` is mathematically sound for 2D media and perfectly aligns with observed placement data in system disks. This must be preserved as the core geometry transformation.
- **Preserve Directory Extent Parsing**: The 32-byte directory boundary and established fields (filename, type, size) are confirmed safe and required.
- **Preserve FAM-Based Read Traversal**: While shared-cluster write logic is unknown, following the existing single-byte FAM chain for sequential reading is structurally stable and sufficient for extracting contiguous or chained files.

### 2. Write-Side Behavior (Must Change for Viability)
- **Abandon Naive Allocation for System Disks**: A naive "one cluster per file" allocation safely implemented for user files exceeds the physical 80-track capacity of 2D media when cloning dense system disks like `XDOS_SYS.D88`.
- **Implement Explicit Logical Duplication**: The write flow must bypass naive cluster allocation and allow forcing the exact target cluster and sector for a file (`forcedStartTrack`, `forcedStartSectorR`, `forcedClusterChain`), perfectly mirroring the source metadata.
- **Bypass Sector Pre-Clear on Overlapping Data**: The directory track (Track 1) overlaps with payload data in system setups. The duplication sequence must be able to write payload without corrupting or blindly zeroing out the shared directory sectors on the same track.

### 3. Boot-Related Invariants (Must be Preserved)
- **IPL and FAT/Directory Alignment**: Track 0, R=1 (Volume Record/IPL) and the entirety of Track 1 (FAT/Directory) and Track 2 (FAM/bdir) must be structurally cloned sector-by-sector to preserve bootability.
- **Observed Placement Consistency**: The 16-bit metadata at directory offset `0x1D/0x1E` must equal the exact physical start coordinates flattened by `(C * 2 + H, R)`.

### 4. Guarded Logic (Must Remain 'Unknown' and Speculative)
- **Shared-Cluster Semantics**: Do not implement a speculative logic for determining how new files can dynamically share a single track during write operations. Rely strictly on copying the raw FAT/FAM sectors from the source image.
- **Semantic meaning of Directory Bytes `0x1B/0x1C`**: Do not assign definitive functional behavior to these bytes despite their observed cross-disk stability.
- **Downstream Translation inside `helper_d6af`**: Do not model internal kernel FDC dispatch behavior directly.

### 5. Assumption Disposition
- **Rejected**: The assumption that standard sequential cluster allocation (1 track = 1 file minimum) can successfully rebuild a bootable 2D system disk from scratch.
- **Preserved**: The physical necessity of exactly replicating the `0x1D/0x1E` pair with `(C * 2 + H, R)` mapping.
- **Deferred**: The bit-level logic for safely allocating new user-generated files alongside system files in overlapping/shared clusters.

### 6. C# Implementation Sequence
1. Introduce a dedicated `Duplicate` or `Clone` API at the filesystem or application layer that formalizes logical reconstruction.
2. Update the cloning write sequence to mask or ignore payload writes that overlap with reserved directory sectors (Track 1, R=2..10).
3. Utilize existing `forcedStartTrack` and `forcedClusterChain` APIs to extract and write each file strictly at its original physical coordinates.
4. Clone the raw FAT, FAM, and Directory sectors directly from the source image to the destination, overriding any intermediate state generated by the file writes.
5. Clone the boot tracks (Track 0) identically.

### 7. Required Test Matrix
- **Unit Layer**: 
  - Test calculation of capacity boundaries.
  - Test the logic that filters or masks data writes targeting the Track 1 directory sector span.
- **Sample-Image Regression**: 
  - Perform logical duplication of `XDOS_SYS.D88` and `XDOSUTIL.D88` into new `2D` images.
  - Perform automated bit-for-bit extraction of the cloned files and ensure they match the source files exactly.
- **Standalone CLI E2E**: 
  - Verify that running the local CLI release tool on a cloned system image successfully detects the X-DOS filesystem, parses the directory, and extracts files without error.

## Primary-Evidence Catalog (X-DOS Files)

This section provides raw binary observations of representative files across `XDOS_SYS.D88` and `XDOSUTIL.D88`.

### 1. XDOS_SYS.D88 Catalog

| Filename | Dir Entry Base Offset | 0x1A..0x1E | 0x1D/0x1E | First Observed Placement Pair | FAM Window (at 1D offset) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `X-DOS System` | `0x80` | `BA D5 80 02 01` | `(02, 01)` | `(02, 01)` | `02 02 09 03 01 0A 04 01 01` (Offsets 0x00-0x08) |
| `SX-BASIC` | `0xe0` | `97 67 80 06 08` | `(06, 08)` | `(06, 08)` | `04 01 01 00 00` (Offsets 0x06-0x0A) |
| `Overlay module` | `0x120` | `BC A6 80 09 02` | `(09, 02)` | `(09, 02)` | `0A 04 01 01 00` (Offsets 0x09-0x0D) |
| `XEDIT` | `0x1a0` | `0A 87 80 10 01` | `(10, 01)` | `(10, 01)` | `00 00 00 00 00` (Offsets 0x10-0x14) |

### 2. XDOSUTIL.D88 Catalog

| Filename | Dir Entry Base Offset | 0x1A..0x1E | 0x1D/0x1E | First Observed Placement Pair | FAM Window (at 1D offset) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `X-DOS System` | `0x80` | `BA D5 80 02 01` | `(02, 01)` | `(02, 01)` | `02 02 09 03 01 0A 04 01 01` (Offsets 0x00-0x08) |
| `SX-BASIC` | `0xa0` | `97 67 80 04 02` | `(04, 02)` | `(04, 02)` | `01 0A 04 01 01` (Offsets 0x04-0x08) |
| `Overlay module` | `0xe0` | `BC A6 80 06 06` | `(06, 06)` | `(06, 06)` | `04 01 01 00 00` (Offsets 0x06-0x0A) |
| `AUTO RUN.BAS` | `0xc0` | `A4 61 00 06 04` | `(06, 04)` | `(06, 04)` | `04 01 01 00 00` (Offsets 0x06-0x0A) |

### 3. Unknowns

- **FAM-Area Semantics**: The meaning of bytes in the FAM window (Track 2, Sector 1) is unknown. No allocation chain or traversal model is implied.
- **Directory Bytes 0x1A..0x1C**: The roles of these indices are unknown.
- **File Length**: The representation of file length in the directory entry is unknown.

---
**Note**: Unrelated local changes were not reset or cleaned during this operation.

## Raw FAM Window Patterns (Analysis-Only)

This section documents raw byte relationships in the FAM area (Track 2, Sector 1) at the offsets indicated by the directory entry's `0x1D` byte.

### Cross-Disk Comparison (Same Filename)

| Filename | Disk | 1D Offset | Raw FAM Window (8 bytes) | Relationship |
| :--- | :--- | :--- | :--- | :--- |
| `X-DOS System` | `XDOS_SYS.D88` | `0x02` | `09 03 01 0A 04 01 01 00` | `same` |
| `X-DOS System` | `XDOSUTIL.D88` | `0x02` | `09 03 01 0A 04 01 01 00` | `same` |
| `SX-BASIC` | `XDOS_SYS.D88` | `0x06` | `04 01 01 00 00 00 00 00` | `different` |
| `SX-BASIC` | `XDOSUTIL.D88` | `0x04` | `01 0A 04 01 01 00 00 00` | `different` |
| `Overlay module` | `XDOS_SYS.D88` | `0x09` | `00 00 00 00 00 00 00 00` | `different` |
| `Overlay module` | `XDOSUTIL.D88` | `0x06` | `04 01 01 00 00 00 00 00` | `different` |
| `AUTO RUN.BAS` | `XDOS_SYS.D88` | `0x42` | `00 00 00 00 00 00 00 00` | `different` |
| `AUTO RUN.BAS` | `XDOSUTIL.D88` | `0x06` | `04 01 01 00 00 00 00 00` | `different` |

### Intra-Disk Comparison (XDOS_SYS.D88)

| Filename 1 | Filename 2 | 1D (1) | 1D (2) | Relationship |
| :--- | :--- | :--- | :--- | :--- |
| `X1-BIOS` | `SX-BASIC` | `0x06` | `0x06` | `same` |
| `Overlay module` | `Overlay moduleX1` | `0x09` | `0x0B` | `repeated` |
| `SYSUP` | `XASM` | `0x0B` | `0x0E` | `repeated` |

### Summary of Observed Raw Motifs
- **Cross-disk identity**: `X-DOS System` shows the same 8-byte raw window across the two sampled disks.
- **Window repetition**: identical 8-byte windows are observed for more than one sampled file or more than one sampled offset.
- **Offset-local observation**: the compared raw windows are collected from the FAM area using the sampled file rows above; no further semantics are claimed here.
- **Status**: the meaning of these windows is **unknown**.

## FAM Byte And Nibble Stability (Analysis-Only)

This section classifies observed stability patterns within the raw FAM area (Track 2, Sector 1) using sampled 8-byte windows starting at the directory index 0x1D. This is position-stability classification only and no semantics are assigned to any byte or nibble.

### FAM Window Sample Table (First 8 Bytes)

| Sample | Disk | Source File | 1D Offset | B0 | B1 | B2 | B3 | B4 | B5 | B6 | B7 |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| S1 | SYS | X-DOS System | 0x02 | 09 | 03 | 01 | 0A | 04 | 01 | 01 | 00 |
| S2 | UTIL | X-DOS System | 0x02 | 09 | 03 | 01 | 0A | 04 | 01 | 01 | 00 |
| S3 | SYS | SX-BASIC | 0x06 | 04 | 01 | 01 | 00 | 00 | 00 | 00 | 00 |
| S4 | UTIL | SX-BASIC | 0x04 | 01 | 0A | 04 | 01 | 01 | 00 | 00 | 00 |
| S5 | UTIL | Overlay module | 0x06 | 04 | 01 | 01 | 00 | 00 | 00 | 00 | 00 |
| S6 | UTIL | AUTO RUN.BAS | 0x06 | 04 | 01 | 01 | 00 | 00 | 00 | 00 | 00 |

### Observed Stability Classification

| Comparison Scope | Stability Label | Evidence |
| :--- | :--- | :--- |
| X-DOS System cross-disk same-file comparison | stable-byte | S1 vs S2 (at 1D offset 0x02) |
| same 1D offset 0x06 repeated-window comparison | stable-byte | S3 vs S5 vs S6 (all at 1D offset 0x06) |
| window positions B0..B7 high nibble | stable-high-nibble | Observed as 0x0 across all sampled windows in FAM sector |
| window positions B0..B7 low nibble | variable | Low nibbles vary based on the 1D offset value |

*Note: No semantic role is assigned to these positions. Classification tracks value stability only.*

## FAM 4-Bit Range Check (Analysis-Only)

- **Inspected Span**: Full 512-byte FAM sector (Track 2, Sector 1) on both disks.
- **Range Check Result**: `in-range` (`0x00..0x0F`).
- **Maximum Value Observed**: `0x0A`.
- **Exceeded 0x0F**: No.
- **Cross-Disk Status**: Identical. The entire 512-byte raw FAM sector is bit-for-bit identical between `XDOS_SYS.D88` and `XDOSUTIL.D88`.

*Note: This observation confirms that all sampled FAM bytes and the wider inspected window stay within the low 4-bit range, while semantic meaning remains unknown.*

## FAM Kernel-Side Value Handling (Analysis-Only)

- **Read-Path Byte Consumption**: Directly observed in `helper_d6af` for the directory-linked pair `0x1D/0x1E`.
- **Status**: Everything else remains unknown.

## FAM-Adjacent Addressing Arithmetic (Analysis-Only)

This section documents the addressing arithmetic directly observed within the `helper_d6af` kernel routine window.

| Address | Instruction | Context | Description |
| :--- | :--- | :--- | :--- |
| `0xD6AF` | `dec de` | `helper_d6af` entry | Neutrally observed register decrement. |
| `0xD6B0` | `dec de` | `helper_d6af` entry | Neutrally observed register decrement. |
| `0xD6C3` | `ld de, 0x001D` | Before structure access | Loading the directory entry offset for index 0x1D. |
| `0xD6C6` | `add hl, de` | Structure indexing | Offsetting the pointer in `HL` to access the target field. |
| `0xD6C7` | `ld d, (hl)` | Sequential load | Loading the first byte of the 16-bit pair. |
| `0xD6C8` | `inc hl` | Pointer stepping | Incrementing `HL` to the next adjacent address. |
| `0xD6C9` | `ld e, (hl)` | Sequential load | Loading the second byte of the 16-bit pair. |

- **Observation**: The `helper_d6af` routine uses standard Z80 pointer arithmetic and structure-relative indexing to access the directory-linked metadata. Read-path addressing arithmetic is directly observed in the reconstructed helper window.

## FAM Correlation Boundary (Analysis-Only)

This section defines the current evidence boundary for the relationship between directory metadata and raw FAM bytes.

- **Observed**: Raw 512-byte FAM windows are observed and consistent across disks.
- **Observed**: All FAM bytes in the full-sector 512-byte range stay within the 4-bit range (`0x00..0x0F`).
- **Observed**: Read-path byte consumption of the directory `0x1D/0x1E` pair is confirmed in the `helper_d6af` kernel routine.
- **Observed**: Read-path addressing arithmetic using the `0x1D` offset to index the entry is confirmed in `helper_d6af`.
- **Boundary**: A direct one-to-one correlation from the directory `0x1D/0x1E` pair to specific raw FAM byte positions within the sector remains unproven.

## Observed Shared Placement Cases (Analysis-Only)

This section catalogs observed cases where multiple files share the same track-level placement region or exact first placement pair within the sampled 2D X-DOS disks.

| disk | file A | file B (or more) | shared observed placement pair or shared placement region | evidence note |
| :--- | :--- | :--- | :--- | :--- |
| `XDOS_SYS.D88` | `X1-BIOS` | `SX-BASIC` | Track 0x06 | Shared track-level placement region |
| `XDOS_SYS.D88` | `Overlay moduleX1` | `SYSUP` | Track 0x0B | Shared track-level placement region |
| `XDOSUTIL.D88` | `AUTO RUN.BAS` | `Overlay module` | Track 0x06 | Shared track-level placement region |
| `XDOSUTIL.D88` | `XUTIL` | `GAME LOAD.DOC` | Track 0x09 | Shared track-level placement region |
| `XDOSUTIL.D88` | `MML.DOC` | `X.DOC` | Track 0x0A | Shared track-level placement region |
| `XDOSUTIL.D88` | `X.sub` | `Make X`, `X.sub2` | Track 0x0B | Shared track-level placement region (3 files) |
| `XDOS_SYS / XDOSUTIL` | `X-DOS System` | `X-DOS System` | `(02, 01)` | Shared first placement pair across different disks |
| `XDOS_SYS / XDOSUTIL` | `X-DOS System X1` | `SX-BASIC` | `(04, 02)` | Shared first placement pair across different disks |

## Shared Placement Boundary (Analysis-Only)

- representative raw shared-placement cases are cataloged
- shared track-level regions are observed
- cross-disk same-pair reuse is observed
- ownership rules remain unknown
- runtime resolution rules remain unknown
- write-side reconstruction rules remain unknown

## Shared Track Byte Pattern Check (Analysis-Only)

| disk | representative files | result | evidence note |
| :--- | :--- | :--- | :--- |
| `XDOS_SYS.D88` | `X1-BIOS` / `SX-BASIC` | `same-1D-different-1E` | Track 0x06: 1D/1E is `(06, 03)` and `(06, 08)`. |
| `XDOS_SYS.D88` | `Overlay moduleX1` / `SYSUP` | `same-1D-different-1E` | Track 0x0B: 1D/1E is `(0B, 07)` and `(0B, 06)`. |
| `XDOSUTIL.D88` | `AUTO RUN.BAS` / `Overlay module` | `same-1D-different-1E` | Track 0x06: 1D/1E is `(06, 04)` and `(06, 06)`. |
| `XDOSUTIL.D88` | `XUTIL` / `GAME LOAD.DOC` | `same-1D-different-1E` | Track 0x09: 1D/1E is `(09, 03)` and `(09, 09)`. |
| `XDOSUTIL.D88` | `MML.DOC` / `X.DOC` | `same-1D-different-1E` | Track 0x0A: 1D/1E is `(0A, 02)` and `(0A, 09)`. |
| `XDOSUTIL.D88` | `X.sub` / `Make X` / `X.sub2` | `same-1D-different-1E` | Track 0x0B: 1D/1E is `(0B, 01)`, `(0B, 03)`, and `(0B, 05)`. |

## Exact Pair Duplication Check (Analysis-Only)

| disk | result | evidence note |
| :--- | :--- | :--- |
| `XDOS_SYS.D88` | `not observed` | Full directory scan (Track 1, R=2..10) shows unique `0x1D/0x1E` pairs for all valid files. |
| `XDOSUTIL.D88` | `not observed` | Full directory scan (Track 1, R=2..10) shows unique `0x1D/0x1E` pairs for all valid files. |

## Shared Placement Summary Boundary (Analysis-Only)

- representative same-disk shared track-level cases are cataloged
- same-disk exact pair duplication is not observed for valid files
- cataloged shared-track cases currently fit `same-1D-different-1E`
- ownership/runtime/write reconstruction rules remain unknown

## Write Path Boundary (Analysis-Only)

- confirmed write-path entry windows are cataloged
- write-path helper windows are cataloged
- detailed FAM/FAT update semantics remain unknown
- write-side reconstruction ordering remains unknown
- failure/rollback behavior remains unknown

## Boot And Early-Area Boundary (Analysis-Only)

- boot and early-area observations are cataloged
- volume record, FAT area, directory start, FAM area, and boot copy region are observed at raw locations
- the exact boot rule and exact required clone reconstruction conditions remain unknown
- the exact full extent of the directory area remains unknown beyond the directly observed start

## Boot And Early-Area Summary Boundary (Analysis-Only)

- boot and early-area observations are cataloged
- early-area spans are cataloged
- cross-disk equality for sampled early-area regions is cataloged
- some sampled early-area regions are same and some are different
- the exact boot rule remains unknown
- the exact required clone reconstruction rule remains unknown
- the full exact extent of directory and adjacent management regions remains unresolved beyond the observed spans

## Implementation Reconciliation Matrix (Analysis-Only)

| implementation concern | current evidence grade | current boundary |
| :--- | :--- | :--- |
| directory entry structure | confirmed | 32-byte fixed block with filename at offset 2 (length 16) |
| initial placement metadata | confirmed | 16-bit pair at offset 0x1D/0x1E matching first observed placement pair |
| FAM sector byte range | confirmed | 512-byte span at Track 2 Sector 1 within 0x00-0x0F |
| sequential read traversal | unknown | downstream translation of 0x1D/0x1E for subsequent record offsets |
| shared placement resolution | unknown | bit-level logic for resolving shared track occupancy in FAM/FAT |
| write-side update logic | unknown | bit-level FAM/FAT modification sequence and field ordering |

## Downstream Read Traversal Windows (Analysis-Only)

| observed window | directly observed relation | evidence class |
| :--- | :--- | :--- |
| `0xD155` | `call target observed` | confirmed from `helper_d6af` |
| `0xE00E` | `call target observed` | confirmed from `helper_d6af` |
| `0xDEE8` | `call target observed` | confirmed from `helper_d6af` |
| `0xD753` | `jp target observed` | confirmed from `helper_d6af` |
| `0xD6AF` window | `downstream window cataloged` | confirmed from `sys_rdd_impl` |
| `0xDEE8` context | `control transfer observed` | confirmed from `helper_d6af` after 1D/1E load |

## Shared Placement Resolution Windows (Analysis-Only)

| observed window | directly observed relation | evidence class |
| :--- | :--- | :--- |
| `0xD6AF` | window cataloged | confirmed |
| `0xD155` | call target observed | confirmed |
| `0xE00E` | call target observed | confirmed |
| `0xDEE8` | call target observed | confirmed |
| `0xD753` | jp target observed | confirmed |
| `0xC9EA` | call target observed | confirmed |
| `0xEB32` | call target observed | confirmed |
| `0xD6AF` | adjacent control transfer observed | confirmed |

## FAM Pattern Closeout Boundary (Analysis-Only)

- raw windows are cataloged
- sampled byte/nibble stability is cataloged
- full-sector range stays within `0x00..0x0F`
- semantic interpretation remains unknown

## Analysis Closeout Boundary (Analysis-Only)

- **Directory Entry Structure**: analysis-complete; boundary established.
- **Initial Placement Metadata**: analysis-complete; raw catalog exists; boundary established.
- **FAM Sector Range**: analysis-complete; raw catalog exists; boundary established.
- **Boot and Early-Area Spans**: analysis-complete; raw catalog exists; boundary established.
- **Shared Placement Pattern**: analysis-complete; raw catalog exists; boundary established.
- **Write Path Entry Windows**: analysis-complete; raw catalog exists; boundary established.
- **Sequential Read Traversal**: blocked unknown.
- **Shared Placement Resolution**: blocked unknown.
- **FAM Pattern Semantics**: blocked unknown.
- **Write-Side Update Logic**: blocked unknown.
- **Boot Rule Invariants**: blocked unknown.



