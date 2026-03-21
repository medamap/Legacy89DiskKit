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

## Write Path Spec (Conservative Reconstruction)

This section consolidates the directly observed X-DOS write-path evidence into a role-split specification.

### 1. `sys_wopen` (Entry: 0xED78, Impl: 0xC876)
- **Direct Observation**:
    - Instruction sequence: `17 CD 34 C9 FE 13 20 17 CD 34 C9 B7 20 FA CD 7E C9`.
    - Disassembly highlights:
        - Entry with `17` (`rla`).
        - Multiple calls to `helper_c934`.
        - A conditional jump `jr nz, -6` that creates a loop calling `helper_c934` at `0xC87E`.
        - A final call to `helper_c97e`.
- **Instruction-Level Inference**:
    - `rla` at entry suggests the Carry flag might be used as a boolean input (e.g., "force create" or "append mode").
    - `cp 0x13` is a magic number check; in X-DOS, `0x13` could relate to a specific file type or system status.
    - The loop pattern `call helper_c934 / or a / jr nz, -6` indicates it's processing or waiting for a zero-terminator/success state from the helper.
- **Behavioral Hypothesis**:
    - Opens a file for writing.
    - Likely involves searching for an existing directory entry or allocating a new one, then initializing the FAM chain.

### 2. `sys_wrd` (Entry: 0xED7B, Impl: 0xC860)
- **Direct Observation**:
    - Instruction sequence: `CD 34 C9 B7 CA 38 C9 C9`.
    - Disassembly:
        ```asm
        call 0xC934   ; call helper_c934
        or a          ; test A
        jp z, 0xC938  ; mid-entry jump to helper_c934
        ret
        ```
- **Instruction-Level Inference**:
    - `sys_wrd` is highly optimized and shares logic with `helper_c934`.
    - The jump to `0xC938` bypasses the initial `ld (bc), a` and `jr c` instructions in `helper_c934`.
- **Behavioral Hypothesis**:
    - Writes data from memory (likely `sys_dtadr`) to the open file.
    - The delegation to `helper_c934` suggests the helper is the primary engine for updating the FAM/sector-chain.

### 3. Shared Write-Path Helper: `helper_c934` (Impl: 0xC934)
- **Direct Observation**:
    - Instruction sequence: `02 38 0D 0F 0F 0F 0F 4F 1A 13 CD EA C9 38 01 B1 C1 C9`.
    - Disassembly highlights:
        - `02`: `ld (bc), a` - Stores accumulator to memory at `BC`.
        - `38 0D`: `jr c, +0x0D` - Jump to `0xC944` (pop/ret) if Carry set.
        - `0F 0F 0F 0F`: Swaps nibbles of `A`.
        - `4F`: `ld c, a`
        - `1A`: `ld a, (de)` - Reads from `DE`.
        - `CD EA C9`: Call to sub-helper `0xC9EA`.
        - `C1`: `pop bc` - Stack cleanup.
- **Instruction-Level Inference**:
    - `BC` and `DE` are treated as pointers to kernel buffers (e.g., `fam_area` or `dir_area`).
    - The nibble swap (`rrca` x4) strongly suggests 4-bit or 12-bit data packing, common in legacy FAT or FAM structures.
- **Behavioral Hypothesis**:
    - This is a low-level "Update FAM Entry" or "Write Byte to Buffer" primitive.
    - It manages the in-memory representation of the file's allocation.

### 4. Stack Helper: `helper_c97e` (Impl: 0xC97E)
- **Direct Observation**:
    - Instruction sequence: `78 C1 B7 E1 C9`.
    - Disassembly: `ld a, b / pop bc / or a / pop hl / ret`.
- **Instruction-Level Inference**:
    - Standard utility for cleaning up the stack after a syscall.
    - Restores `BC` and `HL` from the stack.
    - Returns a status in `A` derived from `B`.
- **Behavioral Hypothesis**:
    - Final exit path for `sys_wopen` and potentially other complex syscalls.

