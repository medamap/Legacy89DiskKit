using System.Text;
using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Xunit;

namespace Legacy89DiskKit.Tests.Application;

public class BootEntryExportServiceTest
{
    private sealed class FakeDiskContainer : IDiskContainer
    {
        private readonly Dictionary<(int Cylinder, int Head, int Sector), byte[]> _sectors = new();

        public string FilePath => "fake.d88";
        public bool IsReadOnly => true;
        public DiskType DiskType { get; init; } = DiskType.TwoD;

        public DiskContainerMetadata GetMetadata() => new("d88", DiskType, new DiskGeometryInfo(80, 2, 16, 256), true, 0);
        public byte[] ReadSector(int cylinder, int head, int sector) => _sectors[(cylinder, head, sector)];
        public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted) => ReadSector(cylinder, head, sector);
        public void WriteSector(int cylinder, int head, int sector, byte[] data) => _sectors[(cylinder, head, sector)] = data;
        public bool SectorExists(int cylinder, int head, int sector) => _sectors.ContainsKey((cylinder, head, sector));
        public IEnumerable<SectorInfo> GetAllSectors() => _sectors.Select(kvp => new SectorInfo(kvp.Key.Cylinder, kvp.Key.Head, kvp.Key.Sector, kvp.Value.Length));
        public void Save() { }
        public void SaveAs(string filePath) { }
        public void Dispose() { }
    }

    private sealed class FakeFileSystem : IFileSystem
    {
        public DiskFileSystemInfo Info { get; set; } = new("Unknown", 0, 0, 256, 0, "TEST");
        public byte[] BootArea { get; set; } = Array.Empty<byte>();

        public DiskFileSystemInfo GetFileSystemInfo() => Info;
        public FileSystemCapabilities Capabilities => FileSystemCapabilities.None;
        public IEnumerable<FileEntry> GetFiles() => Enumerable.Empty<FileEntry>();
        public bool FileExists(string fileName) => false;
        public byte[] ReadFile(string fileName) => Array.Empty<byte>();
        public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null) { }
        public void DeleteFile(string fileName) { }
        public void RenameFile(string oldName, string newName) { }
        public void CopyFile(string sourceName, string targetName) { }
        public void UpdateAttributes(string fileName, ExtendedFileAttributes attributes) { }
        public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii) => null!;
        public void Format() { }
        public byte[] ReadBootArea() => BootArea;
        public void WriteBootArea(byte[] data) => BootArea = data;
        public void Dispose() { }
    }

    [Fact]
    public void ExportEntries_X1HuBasic_UsesBootRecordStartRecordAndSize()
    {
        var service = new BootEntryExportService();
        var container = new FakeDiskContainer { DiskType = DiskType.TwoD };
        var fs = new FakeFileSystem
        {
            Info = new DiskFileSystemInfo("Hu-BASIC", 0, 0, 256, 0, "X1")
        };

        var bootArea = new byte[256];
        bootArea[0] = 0x01;
        Array.Copy(Encoding.ASCII.GetBytes("BASIC CZ8FB01".PadRight(13)), 0, bootArea, 1, 13);
        Array.Copy(Encoding.ASCII.GetBytes("Sys"), 0, bootArea, 0x0E, 3);
        BitConverter.GetBytes((ushort)300).CopyTo(bootArea, 0x12);
        BitConverter.GetBytes((ushort)0x1234).CopyTo(bootArea, 0x14);
        BitConverter.GetBytes((ushort)0x5678).CopyTo(bootArea, 0x16);
        BitConverter.GetBytes((ushort)16).CopyTo(bootArea, 0x1E);
        fs.BootArea = bootArea;

        var payload = Enumerable.Range(0, 300).Select(i => (byte)(i & 0xFF)).ToArray();
        SeedRecords(container, DiskType.TwoD, 16, payload);

        var entries = service.ExportEntries(container, fs);

        var entry = Assert.Single(entries);
        Assert.Equal("X1_BootRecord_BASIC_CZ8FB01.Sys.bin", entry.SuggestedBinaryFileName);
        Assert.Equal("X1_BootRecord_BASIC_CZ8FB01.Sys.json", entry.SuggestedMetadataFileName);
        Assert.Equal((ushort)0x1234, entry.LoadAddress);
        Assert.Equal((ushort)0x5678, entry.ExecutionAddress);
        Assert.Equal(payload, entry.Payload);
    }

    [Fact]
    public void ExportEntries_Pc88Cpm_UsesMachineOnlyPrefix()
    {
        var service = new BootEntryExportService();
        var container = new FakeDiskContainer { DiskType = DiskType.TwoD };
        var fs = new FakeFileSystem
        {
            Info = new DiskFileSystemInfo("CP/M", 0, 0, 256, 0, "PC88"),
            BootArea = new byte[] { 0xEB, 0x3C, 0x90 }
        };

        var entries = service.ExportEntries(container, fs);

        var entry = Assert.Single(entries);
        Assert.Equal("PC-8801_BootRecord_CP_M.bin", entry.SuggestedBinaryFileName);
        Assert.Equal(fs.BootArea, entry.Payload);
    }

    [Fact]
    public void ExportEntries_MsxDos_UsesMachineOnlyPrefix()
    {
        var service = new BootEntryExportService();
        var container = new FakeDiskContainer { DiskType = DiskType.TwoDD };
        var fs = new FakeFileSystem
        {
            Info = new DiskFileSystemInfo("MSX-DOS", 0, 0, 512, 0, "MSX"),
            BootArea = new byte[] { 0xEB, 0xFE, 0x90 }
        };

        var entries = service.ExportEntries(container, fs);

        var entry = Assert.Single(entries);
        Assert.Equal("MSX_BootRecord_BOOT.bin", entry.SuggestedBinaryFileName);
        Assert.Equal(fs.BootArea, entry.Payload);
    }

    [Fact]
    public void ApplicationFactory_CreatesBootEntryExportService()
    {
        var service = Legacy89DiskKitApplication.CreateBootEntryExportService();
        Assert.IsType<BootEntryExportService>(service);
    }

    private static void SeedRecords(FakeDiskContainer container, DiskType diskType, int startRecord, byte[] payload)
    {
        var sectorsPerTrack = diskType == DiskType.TwoHD ? 26 : 16;
        var sectorSize = 256;
        var remaining = payload.Length;
        var offset = 0;
        var record = startRecord;

        while (remaining > 0)
        {
            var cylinder = (record / sectorsPerTrack) / 2;
            var head = (record / sectorsPerTrack) % 2;
            var sector = (record % sectorsPerTrack) + 1;
            var block = new byte[sectorSize];
            var copy = Math.Min(sectorSize, remaining);
            Array.Copy(payload, offset, block, 0, copy);
            container.WriteSector(cylinder, head, sector, block);
            offset += copy;
            remaining -= copy;
            record++;
        }
    }
}
