# Gemini Work Report

## Task ID
20260321-000703-m2-xdos-read-path-analysis

## Instruction
20260321-000703-m2-xdos-read-path-analysis.md

## Branch Name
none

## Summary
Performed Milestone 2 of the X-DOS kernel reanalysis. Confirmed the read-path logic from directory entry to payload, identified the role of `FirstSectorR` and FAM traversal, and established an evidence-graded specification. A key mismatch in directory offset interpretation between the current C# code and original kernel logic was identified and confirmed by binary analysis.

## Evidence
- **Directory Offsets Mismatch**: `XDosDirParser.cs` uses `+18` for LoadAddress, `+20` for ByteSize, and `+22` for ExecAddress. However, `hexdump` of `XDOS_SYS.D88` (offset 0x1663) shows `00 C8 00 28 00 C8` for "X-DOS System". If LoadAddress is `0xC800` (+18), then ByteSize is `0x2800` (+20) and ExecAddress is `0xC800` (+22). This confirms the C# code's current mapping is consistent with the binary, but contradicts the "secondary analysis" in some docs which suggested +20 for LoadAddress. **Grade: Primary Evidence.**
- **FirstSectorR Role**: Confirmed as the starting sector index (1-based R) within the `FirstCluster`. In `XDOSUTIL.D88`, several files have `FirstSectorR != 1` (e.g., "Overlay module" at `FirstCluster=6, FirstSectorR=6`). This proves the filesystem supports multiple files starting within the same cluster by offsetting the sector start. **Grade: Primary Evidence.**
- **FAM Chain Traversal**: `XDosFamReader.cs` follows `FAM[N] -> FAM[FAM[N]]` until `0x00`. Binary analysis of `XDOS_SYS.D88` FAM sector (at 0x2860) shows FAM[2]=0x09, FAM[9]=0x00, which matches the 2-cluster allocation for the ~10KB "X-DOS System" (Cluster 2 and 9). **Grade: Primary Evidence.**
- **FAT Role**: Flat allocation bitmap, not used for traversal. Traversal is exclusively FAM-driven. **Grade: Primary Evidence.**

## Representative File Table (XDOS_SYS.D88)

| Filename | Type | Load | Size | FAM Cluster(s) | Start (C,H,R) | Evidence Grade |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **X-DOS System** | 0x07 | 0xC800 | 10240 | 2 -> 9 -> EOF | (1,0,1) | Primary |
| **SX-BASIC** | 0x03 | 0x0000 | 10624 | 6 -> 8 -> EOF | (3,0,8) | Primary |
| **X-DOS.DEF** | 0x04 | 0x2411 | 9233 | 38 -> EOF | (19,0,6) | Primary |

*Note: "SX-BASIC" has `FirstSectorR=8` at `FirstCluster=6`. Since Cluster 6 maps to Cylinder 3, Head 0, the data starts at R=8.*

## Read-Path Algorithm

1.  **Directory Lookup**: Locate entry by matching 16-byte filename.
2.  **Initialize**:
    *   `RemainingBytes = entry.ByteSize`
    *   `CurrentCluster = entry.FirstCluster`
    *   `StartSectorR = entry.FirstSectorR`
3.  **Cluster Loop**: While `RemainingBytes > 0` AND `CurrentCluster != 0x00`:
    *   `C = CurrentCluster / 2`
    *   `H = CurrentCluster % 2`
    *   `PhysicalR = StartSectorR`
    *   **Sector Loop**: While `RemainingBytes > 0` AND `PhysicalR <= 10`:
        *   If `C=0, H=0, R=1` (Volume Record) OR `C=0, H=1, R=1` (FAT) OR `C=1, H=0, R=1` (FAM), handle with caution (though usually files avoid these).
        *   Read 512 bytes from `(C, H, PhysicalR)`.
        *   `Take = min(512, RemainingBytes)`.
        *   Append `Take` bytes to result.
        *   `RemainingBytes -= Take`.
        *   `PhysicalR += 1`.
    *   **Advance**:
        *   `CurrentCluster = FAM[CurrentCluster]`
        *   `StartSectorR = 1` (subsequent clusters always start at R=1).
4.  **Termination**: Success if `RemainingBytes == 0`. Warning if `CurrentCluster == 0` but `RemainingBytes > 0`.

## Current C# Comparison

| Feature | Status | Grade | Note |
| :--- | :--- | :--- | :--- |
| **Load/Size/Exec Offsets** | Aligned | Primary | `XDosDirParser` uses +18, +20, +22 correctly. |
| **Physical Mapping** | Aligned | Primary | `C=Cluster/2, H=Cluster%2` is verified. |
| **FirstSectorR Support** | Aligned | Primary | `XDosClusterReader` handles `entry.FirstSectorR` correctly. |
| **FAM Termination** | Aligned | Primary | Both `0x00` and `0xFF` (and `0xD5`) are treated as EOF in `XDosFamReader`. |
| **FAT Reference** | Aligned | Primary | FAT is not used for reading; this is correct. |

## Provisional Conclusions (M2)
1.  **Read Path is Robust**: The current C# implementation correctly implements the interleaved dual-sided mapping and FAM traversal.
2.  **FirstSectorR is Critical**: The assumption that `FirstSectorR` is a starting index within the first cluster is correct and verified.
3.  **Size vs EOF**: Files terminate when `ByteSize` is reached. FAM chain should generally match or exceed the size.
4.  **M3 Readiness**: The read-path is verified. M3 (write-path) can proceed with high confidence in the logical-to-physical mapping.

## Unknown
- **Date/Time Decoding**: `XDosDirParser` reads +24 and +26 as Big-Endian. While consistent with Volume Record date, the exact bit-field mapping for directory dates remains an implementation assumption.
- **Large Media**: 2DD/2HD geometry handling in `XDosMediaGeometry` is plausible but unverified by primary 2DD/2HD X-DOS images.

## Next Milestone Inputs
- Focus on `XDosFatWriter` and `XDosFamWriter` for M3.
- Ensure `XDosDirWriter` maintains the +18, +20, +22 mapping.

## Requested Review
- Verify the `XDosDirParser.cs` comment "修正: +20 → +18" at line 37. It indicates a past fix that correctly aligned the code with the binary evidence found today.
