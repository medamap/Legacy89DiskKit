using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.FileSystem.Application;
public class HuBasicMetadataService
{
    private readonly HuBasicBootRecordParser _bootRecordParser = new();
    public HuBasicBootRecordInfo? GetBootRecordInfo(IFileSystem fileSystem)
    {
        if (fileSystem.GetFileSystemInfo().FileSystemName != "Hu-BASIC")
        {
            return null;
        }

        return _bootRecordParser.Parse(fileSystem.ReadBootArea());
    }

    public BootInfoSummary GetBootSummary(IFileSystem fileSystem)
    {
        var bootArea = fileSystem.ReadBootArea();
        var bootRecord = GetBootRecordInfo(fileSystem);
        if (bootRecord != null)
        {
            var fullName = string.IsNullOrWhiteSpace(bootRecord.Extension) ? bootRecord.Name : $"{bootRecord.Name}.{bootRecord.Extension}";
            return new BootInfoSummary(BootInfoMode.FileBacked, fullName, bootRecord.LoadAddress, bootRecord.ExecutionAddress, MachineFamily: MachineType.X1);
        }

        return bootArea.Any(value => value != 0x00) ? new BootInfoSummary(BootInfoMode.SectorResident, MachineFamily: MachineType.X1) : new BootInfoSummary(BootInfoMode.None);
    }

    public void ClearBootRecord(IFileSystem fileSystem)
    {
        if (fileSystem.GetFileSystemInfo().FileSystemName != "Hu-BASIC")
        {
            throw new InvalidOperationException("Boot clear is only supported for Hu-BASIC file-backed boot metadata.");
        }

        var bootArea = fileSystem.ReadBootArea();
        if (bootArea.Length == 0)
        {
            return;
        }

        bootArea[0] = 0x00;
        fileSystem.WriteBootArea(bootArea);
    }
}
