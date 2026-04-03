using Legacy89DiskKit.Domain.CharacterEncoding.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.Application.FileSystem;

public class Pc88BootEntrySummaryService
{
    public Pc88BootEntrySummary GetSummary(IFileSystem fileSystem)
    {
        var fsInfo = fileSystem.GetFileSystemInfo();
        var bootArea = fileSystem.ReadBootArea();

        if (fsInfo.FileSystemName == "N88-BASIC")
        {
            if (IsBootableArea(bootArea))
            {
                return new Pc88BootEntrySummary(Pc88BootEntryKind.N88BasicSectorResident, "N88-BASIC", MachineFamily: MachineType.PC8801);
            }
            return new Pc88BootEntrySummary(Pc88BootEntryKind.None, MachineFamily: MachineType.PC8801);
        }

        if (fsInfo.FileSystemName == "CP/M")
        {
            if (IsBootableArea(bootArea))
            {
                return new Pc88BootEntrySummary(Pc88BootEntryKind.CpmSectorResident, "CP/M", MachineFamily: MachineType.PC8801);
            }
            return new Pc88BootEntrySummary(Pc88BootEntryKind.None, MachineFamily: MachineType.PC8801);
        }

        return new Pc88BootEntrySummary(Pc88BootEntryKind.Unsupported, MachineFamily: MachineType.Unknown);
    }

    private static bool IsBootableArea(byte[] bootArea)
    {
        if (bootArea == null || bootArea.Length == 0) return false;
        // Simple heuristic: non-zero
        return bootArea.Any(b => b != 0x00 && b != 0xFF);
    }
}
