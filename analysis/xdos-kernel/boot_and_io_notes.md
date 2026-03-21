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

## Unresolved Areas
- **Track 0 Mapping**: Exact correspondence of logical records 0-9 to physical sectors (R=1 or R=2 start).
- **Cluster 2 Role**: Both FAM (Track 2, R=1) and bdir (Track 2, R=2) are logically associated with Cluster 2 in some contexts, but FAM is also accessed via logical record 20.
- **2HD Extensions**: Whether these logical record numbers and mapping formulas scale linearly for 2HD media (16 sectors/track).

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
