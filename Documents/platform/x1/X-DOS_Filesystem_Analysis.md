# X-DOS Filesystem Format Specification (Working Reconstruction)

## Scope

This document reconstructs the X-DOS on-disk filesystem format from:

- direct repository-local disk analysis of X-DOS system-disk samples and related media
- previously collected reverse-engineering notes under `analysis/xdos-kernel/`
- contemporary printed documentation visible in the user-provided `X1通信研究所 3` pages 58-60

The goal of this document is not to preserve every historical hypothesis. It is to state the
best current format specification, with unresolved areas clearly marked.

## Confidence Legend

- **Confirmed**: directly stated in the printed source and/or matched by current disk evidence
- **Strongly supported**: not spelled out in one sentence, but strongly constrained by the printed source and observed bytes
- **Unresolved**: still not safe to encode as a final implementation rule

## High-Level Model

X-DOS manages files using three structures:

- **FAT**
- **Directory**
- **FAM**

This is explicitly stated in the printed source and matches the current reverse-engineering work.

## Final Working Memo

This section is the short implementation-facing summary of the current best understanding.

### Media Geometry

- 2D / 2DD: 512 bytes per sector, 10 sectors per track
- 2HD: 512 bytes per sector, 16 sectors per track

### Reserved Area and Data Area

- Track 0 is reserved
  - Sector 1: function-key related area
  - Sector 2: auto-run command area
- Track 1 is reserved
  - Sector 1: FAT
  - Sector 2 and later: Directory
- Track 2 and later: data area

### Sector and Record

For current X-DOS analysis, **sector** and **record** should be treated as the same 512-byte unit.

- `sector` is the physical disk-facing term
- `record` is the X-DOS logical numbering term

The printed `@GETREC` explanation explicitly connects `(track, sector)` to a returned record number,
which makes this equivalence operationally important.

### FAM

- FAM describes the file's occupied record chain
- FAM is encoded as repeating 3-byte groups:
  - track
  - sector
  - record_count
- FAM terminates with `00`
- A small file still consumes at least 1 KB because both:
  - file data
  - FAM data
  occupy record space

### Directory

- Directory begins at Track 1, Sector 2
- First 80 bytes are the disk title area
- Bytes at `0x0060-0x0062` are:
  - disk type
  - directory attribute
  - directory version
- Actual file entries begin at `0x0080`
- Each directory entry is 32 bytes
- One 512-byte sector therefore holds 16 entries

Directory entry fields:

- `0x00-0x01`: file type (2 bytes, read in written order; do not swap)
- `0x02-0x11`: file name (16 bytes, space-padded)
- `0x12-0x13`: start address
- `0x14-0x15`: file size low 16 bits
- `0x16-0x17`: execution address for normal binaries
  - for large ASCII-file cases, `0x14-0x17` together express file size
- `0x18-0x1B`: timestamp (4 bytes)
- `0x1C`: file attribute
  - bit 7 = secret
- `0x1D-0x1F`: start FAM position
  - track
  - sector
  - record

Directory markers:

- killed file: file type becomes `0000`
- directory end: `FFFF`

### Directory Control Bytes

- `0x0060`: disk type
  - `0x00` = 2D
  - `0x01` = 2DD
  - `0x02` = 2HD
  - `0x03` = RAM disk
- `0x0061`: directory attribute
  - still unresolved
- `0x0062`: directory version
  - still unresolved

### File Attribute Bits

- bit 7 = secret
- bit 6 = write protect
- bit 5 = system
- bit 4 = kanji
- bits 0-3 = user attribute

### File Type Classes

- `0x0000`: killed file marker
- `0x0100`: BIN / object file
- `0x0200`: BAS / BASIC file
- `0x0300`: CMD / user command file
- `0x0400`: ASC / text file
- `0x0500`: SUB / user file
- `0x0600`: BAT / batch file
- `0x0700`: SYS / X-DOS system family
- `0x0800`: DIC / Xsystem dictionary
- `0x8000`: DIR / subdirectory

