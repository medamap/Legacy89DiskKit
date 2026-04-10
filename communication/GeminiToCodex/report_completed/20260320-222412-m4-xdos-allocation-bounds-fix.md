# Gemini Work Report

## Task ID
20260320-222412-m4-xdos-allocation-bounds-fix

## Instruction Filename
20260320-222412-m4-xdos-allocation-bounds-fix.md

## branch_name
codex/m4-xdos-allocation-bounds-fix

## Summary
Fixed a critical out-of-bounds allocation bug in the X-DOS filesystem implementation. Previously, `XDosFatWriter.AllocateClusters` only bounded its search loop by the size of the FAT sector (256 entries), which allowed allocating clusters beyond the physical capacity of the disk (e.g., Cluster 80 on a 2D disk which only has 80 tracks numbered 0-79). This resulted in a "Sector not found" crash during write operations. I updated `XDosMediaGeometry` to include a `TotalTracks` property derived from the disk type and modified `XDosFatWriter` to respect this limit. Now, when a disk is full, the system throws a clean `IOException("Disk full.")` instead of crashing.

## Changed Files
- `CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/XDosMediaGeometry.cs`: Added `TotalTracks` property and updated `FromDiskType` mapping.
- `CSharp/Legacy89DiskKit.Infrastructure/FileSystem/XDos/Reader/XDosFatWriter.cs`: Updated `AllocateClusters` to bound the loop by `TotalTracks`.
- `CSharp/Legacy89DiskKit.Tests/FileSystem/XDos/XDosFileSystemTest.cs`: Added `WriteFile_ExceedPhysicalCapacity_ThrowsDiskFull` regression test.

## Commands
- `dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter "XDos|WriteFile_NewDisk2DD_CrossCopy|WriteFile_DoesNotAllocateCluster0Or2"`
- `dotnet publish CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:PublishAot=false -o /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/test/publish-m4-standalone-cli`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/test/publish-m4-standalone-cli/Legacy89DiskKit.Cli disk create images/test/XDOS_2D_M4.d88 -d 2d -f xdos -n "XDOS_M4"`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/test/publish-m4-standalone-cli/Legacy89DiskKit.Cli disk boot-copy images/disk_org/x1/XDOS_SYS.D88 images/test/XDOS_2D_M4.d88 --force`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/test/publish-m4-standalone-cli/Legacy89DiskKit.Cli file cross-copy images/disk_org/x1/XDOS_SYS.D88 images/test/XDOS_2D_M4.d88 all`

## Evidence
- **Regression Test**: `WriteFile_ExceedPhysicalCapacity_ThrowsDiskFull` passes (Asserts `IOException("Disk full.")`).
- **Standalone CLI Output**: 
  ```
  Error: Failed to transfer file 'PALINIT': Disk full.
  ```
  This confirms the crash mode has changed from `Sector not found` to a graceful `Disk full.` error.

## Risks
- Standard 2D disks remain insufficient for a full `XDOS_SYS.D88` fileset without implementing cluster-sharing support.
- If a custom disk image has non-standard track counts not covered by the `DiskType` switch, the allocation limit might still be slightly off, but it will always be safer than the previous unbounded 256.

## Requested Review
- Verify the mapping of `TotalTracks` in `XDosMediaGeometry.FromDiskType` (2D=80, 2DD=160, 2HD=160).
- Confirm that `IOException("Disk full.")` is the preferred error for this failure mode in the CLI.
