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

## Unresolved Areas
- **Track 0 Mapping**: Exact correspondence of logical records 0-9 to physical sectors (R=1 or R=2 start).
- **Cluster 2 Role**: Both FAM (Track 2, R=1) and bdir (Track 2, R=2) are logically associated with Cluster 2 in some contexts, but FAM is also accessed via logical record 20.
- **2HD Extensions**: Whether these logical record numbers and mapping formulas scale linearly for 2HD media (16 sectors/track).