Known documented subtypes:

- `0x0310`: SX-BASIC
- `0x0311`: XASM
- `0x0312`: XEDIT
- `0x0313`: SLANG
- `0x0501`: printer-related subtype
- `0x0510-0x0517`: overlay module 0-7 (turbo / MZ)
- `0x0518-0x051F`: overlay module 8-F (normal X1)
- `0x0520-0x052F`: access module 0-15
- `0x0700`: X-DOS System (turbo)
- `0x0701`: X-DOS System (normal X1)
- `0x0702`: X-DOS System (MZ-2500)

### User File Type Encoding

User-defined file types use a 15-bit three-character code:

- top bit is always `1`
- remaining bits encode 3 characters
- 5 bits per character
- `1-26` represent `A-Z`
- `0` represents `@` with special handling

This behaves like a three-letter extension-class namespace rather than a built-in system file class.

### FAT

- FAT is stored at Track 1, Sector 1
- byte `0x0000`: media type
  - `0` = 2D
  - `1` = 2DD
  - `2` = 2HD
- byte `0x0001`: X-DOS recognition mark, normally `1`
- bytes after that include per-track format descriptors
  - `bit7-6 = length`
  - `bit5-0 = sector count`
  - 2D / 2DD should be `0x4A`
  - 2HD should be `0x50`
  - the printed source says the formatter has a bug here and the running system does not rely on these bytes
- the real allocation bitmap begins at relative offset `0x00A8`
- the real FAT uses 2 bytes per track
- each bit represents one sector
  - `0 = used`
  - `1 = free`
- Track 0 and Track 1 are system-reserved, so their FAT words are effectively `0x0000`
- a fully free track appears as:
  - `0xFFFF` on 2HD
  - `0xFFC0` on 2D / 2DD

### Current Implementation Posture

The safest current implementation posture is:

- trust the printed geometry, FAT, Directory, and FAM structure
- treat sector and record as the same 512-byte unit
- trust the documented disk type, file type, and file attribute semantics listed above
- continue treating directory attribute and directory version as raw bytes until better evidence appears

## Active Unknowns

The following items remain active analysis targets and should stay explicit unknowns until new
evidence arrives.

- Directory attribute byte at `0x0061`
- Directory version byte at `0x0062`
- the exact runtime meaning of the `kanji` attribute in modern tooling terms
- whether all documented subtype ranges are fully implemented on every X-DOS variant

## Core Storage Units

### Record

**Confirmed**

- One record is **512 bytes**

This is stated in the printed source and matches the current repository disk images for the main
filesystem area.

### Cluster

**Strongly supported**

- One cluster is **1 KB**
- Therefore one cluster is **2 records**

This unit was identified from an additional contemporary source described by the user and is
consistent with a 512-byte record size. However, the exact way cluster terminology maps onto all
X-DOS internal structures still needs implementation-level confirmation, because the printed FAM
description itself is record-oriented.

## Disk Geometry

### 2D / 2DD

**Confirmed**

- 1 sector = **512 bytes**
- 1 track = **10 sectors**

### 2HD

**Confirmed**

- 1 sector = **512 bytes**
- 1 track = **16 sectors**

## Reserved and Data Areas

**Confirmed**

X-DOS reserves track 0 and track 1 as system area.

### Reserved Area Layout

#### Track 0

- Sector 1: function-key related system data
- Sector 2: auto-run command area

#### Track 1

- Sector 1: FAT
- Sector 2 and later: Directory
  - 2D / 2DD: sectors 2-10
  - 2HD: sectors 2-16

### Data Area

**Confirmed**

- Track 2 and later are the data area

This is one of the most important corrections to the earlier reverse-engineering notes.

## FAT Format

## Location

**Confirmed**

- FAT is stored at **track 1, sector 1**

## FAT Header Bytes

The printed source describes the first bytes of a freshly formatted disk FAT sector as follows.

### Byte 0: Media Type

**Confirmed**

