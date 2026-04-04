using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Xunit;

namespace Legacy89DiskKit.Tests.Application;

public class Pc88BootEntrySummaryServiceTest
{
    private readonly Pc88BootEntrySummaryService _service = new();

    private class MockFileSystem : IFileSystem
    {
        public string FileSystemName { get; set; } = "Unknown";
        public byte[] BootArea { get; set; } = new byte[256];

        public DiskFileSystemInfo GetFileSystemInfo() => new DiskFileSystemInfo(FileSystemName, 0, 0, 0, 0, "PC88");
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
    public void GetSummary_N88BasicSectorResident_ReturnsCorrectSummary()
    {
        var fs = new MockFileSystem { FileSystemName = "N88-BASIC" };
        var bootArea = new byte[256];
        bootArea[0] = 0xEB; bootArea[1] = 0x3C; bootArea[2] = 0x90; // Some code
        fs.BootArea = bootArea;

        var summary = _service.GetSummary(fs);

        Assert.Equal(Pc88BootEntryKind.N88BasicSectorResident, summary.Kind);
        Assert.Equal("N88-BASIC", summary.DisplayName);
        Assert.Equal(Legacy89DiskKit.Domain.CharacterEncoding.Model.MachineType.PC8801, summary.MachineFamily);
    }

    [Fact]
    public void GetSummary_CpmSectorResident_ReturnsCorrectSummary()
    {
        var fs = new MockFileSystem { FileSystemName = "CP/M" };
        var bootArea = new byte[256];
        bootArea[0] = 0xEB; bootArea[1] = 0x3C; bootArea[2] = 0x90; // Some code
        fs.BootArea = bootArea;

        var summary = _service.GetSummary(fs);

        Assert.Equal(Pc88BootEntryKind.CpmSectorResident, summary.Kind);
        Assert.Equal("CP/M", summary.DisplayName);
        Assert.Equal(Legacy89DiskKit.Domain.CharacterEncoding.Model.MachineType.PC8801, summary.MachineFamily);
    }

    [Fact]
    public void GetSummary_N88BasicNone_ReturnsCorrectSummary()
    {
        var fs = new MockFileSystem { FileSystemName = "N88-BASIC" };
        fs.BootArea = new byte[256]; // All zeros

        var summary = _service.GetSummary(fs);

        Assert.Equal(Pc88BootEntryKind.None, summary.Kind);
    }

    [Fact]
    public void GetSummary_CpmEmpty_ReturnsNone()
    {
        var fs = new MockFileSystem { FileSystemName = "CP/M" };
        fs.BootArea = new byte[256]; // All zeros

        var summary = _service.GetSummary(fs);

        Assert.Equal(Pc88BootEntryKind.None, summary.Kind);
    }

    [Fact]
    public void GetSummary_Unsupported_ReturnsCorrectSummary()
    {
        var fs = new MockFileSystem { FileSystemName = "Unknown" };

        var summary = _service.GetSummary(fs);

        Assert.Equal(Pc88BootEntryKind.Unsupported, summary.Kind);
    }
}
