using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.FileSystem.Application;
public class CompositeBootProfileService : IBootProfileService
{
    private readonly X1BootEntrySummaryService _x1Service = new();
    private readonly MsxBootMetadataService _msxService = new();
    private readonly Pc88BootEntrySummaryService _pc88Service = new();
    public BootInfoSummary GetBootProfile(IFileSystem fileSystem)
    {
        var fsInfo = fileSystem.GetFileSystemInfo();
        if (fsInfo.FileSystemName == "MSX-DOS")
        {
            return _msxService.GetBootSummary(fileSystem);
        }

        if (fsInfo.FileSystemName == "Hu-BASIC" || fsInfo.FileSystemName == "X-DOS")
        {
            var x1Summary = _x1Service.GetSummary(fileSystem);
            return MapX1ToBootInfo(x1Summary);
        }

        if (fsInfo.FileSystemName == "N88-BASIC" || fsInfo.FileSystemName == "CP/M")
        {
            var pc88Summary = _pc88Service.GetSummary(fileSystem);
            return MapPc88ToBootInfo(pc88Summary);
        }

        return new BootInfoSummary(BootInfoMode.None);
    }

    private static BootInfoSummary MapX1ToBootInfo(X1BootEntrySummary x1)
    {
        return x1.Kind switch
        {
            X1BootEntryKind.HuBasicFileBacked => new BootInfoSummary(BootInfoMode.FileBacked, x1.DisplayName, x1.LoadAddress, x1.ExecutionAddress, x1.DisplayName, x1.MachineFamily),
            X1BootEntryKind.XDosSectorResident => new BootInfoSummary(BootInfoMode.SectorResident, DisplayName: "X-DOS", MachineFamily: x1.MachineFamily),
            _ => new BootInfoSummary(BootInfoMode.None, MachineFamily: x1.MachineFamily)};
    }

    private static BootInfoSummary MapPc88ToBootInfo(Pc88BootEntrySummary pc88)
    {
        return pc88.Kind switch
        {
            Pc88BootEntryKind.N88BasicSectorResident => new BootInfoSummary(BootInfoMode.SectorResident, DisplayName: pc88.DisplayName, MachineFamily: CharacterEncoding.Domain.Model.MachineType.PC8801),
            Pc88BootEntryKind.CpmSectorResident => new BootInfoSummary(BootInfoMode.SectorResident, DisplayName: pc88.DisplayName, MachineFamily: CharacterEncoding.Domain.Model.MachineType.PC8801),
            _ => new BootInfoSummary(BootInfoMode.None, MachineFamily: pc88.MachineFamily)};
    }
}
