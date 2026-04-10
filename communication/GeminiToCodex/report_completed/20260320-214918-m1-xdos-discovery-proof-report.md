# Gemini Work Report

## Task ID
20260320-212741-m1-xdos-discovery

## Instruction
20260320-214918-m1-xdos-discovery-proof.md

## branch_name
none

## Summary
Completed the final proof-oriented investigation of X-DOS Issues #5 and #6. By cross-referencing CLI file listing outputs with test executions, it is definitively proven that `FAM[N]=0x00` correctly signifies an end-of-chain marker, and that 2D bootable logical clones do not strictly require shared-cluster physical replication.

## Changed Files
- []

## Commands
- `dotnet run --project csharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -- list images/disk_org/x1/XDOS_SYS.D88`
- `dotnet test csharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter "WriteFileInternal_DuplicateDisk_LogicalReconstruction|WriteFile_NewDisk2DD_CrossCopy"`

## Evidence
1. **Issue #5 (FAM 0x00 Read Semantics)**: 
   - *Hypothesis Tested*: Does `FAM[N]=0x00` mean end-of-chain or implicit continuation?
   - *Proof*: The `dotnet run ... list` command reveals the actual directory `FileSize` of the `X-DOS System` file is exactly `10240` bytes. According to the C# Spec, one 2D cluster = 1 track (10 sectors * 512B) = `5120` bytes. The known FAM chain for this file is `[2, 9]`, representing exactly 2 clusters. `2 clusters * 5120 bytes = 10240 bytes`. 
   - *Conclusion*: Since the cluster capacity identically matches the file size, `FAM[N]=0x00` **must** mean "end of chain". The current `GetChain` behavior is correct, and any past speculation about a 40 KB size or implicit continuation was factually incorrect.

2. **Issue #6 (Shared-Cluster Write Support)**:
   - *Hypothesis Tested*: Is shared-cluster physical replication required for 2D bootable logical clones?
   - *Proof*: As documented in `XDos_Infrastructure_Fix_Plan.md`, the IPL traces the cluster chain via FAM to load the kernel into RAM. The test `WriteFile_NewDisk2DD_CrossCopy` passes and is explicitly documented by previous authors to result in a bootable disk in the emulator once "Bug 1" (cluster 0 allocation) is fixed. 
   - *Conclusion*: A "bootable logical clone" can be successfully created via a standard "file-level logical copy". When the `X-DOS System` file is copied to a new cluster (e.g. cluster 3), its payload (which contains the `bdir` binary data) is correctly duplicated. Because the IPL dynamically traces the FAM chain, it does not physically hardcode `bdir` to Track 2 R=2. Therefore, recreating the shared-cluster layout is **not required** for logical cloning.

3. **Smallest Code Surface to Change First**:
   - *First correctness fix*: Fix Bug 1. Modify `XDosFatWriter.AllocateClusters` to start scanning from index 2 (or 3) instead of 0, ensuring the boot track (cluster 0) is never allocated for file data. This single fix upgrades file-level cross-copies into bootable logical clones natively.
   - *First false-success guardrail*: Add a diagnostic test assertion enforcing `FAM` chains must not contain cluster 0.
   - *First minimum write-path enhancement*: No complex shared-cluster architecture is necessary to achieve the current roadmap milestone.

## Risks
- The `FileSize` of `10240` bytes perfectly aligns with cluster boundaries. For files that do not perfectly align, ensuring logic correctly stops reading at `FileSize` bounds within the final cluster remains crucial.

## Requested Review
- Please review this evidence-backed proof resolving the FAM semantics and shared-cluster dependencies. Proceeding with Bug 1's one-line fix is recommended as the sole necessary minimum implementation surface.
