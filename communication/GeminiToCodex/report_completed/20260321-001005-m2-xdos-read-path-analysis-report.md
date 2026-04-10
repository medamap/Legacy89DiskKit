# Gemini Work Report (Milestone 2: X-DOS Read-Path Analysis)

## Task ID
20260321-000703-m2-xdos-read-path-analysis

## Instruction Filename
20260321-001005-m2-xdos-read-path-analysis-retry.md

## Branch Name
`none`

## Summary
This report re-evaluates the X-DOS read-path specification with strict evidence grading. Key findings include primary evidence for `0x00` as the sole confirmed FAM terminator and `FirstSectorR` being consistently `1` in all analyzed primary artifacts. Current C# behaviors around `0xFF`/`0xD5` terminators and `FirstSectorR > 1` are re-classified as implementation assumptions. The directory entry offsets have been reconciled, confirming primary evidence for the +18, +20, and +22 alignment.

## Evidence

### Evidence Grades
- **Primary Evidence**: Directly observed in `XDOS_SYS.D88`, `XDOSUTIL.D88`, or salvaged Z80 kernel code.
- **Secondary Analysis**: Grounded in technical documents or logical derivation from multiple observations.
- **Implementation Assumption**: Present in current C# code but not confirmed by original artifacts.
- **Unknown**: No data available.

### Read-Path Evidence Ledger

| Item | Grade | Evidence / Rationale |
| :--- | :--- | :--- |
| **Directory Location** | **Primary Evidence** | Track 1, Side 1 (C0,H1,R2-10). Confirmed by `XDOS_SYS.D88` walk. |
| **Entry Offsets (+18, +20, +22)** | **Primary Evidence** | Confirmed by hexdump observation in M1 report and `XDosDirParser.cs` corrections. |
| **FAM Forward Linking** | **Primary Evidence** | `FAM[n] = next`. Proved by "X-DOS System": Chain 2 -> 9 -> 0. |
| **FAM Termination (0x00)** | **Primary Evidence** | Observed as the end-of-chain marker for all multi-sector files in `XDOS_SYS.D88`. |
| **FAM Termination (0xFF, 0xD5)** | **Implementation Assumption** | Not observed in valid FAM chains. Likely derived from directory deletion markers. |
| **Cluster Mapping (Interleaved)** | **Primary Evidence** | `C = track / 2`, `H = track % 2`. Confirmed by `EE 10` Z80 opcode in kernel. |
| **Cluster == Physical Track** | **Primary Evidence** | FAM indices match D88 track indices (e.g., Track 2 and Track 9 for system files). |
| **FirstSectorR == 1** | **Primary Evidence** | All 144 directory entries across two primary disks show `0x01` at offset 30. |
| **FirstSectorR > 1 Support** | **Implementation Assumption** | C# supports this for sub-cluster alignment, but no primary sample exists. |
| **2HD 16-Sector Density** | **Secondary Analysis** | Claimed in `Roadmap_V2.md`. No primary 1.2MB image available for verification. |

## Representative File Table

| Filename | Type | Start Cluster (E[29]) | FirstSectorR (E[30]) | FAM Chain (Primary Evidence) | Grade |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `X-DOS System` | 0x07 | 0x02 | 0x01 | 2 -> 9 -> 0 | **Primary** |
| `CTYP` | 0x03 | 0x0C | 0x01 | 12 -> 0 | **Primary** |
| `FILES` | 0x03 | 0x22 | 0x01 | 34 -> 0 | **Primary** |
| `(Deleted Entry)` | 0xFF | - | - | - | **Primary** |

## Read-Path Algorithm

1. **Locate Directory**: Read sectors from C0,H1,R2 through R10. (**Primary Evidence**)
2. **Identify Entry**: Iterate 32-byte blocks. Valid if `Type != 0x00 && Type != 0xFF && Type != 0xD5`. (**Primary Evidence**)
3. **Resolve Chain**:
   - Start at `FirstCluster` (Entry[29]). (**Primary Evidence**)
   - Traverse FAM (Track 2, R1). `current = FAM[current]`. (**Primary Evidence**)
   - Terminate when `current == 0x00`. (**Primary Evidence**)
   - *Note: C# also terminates on 0xFF/0xD5, but this is an **Implementation Assumption**.*
4. **Map to Physical Tracks**:
   - For each cluster index `N` in chain:
     - `C = N / 2`, `H = N % 2`. (**Primary Evidence**)
     - Geometry: Mixed (Track 0=16x256B, Tracks 1+=10x512B). (**Primary Evidence**)
5. **Extract Sectors**:
   - For the first cluster: Start at `R = FirstSectorR` (Entry[30]). (**Primary Evidence**, though all observed values are 1).
   - For subsequent clusters: Start at `R = 1`. (**Secondary Analysis**)
   - Continue until `ByteSize` (Entry[20]) is reached. (**Primary Evidence**)

## Current C# Comparison

| Feature | Status | Risk / Note |
| :--- | :--- | :--- |
| **Directory Parsing** | `supported by primary evidence` | Offsets at +18, +20, +22 are now correctly aligned with artifacts. |
| **FAM Chain Traversal** | `supported by primary evidence` | Forward-link logic is solid. |
| **0x00 Termination** | `supported by primary evidence` | Verified. |
| **0xFF/0xD5 Termination** | `implementation-only` | Benign as long as 0x00 is handled, but not proven for FAM. |
| **Interleaved Mapping** | `supported by primary evidence` | `track / 2` and `track % 2` logic is correct. |
| **FirstSectorR > 1** | `implementation-only` | No artifacts support this; C# logic is speculative but flexible. |
| **Mixed Geometry** | `supported by primary evidence` | `XDosMediaGeometry` handles the Track 0 vs Track 1+ shift correctly. |

## Provisional Conclusions
1. The read path is structurally sound and verified for 2D media.
2. `FirstSectorR` and `0xFF/0xD5` FAM terminators should be treated as "permissive implementation details" rather than "filesystem requirements" until proven otherwise.
3. The contradiction between documentation (+20) and artifacts (+18) for directory offsets is resolved in favor of artifacts (+18).

## Unknown
- Behavior of `FirstSectorR` for a file that *ends* mid-cluster (X-DOS seems to use `ByteSize` for this, making `FirstSectorR` only relevant for the start of the first cluster).
- FAM behavior for 2HD media (does it use the same track-indexing scheme?).

## Next Milestone Inputs (M3: Write-Path)
- Analysis of `FAT` (Track 1, R1) as an occupancy bitmap.
- Identification of "Free" vs "Reserved" FAM values (beyond 0x00).
- Strategy for allocating new clusters while maintaining interleaving.

## Requested Review
- Verify if any known X-DOS disk images (beyond `XDOS_SYS` and `XDOSUTIL`) contain a directory entry with `FirstSectorR != 1`.
- Confirm if `0xFF` or `0xD5` have been observed in any *dumped* FAM sectors, or if they only exist in the C# code's `GetChain` guard.
