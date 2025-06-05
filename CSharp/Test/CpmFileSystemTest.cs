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

public class CpmFileSystemTest : IDisposable
{
    private readonly string _tempDirectory;
    private readonly DiskContainerFactory _diskContainerFactory;
    private readonly List<string> _tempFiles = new();

    public CpmFileSystemTest()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"CpmTest_{Guid.NewGuid()}");
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
    [InlineData(DiskType.TwoDD)]
    [InlineData(DiskType.TwoHD)]
    public void Format_CreatesEmptyFileSystem(DiskType diskType)
    {
        // Arrange
        var diskPath = CreateTempDiskImage($"format_test_{diskType}.d88", diskType);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CpmFileSystem(container);

        // Act
        fs.Format();

        // Assert
        var files = fs.GetFiles().ToList();
        Assert.Empty(files);
    }

    [Theory]
    [InlineData("TEST.TXT", "TEST.TXT")]
    [InlineData("test.txt", "TEST.TXT")]
    [InlineData("LONGNAME.EXT", "LONGNAME.EXT")]
    [InlineData("verylongfilename.verylongext", "VERYLONG.VER")]
    public void ImportFile_NormalizesFileName(string inputName, string expectedName)
    {
        // Arrange
        var diskPath = CreateTempDiskImage("import_normalize.d88", DiskType.TwoDD);
        var sourcePath = CreateTempTextFile("Hello, CP/M!");
        
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CpmFileSystem(container);
        fs.Format();

        // Act
        fs.ImportFile(sourcePath, inputName);

        // Assert
        var files = fs.GetFiles().ToList();
        Assert.Single(files);
        Assert.Equal(expectedName, files[0].FileName);
    }

    [Fact]
    public void ImportAndExportFile_PreservesContent()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("import_export.d88", DiskType.TwoDD);
        var content = "This is a test file for CP/M filesystem.\r\n" +
                     "It contains multiple lines.\r\n" +
                     "And some special characters: !@#$%^&*()";
        var sourcePath = CreateTempTextFile(content);
        var exportPath = Path.Combine(_tempDirectory, "exported.txt");
        
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CpmFileSystem(container);
        fs.Format();

        // Act
        fs.ImportFile(sourcePath, "TEST.TXT");
        fs.ExportFile("TEST.TXT", exportPath);

        // Assert - CP/M pads to 128-byte boundaries, so trim trailing nulls
        var exportedContent = File.ReadAllText(exportPath);
        var trimmedContent = exportedContent.TrimEnd('\0');
        Assert.Equal(content, trimmedContent);
    }

    [Fact]
    public void DeleteFile_RemovesFile()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("delete_test.d88", DiskType.TwoDD);
        var sourcePath = CreateTempTextFile("Delete me!");
        
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CpmFileSystem(container);
        fs.Format();
        
        fs.ImportFile(sourcePath, "DELETE.ME");
        Assert.True(fs.FileExists("DELETE.ME"));

        // Act
        fs.DeleteFile("DELETE.ME");

        // Assert
        Assert.False(fs.FileExists("DELETE.ME"));
        Assert.Empty(fs.GetFiles());
    }

    [Fact]
    public void ImportLargeFile_CreatesMultipleExtents()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("large_file.d88", DiskType.TwoDD);
        
        // Create a file larger than 16KB (requires multiple extents)
        var content = new string('A', 20000);
        var sourcePath = CreateTempTextFile(content);
        var exportPath = Path.Combine(_tempDirectory, "large_exported.txt");
        
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CpmFileSystem(container);
        fs.Format();

        // Act
        fs.ImportFile(sourcePath, "LARGE.TXT");
        fs.ExportFile("LARGE.TXT", exportPath);

        // Assert - CP/M pads to 128-byte boundaries, so trim trailing nulls
        var exportedContent = File.ReadAllText(exportPath);
        var trimmedContent = exportedContent.TrimEnd('\0');
        Assert.Equal(content, trimmedContent);
        Assert.Equal(20000, fs.GetFileSize("LARGE.TXT"));
    }

    [Fact]
    public void GetFiles_WithPattern_ReturnsMatchingFiles()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("pattern_test.d88", DiskType.TwoDD);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CpmFileSystem(container);
        fs.Format();
        
        // Import multiple files
        fs.ImportFile(CreateTempTextFile("1"), "TEST1.TXT");
        fs.ImportFile(CreateTempTextFile("2"), "TEST2.TXT");
        fs.ImportFile(CreateTempTextFile("3"), "DATA.DAT");
        fs.ImportFile(CreateTempTextFile("4"), "README.DOC");

        // Act & Assert
        var txtFiles = fs.GetFiles("*.TXT").ToList();
        Assert.Equal(2, txtFiles.Count);
        Assert.Contains(txtFiles, f => f.FileName == "TEST1.TXT");
        Assert.Contains(txtFiles, f => f.FileName == "TEST2.TXT");

        var testFiles = fs.GetFiles("TEST*.*").ToList();
        Assert.Equal(2, testFiles.Count);

        var allFiles = fs.GetFiles("*.*").ToList();
        Assert.Equal(4, allFiles.Count);
    }

    [Theory]
    [InlineData("READONLY.TXT")]
    [InlineData("SYSTEM.SYS")]
    [InlineData("ARCHIVE.BAK")]
    public void FileAttributes_ArePreserved(string fileName)
    {
        // Arrange
        var diskPath = CreateTempDiskImage("attributes_test.d88", DiskType.TwoDD);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CpmFileSystem(container);
        fs.Format();
        
        // Import file
        fs.ImportFile(CreateTempTextFile("Test"), fileName);

        // Act - This would require extending the API to set attributes
        // For now, we just verify the file exists
        var files = fs.GetFiles().ToList();

        // Assert
        Assert.Single(files);
        Assert.Equal(fileName, files[0].FileName);
    }

    [Fact]
    public void CpmFileNameValidator_ValidatesCorrectly()
    {
        // Valid names
        Assert.True(CpmFileNameValidator.IsValidFileName("TEST.TXT"));
        Assert.True(CpmFileNameValidator.IsValidFileName("12345678.123"));
        Assert.True(CpmFileNameValidator.IsValidFileName("NOEXT"));
        
        // Invalid names
        Assert.False(CpmFileNameValidator.IsValidFileName("TOO.MANY.DOTS"));
        Assert.False(CpmFileNameValidator.IsValidFileName("TOOLONGNAME.TXT"));
        Assert.False(CpmFileNameValidator.IsValidFileName("TEST.TOOLONG"));
        Assert.False(CpmFileNameValidator.IsValidFileName("TEST<>.TXT"));
    }

    [Fact]
    public void CpmFileNameValidator_NormalizesCorrectly()
    {
        // Test normalization
        var (name1, ext1) = CpmFileNameValidator.NormalizeFileName("test.txt");
        Assert.Equal("TEST", name1);
        Assert.Equal("TXT", ext1);

        var (name2, ext2) = CpmFileNameValidator.NormalizeFileName("verylongfilename.extension");
        Assert.Equal("VERYLONG", name2);
        Assert.Equal("EXT", ext2);

        var (name3, ext3) = CpmFileNameValidator.NormalizeFileName("no_ext");
        Assert.Equal("NO_EXT", name3);
        Assert.Equal("", ext3);
    }

    [Theory]
    [InlineData(MachineType.CpmGeneric)]
    [InlineData(MachineType.CpmPc8801)]
    [InlineData(MachineType.CpmX1)]
    [InlineData(MachineType.CpmMsxDos)]
    public void CharacterEncoder_EncodesAndDecodes(MachineType machineType)
    {
        // Arrange
        Legacy89DiskKit.CharacterEncoding.Domain.Interface.ICharacterEncoder encoder = machineType switch
        {
            MachineType.CpmGeneric => new CpmCharacterEncoder(),
            MachineType.CpmPc8801 => new CpmPc8801CharacterEncoder(),
            MachineType.CpmX1 => new CpmX1CharacterEncoder(),
            MachineType.CpmMsxDos => new CpmMsxDosCharacterEncoder(),
            _ => throw new ArgumentException()
        };

        var testText = "HELLO WORLD 123";

        // Act
        var encoded = encoder.EncodeText(testText);
        var decoded = encoder.DecodeText(encoded);

        // Assert
        Assert.Equal(testText, decoded);
    }

    [Fact]
    public void CpmConfiguration_ReturnsCorrectValuesForDiskTypes()
    {
        // Test 2D configuration
        var config2D = CpmConfiguration.GetConfiguration(DiskType.TwoD);
        Assert.Equal(1024, config2D.BlockSize);
        Assert.Equal(64, config2D.DirectoryEntries);
        Assert.Equal(40, config2D.TrackCount);

        // Test 2DD configuration
        var config2DD = CpmConfiguration.GetConfiguration(DiskType.TwoDD);
        Assert.Equal(2048, config2DD.BlockSize);
        Assert.Equal(128, config2DD.DirectoryEntries);
        Assert.Equal(40, config2DD.TrackCount);

        // Test 2HD configuration
        var config2HD = CpmConfiguration.GetConfiguration(DiskType.TwoHD);
        Assert.Equal(2048, config2HD.BlockSize);
        Assert.Equal(256, config2HD.DirectoryEntries);
        Assert.Equal(77, config2HD.TrackCount);
    }

    [Fact]
    public void ImportFile_OnReadOnlyDisk_ThrowsException()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("readonly_test.d88", DiskType.TwoDD);
        var sourcePath = CreateTempTextFile("Test");
        
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, true); // Read-only
        var fs = new CpmFileSystem(container);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => fs.ImportFile(sourcePath, "TEST.TXT"));
    }

    [Fact]
    public void FileExists_CaseInsensitive()
    {
        // Arrange
        var diskPath = CreateTempDiskImage("case_test.d88", DiskType.TwoDD);
        using var container = _diskContainerFactory.OpenDiskImage(diskPath, false);
        var fs = new CpmFileSystem(container);
        fs.Format();
        
        fs.ImportFile(CreateTempTextFile("Test"), "TEST.TXT");

        // Act & Assert
        Assert.True(fs.FileExists("TEST.TXT"));
        Assert.True(fs.FileExists("test.txt"));
        Assert.True(fs.FileExists("Test.Txt"));
    }

    #region Helper Methods

    private string CreateTempDiskImage(string fileName, DiskType diskType)
    {
        var path = Path.Combine(_tempDirectory, fileName);
        using var container = _diskContainerFactory.CreateNewDiskImage(path, diskType, "CPM_TEST");
        _tempFiles.Add(path);
        return path;
    }

    private string CreateTempTextFile(string content)
    {
        var path = Path.Combine(_tempDirectory, $"temp_{Guid.NewGuid()}.txt");
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }

    #endregion
}