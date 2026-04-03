using Legacy89DiskKit.Domain.CharacterEncoding.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.Application.FileSystem;

public class X1BootEntrySummaryService
{
    private readonly HuBasicMetadataService _huBasicMetadataService = new();

    public X1BootEntrySummary GetSummary(IFileSystem fileSystem)
    {
        var fsInfo = fileSystem.GetFileSystemInfo();

        if (fsInfo.FileSystemName == "Hu-BASIC")
        {
            var summary = _huBasicMetadataService.GetBootSummary(fileSystem);
            if (summary.Mode == BootInfoMode.FileBacked)
            {
                return new X1BootEntrySummary(
                    X1BootEntryKind.HuBasicFileBacked,
                    summary.FileName,
                    summary.LoadAddress,
                    summary.ExecutionAddress,
                    MachineFamily: MachineType.X1);
            }
            return new X1BootEntrySummary(X1BootEntryKind.None, MachineFamily: MachineType.X1);
        }

        if (fsInfo.FileSystemName == "X-DOS")
        {
            var bootArea = fileSystem.ReadBootArea();
            if (IsXDosBootArea(bootArea))
            {
                return new X1BootEntrySummary(X1BootEntryKind.XDosSectorResident, MachineFamily: MachineType.X1);
            }
            return new X1BootEntrySummary(X1BootEntryKind.None, MachineFamily: MachineType.X1);
        }

        return new X1BootEntrySummary(X1BootEntryKind.Unsupported, MachineFamily: MachineType.Unknown);
    }

    private static bool IsXDosBootArea(byte[] bootArea)
    {
        if (bootArea.Length < 25) return false;
        // Volume Record at Sector 1 (first 256 bytes)
        // Offset 0: 0x01 (Record type identifier)
        // Offset 24: 0x88 (Format type byte: Sharp X1 2D)
        return bootArea[0] == 0x01 && bootArea[24] == 0x88;
    }
}