---
**Write-Path Hypothesis vs. Observation**:
- **Observation**: `sys_wopen` and `sys_wrd` are tightly coupled via `helper_c934`. Both use `BC` and `DE` pointers.
- **Hypothesis**: X-DOS write operations are buffered in the `fam_area` (`0x7400`). `sys_wrd` updates this area, and physical write to disk (`sys_devo`) is likely deferred until the file is closed or the buffer is full.

---
**Note**: Unrelated local changes were not reset or cleaned during this analysis.

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

## 2D X-DOS Boot and Clone Conditions (Evidence-Graded Spec)

This section defines the requirements for a reliable 2D X-DOS bootable clone, based on the current state of analysis.

### 1. Kernel-Proven Facts (Architectural Constraints)
- **Logical Record Addressing**: X-DOS uses 1-based logical record numbers for system I/O (e.g., Record 10 = Track 1, R=1).
- **Track Layout**: Records 0-9 are reserved for Track 0 (IPL). Track 1 starts at Record 10. Track 2 starts at Record 20.
- **Record Mapping Formula**: `physical_track = (rec - 10) / 10 + 1` and `physical_R = (rec - 10) % 10 + 1`. This proves a **10-sector-per-track** logical geometry for 2D media.
- **Volume Record Location**: Always Track 0, Sector 1 (offset 0x10 from sector start). Contains format ID (`0x88`) and BCD date.
- **I/O Dispatch**: Kernel performs physical I/O via `sys_devi`/`sys_devo` using MB8877A ports (`0x0FF8-0x0FFB`).

### 2. Image-Level Observations (Static Artifacts)
- **Shared Cluster Occupancy**: In dense system disks like `XDOS_SYS.D88`, multiple files appear to occupy sectors within the same logical track/cluster (e.g., Track 51 contains portions of multiple small utility files).
- **Directory Metadata**: Files on `XDOS_SYS.D88` often have non-zero values at Directory Offset 29 (`0x1D`).
- **Physical Sector Headers (D88)**: Most 2D X-DOS images show physical sector IDs (R) from 1 to 40 in the D88 header, but the kernel's logical addressing (`10 sectors/track`) implies a higher-level record-to-sector translation or a specific interleaving strategy.

### 3. Tool-Observed Behavior (Empirical Evidence)
- **Capacity Failure**: A naive "one cluster per file" clone (where each file starts on a new logical track) exceeds the 80-track (80-cluster) limit of 2D media when cloning `XDOS_SYS.D88`.
- **Boot Success**: Track 0 (IPL) and Track 1 (FAT/Dir) must be exactly reproduced at their physical record positions for the disk to be recognized as bootable by the X1 IPL ROM and X-DOS kernel.

### 4. Strong Hypotheses (Working Assumptions)
- **Shared-Cluster Requirement**: A bootable clone of a dense system disk **requires** a writer capable of packing multiple files into a single logical track (cluster).
- **FirstSectorR Encoding**: The Directory byte at offset 29 (`0x1D`) is hypothesized to be the starting logical sector (R) within the cluster assigned to that file.
- **12-bit/4-bit FAM Packing**: The patterns in `helper_c934` (nibble swaps) suggest a packed FAM structure where cluster allocation is not a simple 1-to-1 byte map.

### 5. Critical Unknowns (Implementation Blockers)
- **Traversal Engine Logic**: The exact bit-level logic at `0xD6AF` that resolves a `(Cluster, FirstSectorR)` pair into a physical read command is not yet fully reconstructed.
- **Physical vs. Logical Geometry**: While logical records are 10/track, the relationship to physical D88 sector IDs (often 1-40 or 1-16) remains unproven. It is unknown if the kernel or a lower-level driver handles the `Record -> (Track, Side, Sector)` mapping for different density modes.

---
**Note**: Unrelated local changes were not reset or cleaned during this analysis. Prior read/write sections were preserved.
