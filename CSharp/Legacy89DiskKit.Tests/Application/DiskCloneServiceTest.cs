using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Xunit;

namespace Legacy89DiskKit.Tests.Application;

public class DiskCloneServiceTest
{
    private sealed class FakeFileSystem : IFileSystem
    {
        public List<FileEntry> Files { get; } = new();
        public List<string> DirectWrites { get; } = new();
        public DiskFileSystemInfo Info { get; set; } = new("FakeFs", 0, 0, 256, 0, "FAKE", "FAKE", 8, 3);

        public DiskFileSystemInfo GetFileSystemInfo() => Info;
        public FileSystemCapabilities Capabilities =>
            FileSystemCapabilities.SupportsBootArea |
            FileSystemCapabilities.SupportsAttributes |
            FileSystemCapabilities.SupportsInternalCopy |
            FileSystemCapabilities.SupportsRename;
        public IEnumerable<FileEntry> GetFiles() => Files;
        public bool FileExists(string fileName) => Files.Any(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        public byte[] ReadFile(string fileName) => Array.Empty<byte>();
        public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null) => DirectWrites.Add(fileName);
        public void DeleteFile(string fileName) { }
        public void RenameFile(string oldName, string newName) { }
        public void CopyFile(string sourceName, string targetName) { }
        public void UpdateAttributes(string fileName, ExtendedFileAttributes attributes) { }
        public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii) => new(Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes.None, 0, isAscii, "FAKE");
        public void Format() { }
        public byte[] ReadBootArea() => Array.Empty<byte>();
        public void WriteBootArea(byte[] data) { }
        public void Dispose() { }
    }

    private sealed class FakeTransferAdapter : IFileSystemTransferAdapter
    {
        private readonly IFileSystem _owner;
        public List<string> ImportedFileNames { get; } = new();
        public string FileSystemId { get; }

        public FakeTransferAdapter(IFileSystem owner, string fileSystemId)
        {
            _owner = owner;
            FileSystemId = fileSystemId;
        }

        public bool Supports(IFileSystem fs) => ReferenceEquals(fs, _owner);

        public TransferFileEnvelope Export(FileEntry entry) =>
            new(
                entry.FileName,
                new byte[] { 1, 2, 3 },
                entry.Attributes.IsAscii ? ContentKind.Text : ContentKind.Binary,
                FileSystemId,
                entry.LoadAddress,
                entry.ExecutionAddress,
                null,
                null,
                new Dictionary<string, string>());

        public void Import(TransferFileEnvelope envelope, string destFileName) => ImportedFileNames.Add(destFileName);
    }

    private class FakeDiskContainer : IDiskContainer
    {
        public string FilePath => "fake.d88";
        public bool IsReadOnly => false;
        public DiskType DiskType { get; set; }
        public List<SectorInfo> Sectors { get; set; } = new();
        public Dictionary<(int, int, int), byte[]> Data { get; set; } = new();
        public bool ReadThrowAtSector1 { get; set; }

        public DiskContainerMetadata GetMetadata() => new DiskContainerMetadata("Fake", DiskType, new DiskGeometryInfo(0, 0, 0, 0), false, 0);
        public byte[] ReadSector(int c, int h, int s) => ReadSector(c, h, s, false);
        public byte[] ReadSector(int c, int h, int s, bool allowCorrupted)
        {
            if (ReadThrowAtSector1 && s == 1) throw new Exception("Read error");
            return Data[(c, h, s)];
        }
        public void WriteSector(int c, int h, int s, byte[] data) => Data[(c, h, s)] = data;
        public bool SectorExists(int c, int h, int s) => Data.ContainsKey((c, h, s));
        public bool CylinderExists(int c) => Sectors.Any(s => s.Cylinder == c);
        public bool HeadExists(int c, int h) => Sectors.Any(s => s.Cylinder == c && s.Head == h);
        public IEnumerable<SectorInfo> GetAllSectors() => Sectors;
        public void Save() { }
        public void SaveAs(string path) { }
        public void Dispose() { }
    }

    [Fact]
    public void CopySectors_SameDiskType_CopiesAllSectors()
    {
        var source = new FakeDiskContainer { DiskType = DiskType.TwoD };
        var destination = new FakeDiskContainer { DiskType = DiskType.TwoD };
        
        source.Sectors.Add(new SectorInfo(0, 0, 1, 256));
        source.Sectors.Add(new SectorInfo(0, 0, 2, 256));
        source.Data[(0, 0, 1)] = new byte[256];
        source.Data[(0, 0, 2)] = new byte[256];
        
        var service = new DiskCloneService(null!, null!);
        var result = service.CopySectors(source, destination);
        
        Assert.Equal(1, result.tracksCopied);
        Assert.Equal(0, result.sectorsSkipped);
        Assert.True(destination.Data.ContainsKey((0, 0, 1)));
        Assert.True(destination.Data.ContainsKey((0, 0, 2)));
    }

    [Fact]
    public void CopySectors_DifferentDiskType_Throws()
    {
        var source = new FakeDiskContainer { DiskType = DiskType.TwoD };
        var destination = new FakeDiskContainer { DiskType = DiskType.TwoHD };
        
        var service = new DiskCloneService(null!, null!);
        Assert.Throws<ArgumentException>(() => service.CopySectors(source, destination));
    }

    [Fact]
    public void CopySectors_SourceReadFailure_ContinuesAndReports()
    {
        var source = new FakeDiskContainer { DiskType = DiskType.TwoD, ReadThrowAtSector1 = true };
        var destination = new FakeDiskContainer { DiskType = DiskType.TwoD };
        
        source.Sectors.Add(new SectorInfo(0, 0, 1, 256));
        source.Sectors.Add(new SectorInfo(0, 0, 2, 256));
        source.Data[(0, 0, 2)] = new byte[256];
        
        var service = new DiskCloneService(null!, null!);
        var result = service.CopySectors(source, destination, true);
        
        Assert.Equal(1, result.tracksCopied);
        Assert.Equal(1, result.sectorsSkipped);
        Assert.False(destination.Data.ContainsKey((0, 0, 1)));
        Assert.True(destination.Data.ContainsKey((0, 0, 2)));
    }

    [Fact]
    public void CopySectors_SourceReadFailure_Throws_WhenPartialReadNotAllowed()
    {
        var source = new FakeDiskContainer { DiskType = DiskType.TwoD, ReadThrowAtSector1 = true };
        var destination = new FakeDiskContainer { DiskType = DiskType.TwoD };
        source.Sectors.Add(new SectorInfo(0, 0, 1, 256));

        var service = new DiskCloneService(null!, null!);
        Assert.Throws<Exception>(() => service.CopySectors(source, destination, allowPartialRead: false));
    }

    [Fact]
    public void TransferFiles_WithAdapters_UsesTransferPipelineInsteadOfDirectWrites()
    {
        var source = new FakeFileSystem();
        var destination = new FakeFileSystem();
        source.Files.Add(new FileEntry(
            "X-DOS System",
            "",
            3,
            null,
            null,
            new ExtendedFileAttributes(Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes.Hidden, 0x80, false, "X-DOS"),
            0,
            0xC800,
            null,
            0xC800));

        var sourceAdapter = new FakeTransferAdapter(source, "X-DOS");
        var destinationAdapter = new FakeTransferAdapter(destination, "X-DOS");
        var service = new DiskCloneService(null!, null!);

        service.TransferFiles(source, destination, new[] { "X-DOS System" }, sourceAdapter, destinationAdapter);

        Assert.Empty(destination.DirectWrites);
        Assert.Single(destinationAdapter.ImportedFileNames);
        Assert.Equal("X-DOS System", destinationAdapter.ImportedFileNames[0]);
    }
}
