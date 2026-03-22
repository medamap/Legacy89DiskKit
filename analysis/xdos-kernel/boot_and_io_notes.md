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

### Boundary and Indexing Summary
- **Entry Base**: Start of any 32-byte block in the directory area.
- **Entry Length**: 32 bytes (0x20).
- **Filename Span**: Bytes 2 through 17 (16 bytes).
- **Index 0x1A (26)**: Byte at `Base + 26`.
- **Index 0x1B (27)**: Byte at `Base + 27`.
- **Index 0x1D (29)**: Byte at `Base + 29`.
- **Index 0x1E (30)**: Byte at `Base + 30`.

## Geometry Translation (Methodology)

- **Transform**: `(C * 2 + H, R)` is used to translate the raw D88 header tuple `(C, H, R)` into a flat observed placement pair.
- **Justification**: Flattens physical actuator and head position into a continuous metric for 2D double-sided media.

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
