# X-DOS Filesystem Technical Analysis

## Overview

X-DOS is a Sharp X1-exclusive operating system developed and distributed by C&S Soft (Regulus).
Its filesystem does not resemble any of the other formats supported by Legacy89DiskKit.
This document records the results of direct binary analysis of repository-local X-DOS system and utility disk samples,
supplemented by SLANG source code files salvaged from the
disk images themselves.

Analysis date: 2026-03-18
Analyzed by: Binary inspection via Python sector walk + salvaged SLANG source code

---

## Disk Geometry

Both disk images use D88 type `0x00` (2D) but employ a **mixed-geometry** sector layout that
differs from standard X1 2D:

| Physical track | Sector count | Bytes / sector | D88 N value | Content |
| :--- | :--- | :--- | :--- | :--- |
| Track 0 (C=0, H=0) | 16 | 256 | N=1 | IPL boot code + Volume Record |
| Track 1 (C=1, H=0) | 10 | 512 | N=2 | FAT + Directory |
| Track 2 (C=2, H=0) | 10 | 512 | N=2 | FAM + bdir + (possibly) file data |
| Track 3+ | 10 | 512 | N=2 | File data |
| (Track 79 max) | | | | Last observed data track |

The disk is effectively **double-sided 40-cylinder** (80 D88 tracks used in both disks).
Track 0 uses 256-byte sectors exclusively. Track 1 and above use 512-byte sectors exclusively.
Any code that assumes a uniform sector size will fail to read the Volume Record correctly.

---

## Logical Record Numbering

X-DOS defines a **logical record number** scheme that maps to physical (track, R) addresses.
This scheme was confirmed from SLANG source constants in `make_BGM` (salvaged from disk):

```
fat_rec  = 10   ; Track 1, R=1  (FAT bitmap)
dir_rec  = 11   ; Track 1, R=2  (Directory, first sector)
fam_rec  = 20   ; Track 2, R=1  (FAM = File Allocation Map)
bdir_rec = 21   ; Track 2, R=2  (bdir = binary system code)
```

**Formula** (confirmed for records ≥ 10):

```
physical_track = (record_number - 10) / 10 + 1
physical_R     = (record_number - 10) % 10 + 1
```

| Record | Physical track | Physical R | Content |
| :--- | :--- | :--- | :--- |
| 0–9 | Track 0 | R=1–R=10 (256B) | IPL (records 0–9 map to first 10 of 16 IPL sectors) |
| 10 | Track 1 | R=1 | FAT bitmap |
| 11 | Track 1 | R=2 | Directory sector 0 |
| 18 | Track 1 | R=9 | Directory sector 7 |
| 19 | Track 1 | R=10 | Directory sector 8 (last) |
| 20 | Track 2 | R=1 | FAM (File Allocation Map) |
| 21 | Track 2 | R=2 | bdir (binary system routine, Z80 code) |
| 22 | Track 2 | R=3 | First file data sector |

The system call `devi(adr, record, count)` reads `count` sectors starting at `record`
into memory at `adr`. The `devo` call is the mirror for write.

---

## Volume Record (Track 0, R=1, 256 bytes)

```
Offset  Size  Content
[0]       1   Record type identifier: 0x01
[1:17]   16   Disk label, ASCII, space-padded
               Example: "X-DOS        Sys" (XDOS_SYS.D88)
[2:18]       (same range as above — the name occupies [1] through [16])
[24]      1   Format type byte: 0x88 = Sharp X1 2D
[25:28]   3   BCD date: year, month, day
               Example: 0x24, 0x04, 0x17 = April 17, 1984
```

Bytes outside the ranges documented above are present in the sector but their semantics
are not yet confirmed. They must be treated as reserved during implementation.

---

## FAT Bitmap (Track 1, R=1, 512 bytes = logical record 10)

The FAT is a **flat allocation bitmap**. Each byte represents one cluster.

| Byte value | Meaning |
| :--- | :--- |
| `0x00` | Free cluster |
| `0x01` | Reserved (appears only at FAT index 1) |
| `0x4A` | Allocated / in use |
| `0x3F`, `0xC0`, `0xFF` | Observed near end of FAT; likely format-time guard markers |

