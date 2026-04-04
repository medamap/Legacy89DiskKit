using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Models;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.FileSystem.Application;
public sealed class BootEntryExportService : IBootEntryExportService
{
    private readonly X1BootEntrySummaryService _x1Service = new();
    private readonly Pc88BootEntrySummaryService _pc88Service = new();
    private readonly MsxBootMetadataService _msxService = new();
    private readonly HuBasicMetadataService _huBasicMetadataService = new();
    public IReadOnlyList<BootEntryExportArtifact> ExportEntries(IDiskContainer container, IFileSystem fileSystem)
    {
        var fsInfo = fileSystem.GetFileSystemInfo();
        if (fsInfo.FileSystemName == "Hu-BASIC" || fsInfo.FileSystemName == "X-DOS")
        {
            return ExportX1Entries(container, fileSystem);
        }

        if (fsInfo.FileSystemName == "N88-BASIC" || fsInfo.FileSystemName == "CP/M")
        {
            return ExportPc88Entries(fileSystem);
        }

        if (fsInfo.FileSystemName == "MSX-DOS")
        {
            return ExportMsxEntries(fileSystem);
        }

        return Array.Empty<BootEntryExportArtifact>();
    }

    private IReadOnlyList<BootEntryExportArtifact> ExportX1Entries(IDiskContainer container, IFileSystem fileSystem)
    {
        var summary = _x1Service.GetSummary(fileSystem);
        return summary.Kind switch
        {
            X1BootEntryKind.None => Array.Empty<BootEntryExportArtifact>(),
            X1BootEntryKind.XDosSectorResident => new[]
            {
                CreateArtifact(MachineType.X1, BootInfoMode.SectorResident, "BOOT", fileSystem.ReadBootArea(), null, null)
            },
            X1BootEntryKind.HuBasicFileBacked => new[]
            {
                ExportX1HuBasicFileBacked(container, fileSystem, summary)
            },
            _ => Array.Empty<BootEntryExportArtifact>()};
    }

    private BootEntryExportArtifact ExportX1HuBasicFileBacked(IDiskContainer container, IFileSystem fileSystem, X1BootEntrySummary summary)
    {
        var bootRecord = _huBasicMetadataService.GetBootRecordInfo(fileSystem) ?? throw new InvalidOperationException("Boot record metadata was not found.");
        var payload = ReadRecords(container, container.DiskType, bootRecord.StartRecord, bootRecord.Size);
        return CreateArtifact(MachineType.X1, BootInfoMode.FileBacked, summary.DisplayName, payload, bootRecord.LoadAddress, bootRecord.ExecutionAddress);
    }

    private IReadOnlyList<BootEntryExportArtifact> ExportPc88Entries(IFileSystem fileSystem)
    {
        var summary = _pc88Service.GetSummary(fileSystem);
        return summary.Kind switch
        {
            Pc88BootEntryKind.None => Array.Empty<BootEntryExportArtifact>(),
            Pc88BootEntryKind.N88BasicSectorResident => new[]
            {
                CreateArtifact(MachineType.PC8801, BootInfoMode.SectorResident, summary.DisplayName, fileSystem.ReadBootArea(), null, null)
            },
            Pc88BootEntryKind.CpmSectorResident => new[]
            {
                CreateArtifact(MachineType.PC8801, BootInfoMode.SectorResident, summary.DisplayName, fileSystem.ReadBootArea(), null, null)
            },
            _ => Array.Empty<BootEntryExportArtifact>()};
    }

    private IReadOnlyList<BootEntryExportArtifact> ExportMsxEntries(IFileSystem fileSystem)
    {
        var summary = _msxService.GetBootSummary(fileSystem);
        if (summary.Mode != BootInfoMode.SectorResident)
        {
            return Array.Empty<BootEntryExportArtifact>();
        }

        return new[]
        {
            CreateArtifact(MachineType.MSX, BootInfoMode.SectorResident, "BOOT", fileSystem.ReadBootArea(), null, null)
        };
    }

    private static BootEntryExportArtifact CreateArtifact(MachineType machineFamily, BootInfoMode mode, string? displayName, byte[] payload, ushort? loadAddress, ushort? executionAddress)
    {
        var safeDisplayName = SanitizeDisplayName(string.IsNullOrWhiteSpace(displayName) ? "Entry1" : displayName);
        var prefix = GetMachinePrefix(machineFamily);
        var baseName = $"{prefix}_BootRecord_{safeDisplayName}";
        return new BootEntryExportArtifact(machineFamily, mode, displayName, payload, $"{baseName}.bin", $"{baseName}.json", loadAddress, executionAddress);
    }

    private static string GetMachinePrefix(MachineType machineFamily) => machineFamily switch
    {
        MachineType.X1 => "X1",
        MachineType.PC8801 => "PC-8801",
        MachineType.PC9801 => "PC-9801",
        MachineType.MSX => "MSX",
        MachineType.MSX2 => "MSX",
        MachineType.FM7 => "FM-7",
        _ => "UNKNOWN"
    };
    private static string SanitizeDisplayName(string value)
    {
        var chars = value.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) || char.IsWhiteSpace(ch) || ch == '/' || ch == '\\' ? '_' : ch).ToArray();
        return new string (chars);
    }

    private static byte[] ReadRecords(IDiskContainer container, DiskType diskType, int startRecord, int size)
    {
        var config = HuBasicConfiguration.GetDefault(diskType);
        var result = new byte[size];
        var remaining = size;
        var offset = 0;
        var record = startRecord;
        while (remaining > 0)
        {
            var(cylinder, head, sector) = GetPhysicalAddressFromRecord(record, config.SectorsPerTrack);
            var sectorData = container.ReadSector(cylinder, head, sector);
            var copyLength = Math.Min(config.SectorSize, remaining);
            Array.Copy(sectorData, 0, result, offset, copyLength);
            remaining -= copyLength;
            offset += copyLength;
            record++;
        }

        return result;
    }

    private static (int cylinder, int head, int sector) GetPhysicalAddressFromRecord(int recordNumber, int sectorsPerTrack)
    {
        var cylinder = (recordNumber / sectorsPerTrack) / 2;
        var head = (recordNumber / sectorsPerTrack) % 2;
        var sector = (recordNumber % sectorsPerTrack) + 1;
        return (cylinder, head, sector);
    }
}
