# Gemini Investigation Report

## Task ID
20260320-235810-m1-xdos-kernel-reanalysis-inventory

## Instruction Filename
20260321-000112-m1-xdos-kernel-reanalysis-inventory-retry.md

## Branch Name
`none`

## Summary
Milestone 1 of the X-DOS kernel reanalysis has been retried with strict evidence grading. This investigation focused on a disciplined inventory of primary artifacts (disk images, salvaged binary snippets) versus secondary analysis (documentation, C# assumptions). The contradiction matrix has been refined to focus on material risks for M2 (read-path) and M3 (write-path) work, specifically addressing the FAM `0x00` semantics, cluster mapping confirmed by Z80 opcode analysis, and the recently discovered 2HD sector density shift.

## Source Inventory

| Source | Category | Why? | Proves | Does Not Prove |
| :--- | :--- | :--- | :--- | :--- |
| `XDOS_SYS.D88` | **Primary Evidence** | Direct binary artifact of the OS. | Sector-level data, FAT/FAM state, Directory structure. | Original source intent, kernel-internal variable names. |
| `XDOSUTIL.D88` | **Primary Evidence** | Direct binary artifact containing utilities. | Same as above + includes salvaged SLANG sources. | - |
| Salvaged Z80 Kernel (C1,H1,R8) | **Primary Evidence** | Binary code extracted from `XDOSUTIL.D88`. | `EE 10` instruction proves interleaved side-select logic. | High-level API philosophy. |
| `X-DOS_Filesystem_Analysis.md` | **Secondary Analysis** | Documentation of previous observations. | Aggregated patterns, track/sector mapping hypotheses. | Exactness of all offsets (must be verified by code). |
| `X-DOS_License_And_Sources.md` | **Secondary Analysis** | Meta-data about origin and authors. | Provenance, dates, original site claims. | Technical implementation details. |
| `XDosFileSystem.cs` | **Implementation Assumption** | Current codebase behavior. | Current toolkit logic, registration, and provider flow. | Original X-DOS kernel behavior (risk of false parity). |
| `XDosFileSystemTest.cs` | **Implementation Assumption** | Test-driven verification. | Correctness of C# code against provided D88 samples. | Universal validity for all X-DOS versions. |

## Contradiction Matrix

| Subject | Side A Claim | Side B Claim | Grade A / B | Risk / Resolution |
| :--- | :--- | :--- | :--- | :--- |
| **FAM 0x00 Meaning** | End-of-chain marker (forward link). | Free cluster marker (like FAT). | Primary / Secondary | **High**. If `0x00` means free, chain traversal fails. Resolved by observation that FAM[2]=9 and FAM[9]=0 for a 2-cluster file. |
| **Cluster Mapping** | Interleaved `C=N/2, H=N%2`. | Flat `H=0` for all data. | Primary / Secondary | **Low**. Resolved by `EE 10` (side select toggle) found in kernel binary at C1,H1,R8. |
| **2HD Geometry** | 10 sectors/track (universal). | 16 sectors/track (2HD only). | Implementation / Secondary | **High**. Current C# code was hardcoded to 10. Documents claim 16 for 2HD. Need 2HD primary image to confirm. |
| **FirstSectorR Role** | Sub-cluster alignment offset. | Sector-within-cluster index. | Implementation / Unknown | **Medium**. C# uses it as a starting R within the first cluster. Needs confirmation if it applies to all clusters. |
| **Directory Offsets** | Byte size at +22. | End address at +22. | Implementation / Secondary | **Medium**. Original spec said End Addr. Current C# uses ByteSize at +20. `hexdump` confirms 0xC000 (Load) followed by 0x2800 (Size/End). |

## Evidence Ledger

### Primary Evidence (Verified from Artifacts)
- **Geometry**: Interleaved dual-sided mapping (`C=N/2, H=N%2`) is confirmed by salvaged kernel Z80 code (`EE 10` instruction) and observed directory/FAM alignment.
- **Volume Record**: Track 0, R=1 (256B) contains "X-DOS" identifier and BCD date.
- **FAT/FAM Location**: FAT is at C0,H1,R1. FAM is at C1,H0,R1.
- **Directory Location**: C0,H1,R2-10 (9 sectors).

### Secondary Analysis (Grounded in Docs/Observations)
- **2HD Sector Count**: 16 sectors/track claimed for 2HD media (Roadmap V2 notes).
- **Cluster 2 Content**: FAM sector (C1,H0,R1) and bdir hidden area (C1,H0,R2-10) are consistently assigned to Cluster 2.
- **File Types**: 0x01-0x07 range identified for system/basic/binary types.

### Implementation Assumptions (Carry with Caution)
- **`FirstSectorR` as R-index**: C# code treats `entry[30]` as a starting sector index (1-10) for the first cluster.
- **Date/Time Endianness**: C# code reads Date/Time at +24/+26 as Big-Endian. `hexdump` confirms `88 24 04 17` at Track 0 offset 24, but directory-level date/time needs more samples.

### Unknown
- **Cluster mapping for > 80 tracks**: How X-DOS handles 2DD or 2HD beyond the 2D track limit (interleaving vs flat).
- **Sub-directory semantics**: Documents mention hierarchical support, but no evidence found in `XDOS_SYS.D88` or `XDOSUTIL.D88`.

## Provisional Conclusions (Milestone 1)
1. **Safe for M2 (Read-Path)**:
   - Interleaved geometry (`C=N/2, H=N%2`) is verified and safe.
   - FAT/FAM/Directory locations are stable and verified.
   - FAM `0x00` as end-of-chain is verified.
2. **Open for Investigation**:
   - `FirstSectorR` behavior for multi-cluster files needs a test case with non-1 start values.
   - 2HD media support must be treated as "experimental" until a primary 1.2MB image is analyzed.

## Requested Review
- Verify the distinction between "ByteSize" and "EndAddress" at offset +20/+22 in `XDosDirParser.cs`.
- Confirm if the 2HD 16-sector claim in `Roadmap_V2.md` matches original manual text or is a derived observation.
