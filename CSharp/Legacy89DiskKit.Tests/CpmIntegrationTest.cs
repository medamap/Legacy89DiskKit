using Legacy89DiskKit.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class CpmIntegrationTest
{
    private class MockDiskContainer : Legacy89DiskKit.Domain.DiskImage.Interface.Container.IDiskContainer
    {
        public string FilePath => "mock.d88";
        public DiskType DiskType => DiskType.TwoD;
        public bool IsReadOnly => true;
        public byte[] SectorData { get; set; } = new byte[256];

        public DiskContainerMetadata GetMetadata() => new DiskContainerMetadata(
            "d88",
            DiskType,
            new DiskGeometryInfo(40, 2, 16, 256),
            IsReadOnly,
            40 * 2 * 16 * 256
        );

        public byte[] ReadSector(int track, int head, int sector) => ReadSector(track, head, sector, false);
        
        public byte[] ReadSector(int track, int head, int sector, bool allowCorrupted)
        {
            if (track == 2 && sector == 1)
            {
                // Return a mock CP/M directory sector
                var data = new byte[256];
                // Entry 0: User 0, name "TEST    ", ext "COM"
                data[0] = 0;
                System.Text.Encoding.ASCII.GetBytes("TEST    COM").CopyTo(data, 1);
                return data;
            }
            if (track == 0 && sector == 1)
            {
                // Return a mock boot sector
                var data = new byte[256];
                data[0] = 0xEB; data[1] = 0xFE; data[2] = 0x90; // Boot signature
                return data;
            }
            return new byte[256];
        }

        public void WriteSector(int track, int head, int sector, byte[] data) { }
        public void Flush() { }
        public void Dispose() { }
        public bool SectorExists(int track, int head, int sector) => true;
        public IEnumerable<SectorInfo> GetAllSectors() => Enumerable.Empty<SectorInfo>();
        public void Save() { }
        public void SaveAs(string filePath) { }
    }

    [Fact]
    public void Registry_ShouldDetectCpm_WithMockContainer()
    {
        // Arrange
        var registry = Legacy89DiskKitApplication.CreateFileSystemRegistry();
        var container = new MockDiskContainer();

        // Act
        var fileSystem = registry.DetectAndCreate(container);

        // Assert
        Assert.NotNull(fileSystem);
        Assert.Equal("CP/M", fileSystem.GetFileSystemInfo().FileSystemName);
    }

    [Fact]
    public void BootProfileService_ShouldReturnCpmProfile_ForCpmFileSystem()
    {
        // Arrange
        var bootService = Legacy89DiskKitApplication.CreateBootProfileService();
        var container = new MockDiskContainer();
        var fileSystem = new Infrastructure.FileSystem.Cpm.CpmFileSystem(container);

        // Act
        var profile = bootService.GetBootProfile(fileSystem);

        // Assert
        Assert.Equal(BootInfoMode.SectorResident, profile.Mode);
        Assert.Equal("CP/M", profile.DisplayName);
        Assert.Equal(Legacy89DiskKit.Domain.CharacterEncoding.Model.MachineType.PC8801, profile.MachineFamily);
    }

    [Fact]
    public void ExplicitResolver_ShouldKeepCpmReserved()
    {
        var resolver = Legacy89DiskKitApplication.CreateExplicitFileSystemResolver();
        var container = new MockDiskContainer();

        var ex = Assert.Throws<NotSupportedException>(() => resolver.Create("cpm", container));
        Assert.Contains("This feature is reserved, please request!!", ex.Message);
    }
}
