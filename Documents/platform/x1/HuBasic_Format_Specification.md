# X1 Hu-BASIC (CZ-8FB01/02) Disk Format Specification

This document details the physical and logical structure of disks formatted for Sharp X1's Hu-BASIC, based on official specifications and reverse-engineering findings.

## 1. Physical Format (Sectors, Cylinders, Heads)

X1 2HD disks ("high-density, double-sided") have a total storage capacity of approximately 1MB. The physical layout is as follows:

- **Heads (Sides):** 0 and 1 (front and back of the disk)
- **Cylinders (Tracks):** 0 to 76 (77 concentric tracks per side)
- **Sectors:** 1 to 26 sectors per track
- **Sector Size:** Basically 256 bytes per sector

There are two distinct physical formatting styles for X1 2HD disks:
1. **X1 Format:** Double-density recording at 256 bytes/sector across ALL cylinders and heads.
2. **Standard Format (8-inch compatible):** 
   - **Head 0, Cylinder 0, Sectors 1-26:** Single-density recording at **128 bytes/sector**.
   - **All other areas:** Double-density recording at 256 bytes/sector.
   *(Note: Most commercially available 2HD blank disks are initialized in this Standard Format.)*

## 2. Logical Format (Records and Clusters)

When reading and writing from BASIC, disks are managed via sequential "logical addresses" rather than direct physical addresses (C/H/R).

- **Record:** The basic unit of read/write operations. 1 Record corresponds to 1 logical sector (256 bytes). Managed by sequential Record Numbers starting from 0.
- **Cluster:** The minimum unit of file management. In Hu-BASIC on 2HD, **16 Records (4,096 bytes or 4KB)** are grouped together as 1 Cluster. Even a 1-byte file will consume an entire 4KB cluster on a 2HD disk.

The 2HD disk space is partitioned sequentially by Record Numbers:
- **System Area (Boot/IPL):** Record Number 0
- **FAT (File Allocation Table) Area:** Record Numbers 28 to 29
- **Directory Area:** Record Numbers 32 to 47 (16 records total)
- **Data Area:** Record Numbers 48 to 4003

## 3. FAT (File Allocation Table) Structure

The FAT area (Records 28-29 on 2HD) manages cluster allocation and cluster chains for fragmented files. Because 2HD disks contain more clusters, FAT entries are 2 bytes long (Little-Endian).

Values in the FAT signify the following status for a cluster:
- **`00H`:** Unused (free) cluster.
- **`03H - 7FH` (and `100H - 179H` etc.):** A pointer to the next cluster in the file chain.
- **`80H - 8FH`:** Indicates the end of the file chain (Terminal Flag). 
  - The exact number specifies the number of active records used within this final cluster. 
  - Calculation: The value minus `7FH` (Result: 1 to 16) equals the number of actively used Records (sectors) in this last cluster. Example: `83H - 7FH = 4`, meaning 4 records are used.

*Note for >128 cluster disks (like 2HD): Cluster numbers ending with lower bytes `80H - FFH` are skipped to prevent collision with terminal flags.*
*Reserved System Area Clusters: Typically the FAT chain from `0x01` points to `0x8F` to mark the system area as reserved and non-allocatable for user data.*

## 4. Directory Area Structure

The Directory area (Records 32-47) stores file metadata. Each file entry consumes exactly 32 bytes. On a 2HD disk, the root directory can manage a maximum of 247 files.

**32-Byte Entry Layout:**
- **Byte 0 (File Attributes):**
  - `00H` or `E5H`: Deleted (Killed) file or unused area.
  - `FFH`: End of the active directory table.
  - `Bit 0`: Machine Language file (Bin) (`01H`)
  - `Bit 1`: BASIC flat file (Bas) (`02H`)
  - `Bit 2`: ASCII text file (Asc) (`04H`)
  - `Bit 4`: Secret file (hidden from FILES command)
  - `Bit 5`: Read-after-write verification flag
  - `Bit 6`: Write-protected
  - `Bit 7`: Sub-directory flag
- **Bytes 1-13 (13 bytes):** File Name (Zero-padded or Space-padded, strictly using X1 character encoding which includes Katakana)
- **Bytes 14-16 (3 bytes):** Extension (e.g., `Sys`, `Bas`)
- **Byte 17:** Password block (Defaults to `20H` if no password)
- **Bytes 18-19 (2 bytes):** File size in bytes (Little-Endian)
- **Bytes 20-23 (4 bytes):** Memory Load Address (2 bytes) and Execution Address (2 bytes) (Valid only for Machine Language `.COM`/`.SYS` files)
- **Bytes 24-28 (5 bytes):** Creation Timestamp (Year, Month, Day of week, Day, Hour, Minute)
- **Bytes 29-31 (3 bytes):** Start Cluster Number (Little-Endian). **CRITICAL EXCEPTION**: For Boot Programs (Record 0), this is NOT a Cluster Number but a direct **Record Number**.

## 5. IPL (Boot Programs) and Record 0

When the X1 boots, the IPL (Initial Program Loader) ROM reads exactly **Record Number 0** (the System Area). It searches for a valid Boot Program to load the OS into memory.

The IPL expects the entry at Record 0 to behave like a standard directory entry but with strict, hardcoded rules:
1. **Attribute:** Byte 0 MUST be `01H` (Machine Language/Bin).
2. **Extension:** Bytes 14-16 MUST be exactly `Sys`.
3. **Location Pointer (Bytes 29-31):** Instead of pointing to the Start Cluster, this field **MUST contain the absolute starting Record Number** of the OS binary on the disk. For example, since the 2HD Data Area starts at Record 48, this field is typically `48` (`30 00 00` in hex).
4. **Contiguity:** The boot program (OS) must exist in perfectly contiguous physical records on the disk starting from that specific record number, as the IPL does not parse the FAT. 
