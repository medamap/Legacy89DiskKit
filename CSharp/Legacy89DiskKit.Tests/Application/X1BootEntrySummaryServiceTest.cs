using System.Text;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Xunit;

namespace Legacy89DiskKit.Tests.Application;

public class X1BootEntrySummaryServiceTest
{
    private readonly X1BootEntrySummaryService _service = new();

    private class MockFileSystem : IFileSystem
    {
        public string FileSystemName { get; set; } = "Unknown";
        public byte[] BootArea { get; set; } = new byte[1024];

        public DiskFileSystemInfo GetFileSystemInfo() => new DiskFileSystemInfo(FileSystemName, 0, 0, 0, 0, "X1");
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
    public void GetSummary_HuBasicFileBacked_ReturnsCorrectSummary()
    {
        var fs = new MockFileSystem { FileSystemName = "Hu-BASIC" };
        var bootArea = new byte[32];
        bootArea[0] = 0x01; // Bootable
        // Name: "IPL", Ext: "BAS"
        var name = Encoding.ASCII.GetBytes("IPL          ");
        var ext = Encoding.ASCII.GetBytes("BAS");
        Array.Copy(name, 0, bootArea, 1, 13);
        Array.Copy(ext, 0, bootArea, 0x0E, 3);
        bootArea[0x11] = 0x20; // No password
        bootArea[0x12] = 0x00; bootArea[0x13] = 0x10; // Size
        bootArea[0x14] = 0x00; bootArea[0x15] = 0x20; // Load = 0x2000
        bootArea[0x16] = 0x00; bootArea[0x17] = 0x20; // Exec = 0x2000
        fs.BootArea = bootArea;

        var summary = _service.GetSummary(fs);

        Assert.Equal(X1BootEntryKind.HuBasicFileBacked, summary.Kind);
        Assert.Equal("IPL.BAS", summary.DisplayName);
        Assert.Equal((ushort)0x2000, summary.LoadAddress);
        Assert.Equal((ushort)0x2000, summary.ExecutionAddress);
        Assert.Equal(Legacy89DiskKit.CharacterEncoding.Domain.Model.MachineType.X1, summary.MachineFamily);
    }

    [Fact]
    public void GetSummary_XDosSectorResident_ReturnsCorrectSummary()
    {
        var fs = new MockFileSystem { FileSystemName = "X-DOS" };
        var bootArea = new byte[256];
        bootArea[0] = 0x01;
        bootArea[24] = 0x88;
        fs.BootArea = bootArea;

        var summary = _service.GetSummary(fs);

        Assert.Equal(X1BootEntryKind.XDosSectorResident, summary.Kind);
        Assert.Equal(Legacy89DiskKit.CharacterEncoding.Domain.Model.MachineType.X1, summary.MachineFamily);
    }

    [Fact]
    public void GetSummary_None_ReturnsCorrectSummary()
    {
        var fs = new MockFileSystem { FileSystemName = "Hu-BASIC" };
        fs.BootArea = new byte[32]; // All zeros

        var summary = _service.GetSummary(fs);

        Assert.Equal(X1BootEntryKind.None, summary.Kind);
        Assert.Equal(Legacy89DiskKit.CharacterEncoding.Domain.Model.MachineType.X1, summary.MachineFamily);
    }

    [Fact]
    public void GetSummary_Unsupported_ReturnsCorrectSummary()
    {
        var fs = new MockFileSystem { FileSystemName = "MSX-DOS" };

        var summary = _service.GetSummary(fs);

        Assert.Equal(X1BootEntryKind.Unsupported, summary.Kind);
    }
}