In both analyzed disks, FAT[0]=0x00, FAT[1]=0x01, FAT[2..N]=0x4A (contiguous used range),
FAT[N+1..]=0x00 (free). The FAT does **not** encode cluster chains. It is purely an
occupancy bitmap; cluster chain navigation is handled by the FAM (see below).

---

## File Allocation Map — FAM (Track 2, R=1, 512 bytes = logical record 20)

The FAM is distinct from the FAT. It was confirmed to exist at Track 2, R=1 from the
`make_BGM` SLANG source (`fam_rec=20`).

Binary analysis of XDOSUTIL.D88 FAM:

```
First 16 bytes: 02 02 09 03 01 0a 04 01 01 00 00 00 00 00 00 00 ...
```

The FAM appears to encode a **cluster chain** for each file. The likely structure is:

```
FAM[cluster_index] = next_cluster_in_chain  (0x00 = end of chain / free)
```

Evidence: For "X-DOS System" (directory entry[29]=2), FAM[2]=0x09 → FAM[9]=0x00.
This gives a two-cluster chain: cluster 2 → cluster 9 → end.
The X-DOS System binary is ~40 KB; if each cluster = ~20 KB (4 tracks × 5120 B),
two clusters = ~40 KB matches the measured load/end address difference.

**Important**: the exact cluster-to-physical-track mapping formula is **not yet confirmed**
by analysis. The working hypothesis (1 cluster = 4 physical data tracks) is consistent
with measured file sizes for some files but has not been verified for all entries.
This must be confirmed from X-DOS documentation or source code before implementing write support.

The `bdir` at Track 2, R=2 (logical record 21) is Z80 machine code, confirmed by byte
inspection. It is not a data table. Its exact role is unclear; it may be the resident
directory management routine of X-DOS itself.

---

## Directory Entries (Track 1, R=2–10, 32 bytes each)

Up to 9 directory sectors × 512 bytes ÷ 32 bytes = **144 directory entries** maximum.

```
Offset  Size  Content
[0]       1   File type (see table below)
[1]       1   Attribute byte
[2:18]   16   Filename, ASCII, space-padded, NO extension concept
[20:22]   2   Load address, little-endian (binary files: load into this Z80 address)
[22:24]   2   End address (binary: load+filesize) or record descriptor (text files), LE
[24:26]   2   Execution address, little-endian
[26:28]   2   Checksum or timestamp fragment (do not rely on in current analysis)
[28]      1   Flags byte (0x80 is most common; exact bit semantics TBD)
[29]      1   First cluster index in FAM chain (the chain head, used with FAM to locate file data)
[30]      1   Starting sector R within the first cluster (observed always 0x01 = first sector)
[31]      1   Always 0x01 in observed data
```

A directory entry is empty/deleted if `entry[0]` is `0x00` or `0xFF`.
The byte `0xD5` at `entry[0]` has also been observed (possibly a deleted marker on some images).

### File Type Codes

| Value | Type | Notes |
| :--- | :--- | :--- |
| `0x01` | Sub-program / overlay | Address-space resident helper |
| `0x02` | BASIC text program | `end` field stores a record descriptor, not an end address |
| `0x03` | Binary (machine code) | `load`, `end`, and `exec` are all meaningful |
| `0x04` | Help / auxiliary data / documentation | `end` is a page count; actual content is text or structured data |
| `0x05` | Overlay / loadable system module | Loaded by X-DOS kernel on demand |
| `0x06` | Script / batch file | Equivalent to `.BAT` on MS-DOS |
| `0x07` | Core system file | X-DOS kernel, BIOS, and essential runtime components |

Values with bit 7 set (`0x85`, `0xAE`, `0xB4`, `0xC0`, `0xE3`, etc.) appear in directory
entries, particularly on game data disks. These are likely application-defined type
extensions. The implementation must not reject entries with unknown type bytes; it should
expose the raw byte to callers.

### Notable Filename Observations

- Filenames are 16 bytes, space-padded. There is no `.extension` separator.
- Some filenames contain Japanese text stored as Shift-JIS (garbled in ASCII view).
  Examples: `"X???.DOC"` entries in XDOSUTIL display as corrupted because they contain
  Shift-JIS characters in the filename field.