- `0x00` = 2D
- `0x01` = 2DD
- `0x02` = 2HD

### Byte 1: X-DOS Recognition Flag

**Confirmed**

- Normally `0x01`

The printed source explicitly describes this as an X-DOS recognition flag.

It also notes that `Magical-DOS` uses `0x00` here.

### Per-Track Format Bytes

**Confirmed but not authoritative for runtime**

The bytes immediately following the first two bytes are described as per-track information:

- `bit7-6` = length
- `bit5-0` = sector count

The printed source states that:

- 2D / 2DD should be `0x4A`
- 2HD should be `0x50`

However, due to a formatting bug, both can appear as `0x4A`.

The same source explicitly says the X-DOS system does **not** actually consult this buggy area.
Therefore these per-track bytes should be treated as descriptive metadata, not as the authoritative
allocation map used by the filesystem at runtime.

### Actual FAT Bitmap Area

**Confirmed**

The printed source states that the **real FAT bitmap begins at `0x50A8`** in the sector dump.

This actual FAT body is a **track bitmap**, not a cluster chain table.

- FAT uses **2 bytes per track**
- each 16-bit value represents one track
- each bit corresponds to one sector within that track
- **0 = used**
- **1 = free**

The printed example:

- `$0FFF`
- `%0000 1111 1111 1111`

is explicitly explained as:

- sectors 1-4 = used
- sectors 5-16 = free

### Free Track Patterns

**Confirmed**

- Track 0 and track 1 are system-reserved, so their FAT values are `0x0000`
- In 2HD, a fully free track is `0xFFFF`
- In 2D / 2DD, a fully free track is `0xFFC0`

This is because 2D / 2DD only use 10 sectors per track, while the bitmap still occupies 16 bits.

## Important Consequence

**Confirmed**

The X-DOS FAT is **not** a DOS-style cluster chain table.

It is a per-track sector-usage bitmap.

This corrects the earlier hypothesis that FAT bytes directly encoded cluster occupancy or chain
state.

## Directory Format

## Location

**Confirmed**

- Directory begins at **track 1, sector 2**

## Sector-Level Structure

For a directory sector dump shown in the printed source:

- the first **80 bytes** are the disk title area
- bytes at the next control area contain:
  - disk type
  - directory attribute
  - directory version
- actual file entries begin at offset corresponding to the printed example `0x5080`

The printed source describes the control triplet beginning at the next area as:

- `0x5060[0]` = disk type
- `0x5060[1]` = directory attribute
- `0x5060[2]` = directory version

The document does not currently give a complete semantics table for directory attribute or
directory version.

### Files Per Sector

**Confirmed**

- One directory entry is **32 bytes**
- With 512-byte sectors, one sector holds **16 entries**

## Directory Entry Layout

The following layout is directly supported by the printed field descriptions.

| Offset | Size | Meaning | Status |
| :--- | :--- | :--- | :--- |
| `0x00-0x01` | 2 | File type | Confirmed |
| `0x02-0x11` | 16 | File name, space-padded | Confirmed |
| `0x12-0x13` | 2 | Start address | Confirmed |
| `0x14-0x15` | 2 | File size | Confirmed |
| `0x16-0x17` | 2 | Execution address | Confirmed with caveat |
| `0x18-0x1B` | 4 | Date and time | Confirmed |
| `0x1C` | 1 | File attribute | Confirmed |
| `0x1D-0x1F` | 3 | FAM location tuple | Confirmed |

### File Type

**Confirmed**

- File type is **2 bytes**
- The printed source explicitly warns that the bytes should be read as shown and not blindly
  treated as a swapped little-endian address-like field

### File Name

**Confirmed**

- 16 bytes
- space-padded
- no separate 8.3 extension structure

### Start Address

**Confirmed**

- Stored in `0x12-0x13`
- Printed example `00 00` is interpreted as `$0000`
- This strongly supports little-endian storage for this field

### File Size

**Confirmed**

