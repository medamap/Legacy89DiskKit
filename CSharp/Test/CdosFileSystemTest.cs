using Xunit;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Infrastructure.Factory;
using Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;
using Legacy89DiskKit.FileSystem.Infrastructure.Utility;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using System.Text;

namespace Test;

public class CdosFileSystemTest : IDisposable
{
    private readonly string _tempDirectory;
    private readonly DiskContainerFactory _diskContainerFactory;
    private readonly List<string> _tempFiles = new();

    public CdosFileSystemTest()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"CdosTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
        _diskContainerFactory = new DiskContainerFactory();
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            try { File.Delete(file); } catch { }
        }
        try { Directory.Delete(_tempDirectory, true); } catch { }
    }

    [Theory]
    [InlineData(DiskType.TwoD)]
    [InlineData(DiskType.TwoHD)]
    public void Format_CreatesEmptyFileSystem(DiskType diskType)
    {
        // Arrange
        var diskPath = CreateTempDiskImage($"format_test_{diskType}.d88", diskType);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);

        // Act
        fs.Format();

        // Assert
        var files = fs.GetFiles().ToList();
        Assert.Empty(files);
        Assert.True(fs.IsFormatted);
        Assert.Equal("CDOS DISK", fs.GetVolumeLabel());
    }

    [Theory]
    [InlineData(DiskType.TwoD)]
    [InlineData(DiskType.TwoHD)]
    public void ImportFile_ValidFile_Success(DiskType diskType)
    {
        // Arrange
        var diskPath = CreateTempDiskImage($"import_test_{diskType}.d88", diskType);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        var testData = "Hello, CDOS World!"u8.ToArray();
        var testFilePath = CreateTempFile("test.txt", testData);

        // Act
        fs.ImportFile(testFilePath, "TEST.TXT");

        // Assert
        var files = fs.GetFiles().ToList();
        Assert.Single(files);
        Assert.Equal("TEST", files[0].FileName);
        Assert.Equal("TXT", files[0].Extension);
        Assert.Equal(testData.Length, files[0].Size);
    }

    [Theory]
    [InlineData(DiskType.TwoD)]
    [InlineData(DiskType.TwoHD)]
    public void ExportFile_ExistingFile_Success(DiskType diskType)
    {
        // Arrange
        var diskPath = CreateTempDiskImage($"export_test_{diskType}.d88", diskType);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        var testData = "CDOS Export Test Data"u8.ToArray();
        var testFilePath = CreateTempFile("input.txt", testData);
        fs.ImportFile(testFilePath, "DATA.TXT");

        var exportPath = Path.Combine(_tempDirectory, "exported.txt");

        // Act
        fs.ExportFile("DATA.TXT", exportPath);

        // Assert
        Assert.True(File.Exists(exportPath));
        var exportedData = File.ReadAllBytes(exportPath);
        Assert.Equal(testData, exportedData);
    }

    [Theory]
    [InlineData(DiskType.TwoD)]
    [InlineData(DiskType.TwoHD)]
    public void ReadFile_ExistingFile_ReturnsCorrectData(DiskType diskType)
    {
        // Arrange
        var diskPath = CreateTempDiskImage($"read_test_{diskType}.d88", diskType);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        var testData = "CDOS Read Test - 読み込みテスト"u8.ToArray();
        var testFilePath = CreateTempFile("test.dat", testData);
        fs.ImportFile(testFilePath, "READ.DAT");

        // Act
        var readData = fs.ReadFile("READ.DAT");

        // Assert
        Assert.Equal(testData, readData);
    }

    [Theory]
    [InlineData(DiskType.TwoD)]
    [InlineData(DiskType.TwoHD)]
    public void WriteFile_NewFile_Success(DiskType diskType)
    {
        // Arrange
        var diskPath = CreateTempDiskImage($"write_test_{diskType}.d88", diskType);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        var testData = "Direct write test data"u8.ToArray();

        // Act
        fs.WriteFile("WRITE.BIN", testData, false, 0x8000, 0x8100);

        // Assert
        var files = fs.GetFiles().ToList();
        Assert.Single(files);
        Assert.Equal("WRITE", files[0].FileName);
        Assert.Equal("BIN", files[0].Extension);
        Assert.Equal(testData.Length, files[0].Size);
        Assert.Equal(0x8000, files[0].LoadAddress);
        Assert.Equal(0x8100, files[0].ExecuteAddress);

        var readData = fs.ReadFile("WRITE.BIN");
        Assert.Equal(testData, readData);
    }

    [Theory]
    [InlineData(DiskType.TwoD)]
    [InlineData(DiskType.TwoHD)]
    public void DeleteFile_ExistingFile_Success(DiskType diskType)
    {
        // Arrange
        var diskPath = CreateTempDiskImage($"delete_test_{diskType}.d88", diskType);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        var testData = "File to be deleted"u8.ToArray();
        fs.WriteFile("DELETE.ME", testData);

        // Verify file exists
        Assert.True(fs.FileExists("DELETE.ME"));
        Assert.Single(fs.GetFiles());

        // Act
        fs.DeleteFile("DELETE.ME");

        // Assert
        Assert.False(fs.FileExists("DELETE.ME"));
        Assert.Empty(fs.GetFiles());
    }

    [Theory]
    [InlineData(DiskType.TwoD)]
    [InlineData(DiskType.TwoHD)]
    public void GetFiles_WithPattern_FiltersCorrectly(DiskType diskType)
    {
        // Arrange
        var diskPath = CreateTempDiskImage($"pattern_test_{diskType}.d88", diskType);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        // Create test files
        fs.WriteFile("TEST1.TXT", "data1"u8.ToArray());
        fs.WriteFile("TEST2.TXT", "data2"u8.ToArray());
        fs.WriteFile("DEMO.BAS", "basic code"u8.ToArray());
        fs.WriteFile("MAIN.BIN", "binary data"u8.ToArray());

        // Act & Assert
        var txtFiles = fs.GetFiles("*.TXT").ToList();
        Assert.Equal(2, txtFiles.Count);
        Assert.All(txtFiles, f => Assert.Equal("TXT", f.Extension));

        var testFiles = fs.GetFiles("TEST*").ToList();
        Assert.Equal(2, testFiles.Count);
        Assert.All(testFiles, f => Assert.StartsWith("TEST", f.FileName));

        var allFiles = fs.GetFiles("*.*").ToList();
        Assert.Equal(4, allFiles.Count);
    }

    [Fact]
    public void CdosConfiguration_TwoD_HasCorrectSettings()
    {
        // Act
        var config = CdosConfiguration.GetConfiguration(DiskType.TwoD);

        // Assert
        Assert.Equal(DiskType.TwoD, config.DiskType);
        Assert.Equal(40, config.TrackCount);
        Assert.Equal(2, config.HeadCount);
        Assert.Equal(16, config.SectorsPerTrack);
        Assert.Equal(256, config.SectorSize);
        Assert.Equal(1, config.DirectoryStartTrack);
        Assert.Equal(1, config.DirectoryStartSector);
        Assert.Equal(128, config.MaxDirectoryEntries);
        Assert.False(config.IsMixedSectorSize);
    }

    [Fact]
    public void CdosConfiguration_TwoHD_HasCorrectSettings()
    {
        // Act
        var config = CdosConfiguration.GetConfiguration(DiskType.TwoHD);

        // Assert
        Assert.Equal(DiskType.TwoHD, config.DiskType);
        Assert.Equal(77, config.TrackCount);
        Assert.Equal(2, config.HeadCount);
        Assert.Equal(8, config.SectorsPerTrack);
        Assert.Equal(1024, config.SectorSize);
        Assert.Equal(1, config.DirectoryStartTrack);
        Assert.Equal(1, config.DirectoryStartSector);
        Assert.Equal(128, config.MaxDirectoryEntries);
        Assert.True(config.IsMixedSectorSize);
        Assert.Equal(128, config.Track0SectorSize);
        Assert.Equal(26, config.Track0SectorsPerTrack);
    }

    [Theory]
    [InlineData("TEST.TXT", true)]
    [InlineData("VERYLONGNAME.EXT", false)]  // Name too long (12 chars)
    [InlineData("TEST.LONG", false)]     // Extension too long (4 chars)
    [InlineData("TEST?.TXT", false)]     // Invalid character
    [InlineData("TEST", true)]           // No extension
    [InlineData("A.B", true)]            // Short names
    [InlineData("", false)]              // Empty name
    public void CdosFileNameValidator_ValidatesCorrectly(string fileName, bool expectedValid)
    {
        // Act
        var isValid = CdosFileNameValidator.IsValidFileName(fileName);

        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    [Theory]
    [InlineData("test.txt", "TEST", "TXT")]
    [InlineData("demo", "DEMO", "")]
    [InlineData("file.bas", "FILE", "BAS")]
    [InlineData("verylongname.extension", "VERYLONG", "EXT")]  // Truncated to 8 chars
    public void CdosFileNameValidator_NormalizesCorrectly(string input, string expectedName, string expectedExt)
    {
        // Act
        var (name, extension) = CdosFileNameValidator.NormalizeFileName(input);

        // Assert
        Assert.Equal(expectedName, name);
        Assert.Equal(expectedExt, extension);
    }

    [Theory]
    [InlineData(DiskType.TwoD)]
    [InlineData(DiskType.TwoHD)]
    public void ImportLargeFile_Success(DiskType diskType)
    {
        // Arrange
        var diskPath = CreateTempDiskImage($"large_file_test_{diskType}.d88", diskType);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        // Create a large test file (10KB)
        var largeData = new byte[10240];
        for (int i = 0; i < largeData.Length; i++)
        {
            largeData[i] = (byte)(i % 256);
        }
        var largeFilePath = CreateTempFile("large.dat", largeData);

        // Act
        fs.ImportFile(largeFilePath, "LARGE.DAT");

        // Assert
        var files = fs.GetFiles().ToList();
        Assert.Single(files);
        Assert.Equal("LARGE", files[0].FileName);
        Assert.Equal("DAT", files[0].Extension);
        Assert.Equal(largeData.Length, files[0].Size);

        var readData = fs.ReadFile("LARGE.DAT");
        Assert.Equal(largeData, readData);
    }

    [Theory]
    [InlineData(DiskType.TwoD)]
    [InlineData(DiskType.TwoHD)]
    public void GetFileSystemInfo_ReturnsCorrectInfo(DiskType diskType)
    {
        // Arrange
        var diskPath = CreateTempDiskImage($"info_test_{diskType}.d88", diskType);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        // Act
        var info = fs.GetFileSystemInfo();

        // Assert
        Assert.True(info.TotalClusters > 0);
        Assert.True(info.FreeClusters > 0);
        Assert.True(info.FreeClusters <= info.TotalClusters);
        Assert.True(info.ClusterSize > 0);
        Assert.True(info.SectorSize > 0);
    }

    [Fact]
    public void FileExists_NonExistentFile_ReturnsFalse()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("exists_test.d88", DiskType.TwoD);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        // Act & Assert
        Assert.False(fs.FileExists("NOTHERE.TXT"));
    }

    [Fact]
    public void GetFileSize_ExistingFile_ReturnsCorrectSize()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("size_test.d88", DiskType.TwoD);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        var testData = "Size test data - exactly 32 chars"u8.ToArray();
        fs.WriteFile("SIZE.TST", testData);

        // Act
        var size = fs.GetFileSize("SIZE.TST");

        // Assert
        Assert.Equal(testData.Length, size);
    }

    [Fact]
    public void ReadFile_NonExistentFile_ThrowsException()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("error_test.d88", DiskType.TwoD);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        // Act & Assert
        Assert.Throws<Legacy89DiskKit.FileSystem.Domain.Exception.FileNotFoundException>(
            () => fs.ReadFile("NOTHERE.TXT"));
    }

    [Fact]
    public void DeleteFile_NonExistentFile_ThrowsException()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("delete_error_test.d88", DiskType.TwoD);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        // Act & Assert
        Assert.Throws<Legacy89DiskKit.FileSystem.Domain.Exception.FileNotFoundException>(
            () => fs.DeleteFile("NOTHERE.TXT"));
    }

    [Fact]
    public void ImportFile_DuplicateFileName_ThrowsException()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("duplicate_test.d88", DiskType.TwoD);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        var testData = "test data"u8.ToArray();
        var testFilePath = CreateTempFile("test.txt", testData);
        fs.ImportFile(testFilePath, "DUPE.TXT");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => fs.ImportFile(testFilePath, "DUPE.TXT"));
    }

    [Fact]
    public void CreateDirectory_ThrowsNotSupportedException()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("dir_test.d88", DiskType.TwoD);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => fs.CreateDirectory("SUBDIR"));
    }

    [Fact]
    public void DeleteDirectory_ThrowsNotSupportedException()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("deldir_test.d88", DiskType.TwoD);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => fs.DeleteDirectory("SUBDIR"));
    }

    [Fact]
    public void SetVolumeLabel_DoesNothing()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("label_test.d88", DiskType.TwoD);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CdosFileSystem(container);
        fs.Format();

        var originalLabel = fs.GetVolumeLabel();

        // Act
        fs.SetVolumeLabel("NEW LABEL");

        // Assert - label should remain unchanged as CDOS doesn't support this
        Assert.Equal(originalLabel, fs.GetVolumeLabel());
    }

    private string CreateTempDiskImage(string fileName, DiskType diskType)
    {
        var path = Path.Combine(_tempDirectory, fileName);
        using var container = _diskContainerFactory.CreateNewDiskImage(path, diskType, "CDOS_TEST");
        _tempFiles.Add(path);
        return path;
    }

    private string CreateTempFile(string fileName, byte[] data)
    {
        var path = Path.Combine(_tempDirectory, fileName);
        _tempFiles.Add(path);
        File.WriteAllBytes(path, data);
        return path;
    }
}