- Filename comparison must be byte-exact (including trailing spaces) or use the
  same space-stripping convention as X-DOS itself.

---

## System Call ABI (salvaged from `x-dos.h`, dated 89/06/05)

The following system call addresses were salvaged verbatim from the SLANG header file
`x-dos.h` found at track 62 of XDOSUTIL.D88 (SLANG = the C-like language for X-DOS):

```c
sys_color = 0xF8D0   /* screen color register */
sys_call  = 0xEDF0   /* generic OS call dispatcher: DE = entry point address */
sys_devi  = 0xED8D   /* device input:  HL=buf_addr, DE=record_no, A=record_count */
sys_devo  = 0xED90   /* device output: HL=buf_addr, DE=record_no, A=record_count */
sys_load  = 0xEDC0   /* load or save:  A=0→load, A=1→save, BC=filetype, DE=switch_addr */
sys_ftych = 0xEDE4   /* filetype change: input DE, output BC */
sys_ropen = 0xED96   /* open file for read */
sys_file  = 0xED84   /* set active filename: DE = pointer to filename string */
sys_cls   = 0xEDBA   /* clear screen: A = mode */
sys_error = 0xEDB7   /* display error: A = error code */
sys_rdd   = 0xED81   /* read data into memory */
sys_dtadr = 0xECE2   /* word: data load address (set before sys_rdd) */
sys_size  = 0xECE4   /* word: data size in bytes (set before sys_wrd) */
sys_exadr = 0xECE6   /* word: execution address (set for saves) */
sys_wopen = 0xED78   /* open file for write */
sys_wrd   = 0xED7B   /* write data from memory */
sys_palet = 0xEDBD   /* palette / graphics mode control */
```

### Calling Convention (Z80 registers)

System calls follow a Z80 register-passing convention:

| Register | Role in X-DOS calls |
| :--- | :--- |
| `A` | Mode selector (e.g., 0=load, 1=save) or record count |
| `BC` | File type code (for load/save operations) |
| `DE` | Record number, filename pointer, or secondary address |
| `HL` | Memory buffer address |
| `CY` (carry flag) | 1 = error occurred |
| `ZERO` flag | 0 = file already exists (for wopen) |

### System Call Patterns (from salvaged SLANG source)

**File load:**
```c
// set filename, open for read, set load address, read
sys_file(de=filename_ptr)      // sets active file name
sys_ropen()                    // open for read; CY=1 on error
mem[sys_dtadr] = load_address  // write load address into OS variable
sys_rdd()                      // read file data into load_address
```

**File save:**
```c
sys_file(de=filename_ptr)
sys_wopen(bc=filetype)         // open for write; ZERO=0 if file exists
mem[sys_dtadr] = start_address
mem[sys_size]  = end_addr - start_addr + 1
mem[sys_exadr] = exec_address
sys_wrd()                      // write data to disk
```

**Device sector I/O:**
```c
sys_devi(hl=buf_addr, de=record_number, a=record_count)  // read sectors
sys_devo(hl=buf_addr, de=record_number, a=record_count)  // write sectors
```

---

## Error Messages (salvaged from XDOSUTIL.D88, track 3, R=9)

The following error strings were found verbatim in the binary:

```
Device I/O Error
Device Offline
Bad File Descripter      (note: original spelling preserved)
Write Protect
Bad Record
Bad File Mode
Bad Allocation Table
File not Found
Device Full
File Exists
Reserved Feature
File not Open
Syntax Error
Bad Data
```

---

## Command Names (salvaged from XDOSUTIL.D88, track 3, R=8)

The following command name strings were found as a colon-delimited list:

```
CTYP  DEV    BACKUP  ASK    COPY   BGMS  RAMF   KMODE
TYPE  KEY    /       MKDIR  RMDIR  SET   SDEV   CLK
CP    DOSNO  FILES   CALL   LOAD   SAVE  SCREEN KILL
MON   CLS    NAME    BOOT   BASIC  JUMP  WIDTH  COPY
```

The `/` entry appears to be the directory listing command, functionally equivalent to `DIR`
on MS-DOS or `ls` on Unix.

---

## Filesystem Constants (salvaged from `make_BGM` SLANG source)

The `make_BGM` tool found at track 63 uses the following constants, confirming the internal
memory-mapped addresses used by X-DOS at runtime:

```c
defdev   = 0xED1E   /* default drive register */
dir_recn = 0xECA0   /* table: directory record number per drive (2 bytes per entry) */
dev_type = 0xED20   /* table: device type per drive (1 byte per entry) */
dir_work = 0x5000   /* work buffer for full directory read */
file_work= 0x9000   /* work buffer for file I/O */
fat_area = 0xEE00   /* FAT bitmap loaded here */
dir_area = 0x7000   /* directory sector buffer */
fam_area = 0x7400   /* FAM cluster chain buffer */
bdir_area= 0x7200   /* bdir system code buffer */
bdir_pt  = 0x7220   /* pointer within bdir area */
size     = 0xECE4   /* same as sys_size: file byte count */
```

Drive type values observed in sources:
- `dev_type == 2` → directory occupies 15 sectors (large disk)
- otherwise → directory occupies 9 sectors (standard disk)

---

## Known Unknowns

The following aspects of the X-DOS filesystem were **not conclusively resolved** by
the current analysis. These must be verified against X-DOS system documentation or
additional disk images before write support can be implemented safely.

1. **Cluster-to-physical-address mapping**: The working hypothesis is 1 cluster = 4 data
   tracks = 20480 bytes, with data tracks numbered from track 3 onward. This is
   consistent with measured file sizes for "X-DOS System" (2 FAM clusters = ~40 KB)
   but has not been verified for all directory entries across both disks.

2. **FAM backward/forward chain direction**: The FAM chain direction (whether `FAM[N]`
   gives the next or previous cluster) has not been definitively confirmed from read
   behavior. The forward chain interpretation (FAM[N] = next cluster, 0x00 = chain end)
   is assumed.

3. **Directory entry[28] flag bits**: The most common value `0x80` has been observed but
   its bit semantics are unknown. Values `0x00`, `0x10`, `0x40`, `0x90`, `0xC0` also
   appear with no clear pattern yet identified.

4. **Directory entry[29] and entry[30] exact encoding**: Multiple files share the same
   `entry[29]` value with different `entry[30]` values (e.g., four files all have
   `entry[29]=0x44` with `entry[30]` = 0x03, 0x05, 0x07, 0x09). This makes
   `entry[29]` look like a shared FAT/FAM boundary marker and `entry[30]` like a
   per-file starting-sector offset within the referenced cluster. Neither role has
   been confirmed by a complete end-to-end file extraction. The current implementation
   hypothesis (`entry[29]` = first FAM cluster chain head, `entry[30]` = sector R
   within that cluster) should be treated as provisional.

5. **Track 0 record numbering**: Records 0–9 map to Track 0 sectors, but whether they
   correspond to R=1–R=10 or R=2–R=11 (skipping the Volume Record at R=1) is not yet
   confirmed.

6. **Japanese filename encoding**: Some filenames contain Shift-JIS bytes in the 16-byte
   filename field. The exact code page and encoding handling (mapping to current character
   encoding infrastructure) must be determined.

---

## Source Code Salvaged from Disk

The following source and documentation files were successfully extracted from XDOSUTIL.D88:

| File | Location (D88 track_idx, R) | Content |
| :--- | :--- | :--- |
| `x-dos.h` | track_idx=62 (C=31,H=0), R=2 | X-DOS system call header for SLANG (full text, see above) |
| Life game demo | track_idx=62 (C=31,H=0), R=7 | Demo SLANG program using X-DOS screen primitives |
| `make_BGM` | track_idx=63 (C=31,H=1), R=3 | BGM batch-conversion tool; reveals FAT/FAM I/O patterns |
| `INTMOUSE.ASM` | track_idx=65–67 (C=32,H=1 – C=33,H=1) | Z80 assembly: mouse interrupt handler (Ver3.0, ©1990 REA) |
| `INTMOUSE.DOC` | track_idx=67 (C=33,H=1), R=7–8 | Mouse driver documentation (see below) |
| `MKPCM.DOC` | track_idx=67 (C=33,H=1), R=10 | MKPCM tool doc (X68K PCM→X-DOS converter, by Regulus 90/06/30) |
| `NEW_CMD.DOC` | track_idx=68 (C=34,H=0), R=2 | New command reference: TORGB, PALINIT, MKPCM, SCE, SCH, SCS, PCM |
| `PALINIT.DOC` | track_idx=68 (C=34,H=0), R=10 | PALINIT Ver1.01 documentation (X1turboZ palette initializer) |