- Stored in `0x14-0x15`
- Printed example `00 2E` is interpreted as `$2E00`
- This confirms little-endian storage for the normal 16-bit file-size field

### Execution Address

**Confirmed**

- Stored in `0x16-0x17`
- For ordinary binaries, this is the execution address
- The field is stored little-endian, by the same printed convention used for neighboring address
  and size fields

### Large ASCII File Size Extension

**Confirmed**

For ASCII-file cases where the file size exceeds 64 KB:

- `0x14-0x15` and `0x16-0x17` are used together to express file size

The printed source does not spell out the arithmetic in implementation terms, but the natural
interpretation is:

- low 16 bits at `0x14-0x15`
- high 16 bits at `0x16-0x17`

This should still be implemented cautiously until backed by a concrete large-file sample.

### Date and Time

**Confirmed**

- Stored in `0x18-0x1B`

### File Attribute

**Partially confirmed**

- Stored in `0x1C`
- Bit 7 = **secret**

This is explicitly stated in the printed example explanation.

The meanings of the remaining bits are still unresolved.

### FAM Location Pointer

**Confirmed**

The last three bytes of the directory entry point to the location of the file's FAM entry:

- `0x1D` = track
- `0x1E` = sector
- `0x1F` = record

Printed example:

- `02 01 01`
- interpreted as: the file's FAM begins at **track 2, sector 1, record 1**

## Directory End Conditions

**Confirmed**

- Directory scanning continues while file entries are present
- When a file is killed, the leading field (file type) becomes `00`
- The end of the directory is marked with `FF`

For implementation, the safe rule remains:

- treat file type `0x0000` as unused/deleted entry
- do not invent stronger termination semantics than current evidence supports

## FAM Format

## Purpose

**Confirmed**

FAM records how a file uses on-disk records.

The printed source explicitly describes FAM as indicating the linkage of records occupied by a file.

## Encoding Unit

**Confirmed**

FAM is stored in **3-byte groups**:

1. track
2. sector
3. record count

Printed example:

```text
02 02 0F 03 01 08 00 ...
```

This means:

- track 2, sector 2, 15 records
- track 3, sector 1, 8 records
- then end (`00`)

Therefore the example file occupies a total of **23 records**.

## End Marker

**Confirmed**

- FAM terminates with `00`

## Save Behavior and Space Cost

**Confirmed**

The printed source explains that:

- FAM size itself grows with file size
- FAM consumes records too
- even a very small saved file costs at least **1 KB**

This minimum cost follows naturally from:

- one 512-byte record for file data
- one 512-byte record for FAM data

## Record Number Translation

**Confirmed**

The printed source states that record numbers can be obtained from track/sector by using the
system call:

- `@GETREC`
- address shown in print as `$EDDB`

The calling rule described in the source is:

- place track in `D`
- place sector in `E`
- call the routine
- `DE` returns the record number

This is important because it confirms that X-DOS has an explicit logical record numbering layer in
addition to physical track/sector addressing.

## X-DOS.DEF Independent Confirmation

**Confirmed**

The extracted `X-DOS.DEF` file

provides an independent assembler-side confirmation of several filesystem structures already
reconstructed from disk and print evidence.

### Directory Work Table Layout

The following definitions appear as a contiguous work-table block and match the 32-byte directory
entry layout exactly:

| Symbol | Address | Entry Offset | Meaning |
| :--- | :--- | :--- | :--- |
| `@FTYPE` | `0xECD0` | `+0x00` | file type |
| `@NAME` | `0xECD2` | `+0x02` | file name |
| `@DTADR` | `0xECE2` | `+0x12` | start address |
| `@SIZE` | `0xECE4` | `+0x14` | file size |
| `@EXADR` | `0xECE6` | `+0x16` | execution address |
| `@DAY` | `0xECE8` | `+0x18` | timestamp / date-time field |
| `@ATTR` | `0xECEC` | `+0x1C` | file attribute |
| `@TRACK` | `0xECED` | `+0x1D` | FAM track |
| `@SECT` | `0xECEE` | `+0x1E` | FAM sector |
| `@REC` | `0xECEF` | `+0x1F` | FAM record |

