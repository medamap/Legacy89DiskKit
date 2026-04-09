using Legacy89DiskKit.Domain.CharacterEncoding.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.FileSystem.Application;
public class X1BootEntrySummaryService
{
    private readonly HuBasicMetadataService _huBasicMetadataService = new();
    public X1BootEntrySummary GetSummary(IFileSystem fileSystem)
    {
        var fsInfo = fileSystem.GetFileSystemInfo();
        if (fsInfo.FileSystemName == "Hu-BASIC")
        {
            var bootRecord = _huBasicMetadataService.GetBootRecordInfo(fileSystem);
            if (bootRecord != null)
            {
                var fullName = string.IsNullOrWhiteSpace(bootRecord.Extension) ? bootRecord.Name : $"{bootRecord.Name}.{bootRecord.Extension}";
                return new X1BootEntrySummary(X1BootEntryKind.HuBasicFileBacked, fullName, bootRecord.LoadAddress, bootRecord.ExecutionAddress, MachineFamily: MachineType.X1);
            }

            var bootArea = fileSystem.ReadBootArea();
            if (bootArea.Any(value => value != 0x00))
            {
                return new X1BootEntrySummary(X1BootEntryKind.None, MachineFamily: MachineType.X1);
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
        if (bootArea.Length < 25)
            return false;
        // Volume Record at Sector 1 (first 256 bytes)
        // Offset 0: 0x01 (Record type identifier)
        // Offset 24: 0x88 (Format type byte: Sharp X1 2D)
        return bootArea[0] == 0x01 && bootArea[24] == 0x88;
    }
}