These sources are on the disk as type `0x04` (data/help) files and were extracted by
raw sector read starting at the identified physical track address.

### INTMOUSE.DOC (Salvaged)

```
        X-DOS Mouse Driver Document

  Entry points (ORG 8000H):
    JP 8000H  : INT Mouse (interrupt handler, call via CALL)
    JP 8003H  : RESMS    (reset/disable mouse)
    JP 8006H  : MOUSE    (foreground mouse poll, wait for button press)

  Subroutines:
    CALL CUR_V0 : Show mouse cursor
    CALL CUR_V1 : Hide mouse cursor

  Mouse Work Area:
    TIME:   DB 0A7H       ; CTC Timer Count
    WDOG:   DB 0B4H       ; Watch Dog Timer
    FLAG:   DS 1          ; Mouse View Flag (00H=visible, else hidden)
    STAT:   DS 1          ; Mouse Switch / Click State
    XX:     DS 2          ; Mouse X coordinate
    YY:     DS 2          ; Mouse Y coordinate
    MOVX:   DS 1          ; Distance moved X
    MOVY:   DS 1          ; Distance moved Y
    M_MASK: DS 1          ; Mouse Mask (00H–80H)
    S_ADR:  DS 2          ; Screen VRAM address
    CUR0-CUR7: 8-byte cursor bitmaps (one per X-shift offset 0–7)

  Hardware:
    CTC channel: BC=1FA1H (channel 1), BC=1FA2H (channel 2)
    SIO port:    BC=1F93H
    VRAM access: direct I/O port per X1 hardware convention
```

### MKPCM.DOC (Salvaged)

```
MKPCM doc by Regulus 90/06/30

MKPCM converts X68000 PCM data to X-DOS playable format.

  Usage:  CALL "MKPCM,file1,file2"
    file1 ... X68K PCM source file
    file2 ... output file (X-DOS format)

  Notes:
    - Converted PCM data can be LOADed and played on X-DOS
    - Output file size is approximately 48K bytes max
    - Requires X1 at 8MHz for reliable playback
```

### NEW_CMD.DOC (Salvaged)

```
New command doc

  TORGB   : X1turboZ — convert to 4096-color 400-line RGB format
  PALINIT : X1turboZ — palette initializer; see PALINIT.DOC
              usage: call "PALINIT,?"  (show help)
  MKPCM   : PCM data converter; see MKPCM.DOC

New sub doc (sub-commands/loaders):
  SCE  : MSX2 screen8 mode loader
  SCH  : X1turboZ — MSX2 screen8 mode loader
  SCS  : X1turboZ (turbo series) — MSX2 screen7 loader
  PCM  : PCM loader (see MKPCM.DOC)
```

### PALINIT.DOC (Salvaged)

```
PALINIT Ver1.01 for X1turboZ  By POGE

  Usage  : palinit [,option]
  Option : (default = ,g,t)
    ,?  : print this message
    ,g  : initialize graphic palette
    ,t  : initialize text palette
```

---

## Implementation Priority Notes

For the Legacy89DiskKit implementation (Phases XD-01 through XD-05 in Roadmap_V2.md):

1. **Read-only support first**: Implement read (list, extract) before write. The cluster
   addressing unknowns affect write but not read-path viability.

2. **Volume Record detection**: `entry[0]==0x01` at Track 0, R=1 plus format byte
   `0x88` at offset 24 is the reliable detection heuristic.

3. **FAM chain traversal**: Starting from `dir_entry[29]` (first cluster), follow
   `FAM[cluster]` until `0x00` to enumerate all clusters for a file.

4. **Sector-by-sector extraction**: Within each cluster, read all 10 sectors (512B each)
   in order R=1..R=10 until the file's load/end byte count is satisfied.

5. **16-char filename, no extension**: The current `IFileSystem` abstraction assumes
   8-character or 8.3 names for some operations. X-DOS requires a 16-character name
   field with no extension at the domain layer.