This does not discover a new layout, but it is a strong independent check that the reconstructed
directory entry structure is correct.

### ASC-Specific I/O Path

`X-DOS.DEF` also defines a distinct ASCII/text-file I/O path:

- `@AROPEN`
- `@ARCLSE`
- `@ARDD`
- `@AWOPEN`
- `@AWCLSE`
- `@AWRD`

Together with the explicit file type declaration `ASC 0400`, this is sufficient to treat:

- `0x0400 = ASC / text file`

as a confirmed part of the file type table.

## What the Printed Source Resolves

The newly available printed source resolves the following major uncertainties from the earlier
reverse-engineering notes:

1. FAT is not a DOS-like chain table; it is a per-track sector bitmap
2. Directory file type is 2 bytes, not 1 byte
3. Directory tail bytes `0x1D-0x1F` are a direct FAM location tuple
4. FAM is a list of 3-byte `(track, sector, record_count)` groups
5. Track 0 and 1 are system-reserved; track 2+ is data area
6. The main filesystem record size is 512 bytes
7. 2D/2DD and 2HD track sector counts are confirmed

## Remaining Unknowns

The following points are still unresolved and should remain explicit unknowns in implementation and
documentation.

### 1. Directory Attribute and Directory Version

**Unresolved**

The byte positions are now known, but neither the printed material nor the two currently analyzed
disk samples explain the exact semantics of:

- directory attribute (`0x0061`)
- directory version (`0x0062`)

### 2. Exact Runtime Meaning of the `kanji` Attribute

**Partially resolved**

The printed material labels attribute bit 4 as `kanji`, but the exact runtime behavior remains
unclear from the currently integrated evidence. It may relate to dictionary, FEP, or other
Japanese text handling, but this should not yet be encoded as a strict implementation rule.

### 3. Variant Coverage of Documented Subtypes

**Partially resolved**

The printed material documents several subtype ranges and concrete values, but the current repo
evidence does not yet prove that every listed subtype exists or behaves identically across:

- turbo
- normal X1
- MZ-2500

### 4. Cluster Semantics vs FAM Record Groups

**Partially resolved**

We now have strong evidence that:

- record = 512 bytes
- cluster = 1 KB

But the printed FAM explanation is record-oriented, not cluster-oriented. The exact relationship
between:

- cluster-based reasoning in reverse-engineering notes
- FAM's explicit `(track, sector, record_count)` tuples

still needs careful reconciliation in implementation.

### 5. Record Number Formula for All Media Variants

**Unresolved**

The existence of logical record numbering is confirmed, but the complete formula for every media
variant should still be confirmed from code or additional technical material before encoding it as
a universal rule.

### 6. `NUL 00??` vs Killed Marker

**Partially resolved**

Additional documentation mentions a `NUL 00??` type class, but a narrow probe over the currently
available disk samples found no observed directory entries whose file type matched `00??` with a
nonzero low byte.

Observed result on current samples:

- `XDOS_SYS.D88`: no `00??` entries observed
- `XDOSUTIL.D88`: no `00??` entries observed

Therefore:

- `0x0000` remains the only directly observed killed-file marker in current disk evidence
- `NUL 00??` should not yet be treated as an on-disk active file class without additional proof

## Implementation Guidance

For current Legacy89DiskKit work, the safe filesystem model is:

- detect the media geometry correctly
- treat FAT as a sector-usage bitmap
- treat Directory entries as 32-byte records with 2-byte file type and 3-byte FAM pointer
- treat FAM as a variable-length list of 3-byte `(track, sector, record_count)` groups terminated by `00`
- trust the documented file type and file attribute meanings already listed in this document
- keep unresolved directory-control semantics and undocumented subtype behavior as raw values

This is now a much stronger basis for correct read support than the earlier cluster-chain
hypotheses.
