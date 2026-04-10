# Gemini Work Report

## Task ID
20260320-212741-m1-xdos-discovery

## Instruction
20260320-214128-m1-xdos-discovery-retry.md

## Summary
Re-investigated X-DOS Issues 5 and 6 focusing on FAM semantics and shared-cluster dependencies. Identified critical flaws in existing documentation (`X-DOS_Filesystem_Analysis.md` & `Fix_Plan.md`) regarding cluster mapping sizes and IPL physical boot dependencies, confirming that 2D bootable logical clones are impossible without shared-cluster reconstruction.

## Changed Files
- []

## Commands
- No commands executed in this phase (all analysis drawn from code/doc evidence).

## Evidence
1. **Issue #5 (FAM 0x00 Semantics)**: 
   - `FAM[N]=0x00` MUST mean "implicit continuation to the next contiguous cluster (N+1)", rather than "end of chain". 
   - *Evidence*: `X-DOS_CSharp_Implementation_Spec.md` explicitly defines `clusterIndex / 2` and `clusterIndex % 2` (1 track per cluster). This means 1 cluster = 10 sectors * 512B = 5120 bytes (5 KB). The "X-DOS System" file is ~40 KB. If it occupies `[2, 9]` and stops at `0x00` as the current docs incorrectly claim, it would truncate reading at 10 KB. To read 40 KB, the chain must span 8 contiguous clusters ([2..9]), meaning `0x00` marks unfragmented continuation until `FileSize` is satisfied. 
   - *Action*: The first correctness fix needed is updating `XDosFamReader.GetChain` to use `current + 1` implicitly when `FAM[N]=0` and only halting on capacity limits.

2. **Issue #6 (Shared-Cluster Write Support)**:
   - Reliable 2D logical bootable clones of `XDOS_SYS.D88` are **IMPOSSIBLE** without reproducing the shared-cluster placement.
   - *Evidence*: `XDos_Filesystem_Analysis.md` notes that `bdir` (binary system code) exists at logical record 21 (Track 2, R=2, which is Cluster 2, Sector 2). The `XDos_Infrastructure_Fix_Plan.md` erroneously concluded that "IPL traces the cluster chain for the kernel, so physical address doesn't matter". However, early boot stages map fixed physical records (like `fam_rec=20`, `bdir_rec=21`). If a logical file copy writes the kernel payload to arbitrary clusters (e.g. starting at Cluster 3), physical Cluster 2's `bdir` sector remains zeroes (wiped by `XDosFatWriter.ClearAll`). The IPL will execute zeroes, yielding "System not found!".
   - *Minimum Implementation*: The write path must specifically place the beginning of the `X-DOS System` file payload dynamically back into Cluster 2 starting at `R=1` (or `FirstSectorR=1`), allowing it to overlay and "share" Cluster 2 with the explicit FAM write operations. 

3. **Smallest Code Surface to Change First**:
   - **First correctness fix (Issue #5)**: Fix `XDosFamReader.GetChain()` loop to handle `0x00` as `+1` increment targeting contiguous clusters, ensuring we can read files larger than 10KB.
   - **First guardrail against false success**: Add test assertions that fail format/clone processes if Cluster 2, R=2 (`bdir`) does not contain valid executable Z80 code (non-zero).
   - **First minimum shared-cluster-aware write change (Issue #6)**: Implement a mechanism in `WriteFileInternal` to allow forced manual placement of the "X-DOS System" file starting at Cluster 2 to correctly populate `bdir` simultaneously with its file data payload, followed by fixing the `XDosFatWriter` zero-cluster bug (Bug 1).

4. **File Cross-Copy Re-evaluation on 2D**:
   - *Can it be trusted today?* No; it truncates large files at ~10KB due to the bad FAM reader logic.
   - *Can it become correct with only Issue 5 fixed?* Yes, but ONLY for pure user data files.
   - *Does it require Issue 6 for system duplication?* Yes; without shared-cluster writing (Issue 6), the cloned system disk will physically lack the `bdir` boot sectors and fail to boot.

## Risks
- Amending the `FAM` reader logic to implicitly continue on `0x00` assumes `FileSize` perfectly boundaries the trailing cluster. If `FileSize` is inaccurate (e.g., for BASIC files), read runs might bleed into unallocated clusters without a strict end-of-chain marker.

## Requested Review
- Please review this corrected analysis and confirm whether to proceed with implementing the Issue #5 read fixes before attempting the complex Issue #6 shared-cluster write logic.
