using System.Text;
using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Domain.CharacterEncoding.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Xunit;

namespace Legacy89DiskKit.Tests.Application;

public class BootProfileServiceTest
{
    private readonly CompositeBootProfileService _service = new();

    private class MockFileSystem : IFileSystem
    {
        public string FileSystemName { get; set; } = "Unknown";
        public byte[] BootArea { get; set; } = Array.Empty<byte>();

        public DiskFileSystemInfo GetFileSystemInfo() => new DiskFileSystemInfo(FileSystemName, 0, 0, 0, 0, "TEST");
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
    public void GetBootProfile_ReturnsMsxProfile_ForMsxDosWithBootCode()
    {
        // Arrange
        var fs = new MockFileSystem 
        { 
            FileSystemName = "MSX-DOS",
            BootArea = new byte[] { 0xEB, 0xFE, 0x90 }
        };

        // Act
        var result = _service.GetBootProfile(fs);

        // Assert
        Assert.Equal(BootInfoMode.SectorResident, result.Mode);
        Assert.Equal(MachineType.MSX, result.MachineFamily);
    }

    [Fact]
    public void GetBootProfile_ReturnsX1Profile_ForHuBasicFileBacked()
    {
        // Arrange
        var fs = new MockFileSystem { FileSystemName = "Hu-BASIC" };
        
        // Hu-BASIC boot record at start of boot area
        var bootArea = new byte[256];
        bootArea[0] = 0x01; // Marker
        // Name: "BOOT" at offset 1 (13 bytes)
        var nameBytes = Encoding.ASCII.GetBytes("BOOT".PadRight(13));
        Array.Copy(nameBytes, 0, bootArea, 1, 13);
        // Extension: "SYS" at offset 14 (3 bytes)
        var extBytes = Encoding.ASCII.GetBytes("SYS");
        Array.Copy(extBytes, 0, bootArea, 0x0E, 3);
        // Load Address: 0x1234 at offset 0x14
        bootArea[0x14] = 0x34; bootArea[0x15] = 0x12;
        // Exec Address: 0x5678 at offset 0x16
        bootArea[0x16] = 0x78; bootArea[0x17] = 0x56;
        
        fs.BootArea = bootArea;

        // Act
        var result = _service.GetBootProfile(fs);

        // Assert
        Assert.Equal(BootInfoMode.FileBacked, result.Mode);
        Assert.Equal(MachineType.X1, result.MachineFamily);
        Assert.Equal("BOOT.SYS", result.FileName);
        Assert.Equal((ushort)0x1234, result.LoadAddress);
        Assert.Equal((ushort)0x5678, result.ExecutionAddress);
    }

    [Fact]
    public void GetBootProfile_ReturnsX1Profile_ForXDosSectorResident()
    {
        // Arrange
        var fs = new MockFileSystem { FileSystemName = "X-DOS" };
        
        var bootArea = new byte[256];
        bootArea[0] = 0x01;
        bootArea[24] = 0x88; // X1 2D
        
        fs.BootArea = bootArea;

        // Act
        var result = _service.GetBootProfile(fs);

        // Assert
        Assert.Equal(BootInfoMode.SectorResident, result.Mode);
        Assert.Equal(MachineType.X1, result.MachineFamily);
    }

    [Fact]
    public void GetBootProfile_ReturnsPc88Profile_ForN88BasicSectorResident()
    {
        // Arrange
        var fs = new MockFileSystem 
        { 
            FileSystemName = "N88-BASIC",
            BootArea = new byte[] { 0xEB, 0xFE, 0x90 }
        };

        // Act
        var result = _service.GetBootProfile(fs);

        // Assert
        Assert.Equal(BootInfoMode.SectorResident, result.Mode);
        Assert.Equal(MachineType.PC8801, result.MachineFamily);
        Assert.Equal("N88-BASIC", result.DisplayName);
    }

    [Fact]
    public void GetBootProfile_ReturnsPc88Profile_ForCpmSectorResident()
    {
        // Arrange
        var fs = new MockFileSystem 
        { 
            FileSystemName = "CP/M",
            BootArea = new byte[] { 0xEB, 0xFE, 0x90 }
        };

        // Act
        var result = _service.GetBootProfile(fs);

        // Assert
        Assert.Equal(BootInfoMode.SectorResident, result.Mode);
        Assert.Equal(MachineType.PC8801, result.MachineFamily);
        Assert.Equal("CP/M", result.DisplayName);
    }

    [Fact]
    public void GetBootProfile_ReturnsNone_ForEmptyFileSystem()
    {
        // Arrange
        var fs = new MockFileSystem { FileSystemName = "Unknown" };

        // Act
        var result = _service.GetBootProfile(fs);

        // Assert
        Assert.Equal(BootInfoMode.None, result.Mode);
    }
}
