using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Xunit;

namespace Legacy89DiskKit.Tests.Application;

public class MsxBootMetadataServiceTest
{
    private sealed class FakeFileSystem : IFileSystem
    {
        public DiskFileSystemInfo Info { get; set; } = new("MSX-DOS", 0, 0, 512, 0, "MSX", "MSX", 8, 3);
        public byte[] BootArea { get; set; } = Array.Empty<byte>();

        public DiskFileSystemInfo GetFileSystemInfo() => Info;
        public byte[] ReadBootArea() => BootArea;

        // Unused members
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
        public void WriteBootArea(byte[] data) { }
        public void Dispose() { }
    }

    [Fact]
    public void GetBootSummary_MsxDosWithBootArea_ReturnsSectorResident()
    {
        var service = new MsxBootMetadataService();
        var fs = new FakeFileSystem
        {
            Info = new DiskFileSystemInfo("MSX-DOS", 0, 0, 512, 0, "MSX", "MSX", 8, 3),
            BootArea = new byte[512]
        };
        fs.BootArea[0] = 0xEB; // JMP

        var summary = service.GetBootSummary(fs);

        Assert.Equal(BootInfoMode.SectorResident, summary.Mode);
    }

    [Fact]
    public void GetBootSummary_MsxDosWithEmptyBootArea_ReturnsNone()
    {
        var service = new MsxBootMetadataService();
        var fs = new FakeFileSystem
        {
            Info = new DiskFileSystemInfo("MSX-DOS", 0, 0, 512, 0, "MSX", "MSX", 8, 3),
            BootArea = new byte[512] // All zeros
        };

        var summary = service.GetBootSummary(fs);

        Assert.Equal(BootInfoMode.None, summary.Mode);
    }

    [Fact]
    public void GetBootSummary_NotMsxDos_ReturnsNone()
    {
        var service = new MsxBootMetadataService();
        var fs = new FakeFileSystem
        {
            Info = new DiskFileSystemInfo("Hu-BASIC", 0, 0, 256, 0, "X1", "X1", 8, 3),
            BootArea = new byte[256]
        };
        fs.BootArea[0] = 0x01;

        var summary = service.GetBootSummary(fs);

        Assert.Equal(BootInfoMode.None, summary.Mode);
    }
}

public class MsxBootProfileDetectorServiceTest
{
    private sealed class FakeFileSystem : IFileSystem
    {
        public DiskFileSystemInfo Info { get; set; } = new("MSX-DOS", 0, 0, 512, 0, "MSX", "MSX", 8, 3);
        public byte[] BootArea { get; set; } = Array.Empty<byte>();

        public DiskFileSystemInfo GetFileSystemInfo() => Info;
        public byte[] ReadBootArea() => BootArea;

        // Unused members
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
        public void WriteBootArea(byte[] data) { }
        public void Dispose() { }
    }

    [Fact]
    public void DetectProfile_MsxDosWithBootArea_ReturnsMsxDos()
    {
        var service = new MsxBootProfileDetectorService();
        var fs = new FakeFileSystem
        {
            Info = new DiskFileSystemInfo("MSX-DOS", 0, 0, 512, 0, "MSX", "MSX", 8, 3),
            BootArea = new byte[512]
        };
        fs.BootArea[0] = 0xEB;

        var profile = service.DetectProfile(fs);

        Assert.Equal("MSX_DOS", profile);
    }

    [Fact]
    public void DetectProfile_MsxDosWithEmptyBootArea_ReturnsNull()
    {
        var service = new MsxBootProfileDetectorService();
        var fs = new FakeFileSystem
        {
            Info = new DiskFileSystemInfo("MSX-DOS", 0, 0, 512, 0, "MSX", "MSX", 8, 3),
            BootArea = new byte[512]
        };

        var profile = service.DetectProfile(fs);

        Assert.Null(profile);
    }

    [Fact]
    public void DetectProfile_NotMsxDos_ReturnsNull()
    {
        var service = new MsxBootProfileDetectorService();
        var fs = new FakeFileSystem
        {
            Info = new DiskFileSystemInfo("Hu-BASIC", 0, 0, 256, 0, "X1", "X1", 8, 3),
            BootArea = new byte[256]
        };
        fs.BootArea[0] = 0x01;

        var profile = service.DetectProfile(fs);

        Assert.Null(profile);
    }
}
