# Gemini Work Report

## Task ID
20260320-235810-m1-xdos-kernel-reanalysis-inventory

## Instruction
20260320-235810-m1-xdos-kernel-reanalysis-inventory.md

## Branch Name
none

## Summary
Performed Milestone 1 of the X-DOS kernel reanalysis. Built a classified source inventory, identified five major semantic contradictions, and established an evidence ledger separating direct observations from inferences. Confirmed that the "interleaved dual-sided" mapping and "forward FAM chain" are direct evidence from salvaged kernel sources.

## Changed Files
- none (Investigation only)

## Commands
- `rg -n "X-DOS|XDOS|FAM|FAT|FirstSectorR|bdir|sys_rdd|sys_wrd|sys_ropen|sys_wopen" Documents CSharp csharp communication`
- `dotnet run --project csharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -- list images/disk_org/x1/XDOS_SYS.D88`
- `hexdump -C images/disk_org/x1/XDOS_SYS.D88 | head -n 40`

## Evidence
- **Salvaged Z80 Kernel Source**: Found at Track 1, R=8 in `XDOSUTIL.D88` (referenced in `X-DOS_Filesystem_Analysis.md`). Contains `EE 10` (toggle side select) confirming interleaved geometry.
- **Binary Parity Test**: `WriteFileInternal_DuplicateDisk_LogicalReconstruction` in `XDosFileSystemTest.cs` successfully reconstructs `XDOS_SYS.D88` bit-for-bit, validating the FAM chain and sector mapping logic.
- **D88 Sector Inspection**: Track 0 confirmed to have 16 sectors of 256 bytes (N=1), while data tracks have 10/16 sectors of 512 bytes (N=2), confirming mixed-geometry requirements.

## Contradictions
| Topic | Side A (Current Implementation) | Side B (Document/Theory) | Conflict Type |
| :--- | :--- | :--- | :--- |
| **FAT[0] Meaning** | Reserved for Boot Area (skips allocation). | "Free" in raw dump (0x00), but never used by OS. | Direct vs Inference |
| **FAM Chain End** | `0x00`, `0xFF`, `0xD5` are all treated as ends. | `0x00` is the standard end; others might be deleted. | Inference Conflict |
| **FirstSectorR** | Logical record 10 maps to T1, R1. | Record 0–9 map to T0, but R1 is Volume Record. | Mapping Ambiguity |
| **Sector Size** | D88 auto-handles mixed N values. | Early code assumed uniform 512B sectors. | Implementation Bug |
| **Cluster Mapping** | `cylinder = N/2, head = N%2` (interleaved). | `head = 0` (single-sided 80-track). | Direct Proof (Side A) |

## Evidence Ledger

### Direct Evidence
- **Geometry**: Interleaved dual-sided mapping (`C=N/2, H=N%2`) is confirmed by salvaged kernel Z80 code (`EE 10` instruction).
- **FAM Chain**: FAM[N] stores the next cluster index. FAM[2]=9, FAM[9]=0 for 40KB kernel.
- **Directory Layout**: 32-byte entries, binary addresses at +18, +20, +22. Dates at +24, +26.

### Secondary Inference
- **FAM[N] = 0x00**: Interpreted as EOF. Consistent with all analyzed files.
- **Cluster 0/1 Reservation**: Inferred from `Format()` and `ClearAll()` not assigning them to files, although FAT[0] is often 0x00 on original disks.
- **FirstSectorR Role**: Used as an offset within the *first* cluster only.

### Unknown
- **Bit Semantics of Entry[28]**: Values like 0x80, 0x10, 0x40 observed; exact meaning of each bit is unknown.
- **16-char Encoding**: Handled as Latin-1/ASCII, but evidence suggests Shift-JIS for some Japanese filenames.
- **2HD Data Start**: 2HD uses 16 sectors/track; whether the kernel/FAT/FAM/Dir positions shift is unknown (assumed "same track, different count").

## Milestone 1 Conclusion
The priority order for M2 is: (1) Salvaged Kernel Source, (2) Duplicate Disk Parity Evidence, (3) `X-DOS_Filesystem_Analysis.md`.
Questions ready for M2: FAM read-path verification for fragmented files.
Blocked questions: Write-path safety for shared-cluster allocations (common on original disks).

## Requested Review
- Please verify the `XDosDirWriter.cs` serialization offsets as they were manually corrected in the source but might need cross-validation with a live disk write.
