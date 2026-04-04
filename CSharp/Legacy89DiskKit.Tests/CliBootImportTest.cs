using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Cli.Presentation.FileSystem;
using Xunit;
using System.Text.Json;

namespace Legacy89DiskKit.Tests;

public class CliBootImportTest : IDisposable
{
    private readonly string _tempDir;

    public CliBootImportTest()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Fact]
    public async Task BootImport_WithValidX1FileBacked_FailsWithoutStartRecord_ThenSucceedsWithExplicitStartRecord()
    {
        var imagePath = Path.Combine(_tempDir, "image.d88");
        var outputPath = Path.Combine(_tempDir, "payload.bin");
        var metaPath = Path.Combine(_tempDir, "meta.json");

        // Create empty hu-basic disk
        await CliCommandRunner.RunAsync("disk", "create", imagePath, "--disk-type", "2d", "--file-system", "hu-basic", "--language", "en");

        // Prepare boot payload and json metadata
        var payload = Enumerable.Repeat((byte)0xCC, 256).ToArray();
        await File.WriteAllBytesAsync(outputPath, payload);

        string fileName = "BASIC_CZ.Sys";
        var meta = new System.Text.Json.Nodes.JsonObject
        {
            ["machineFamily"] = "X1",
            ["mode"] = "FileBacked",
            ["displayName"] = fileName,
            ["suggestedBinaryFileName"] = "X1_Boot_BASIC_CZ.Sys.bin",
            ["payloadLength"] = 256,
            ["loadAddress"] = 0x1234,
            ["executionAddress"] = 0x5678
        };
        await File.WriteAllTextAsync(metaPath, meta.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var resultImportFail = await CliCommandRunner.RunAsync("boot", "import", imagePath, "--binary", outputPath, "--metadata", metaPath, "--language", "en");

        Assert.Contains("File-backed boot import requires an explicit start record", resultImportFail.StandardError);

        var resultImportSuccess = await CliCommandRunner.RunAsync(
            "boot", "import",
            imagePath,
            "--binary", outputPath,
            "--metadata", metaPath,
            "--start-record", "32",
            "--language", "en");
        Assert.True(resultImportSuccess.ExitCode == 0, $"Import failed. Out: {resultImportSuccess.StandardOutput} Err: {resultImportSuccess.StandardError}");
        Assert.Contains("Boot entry successfully imported", resultImportSuccess.StandardOutput);

        using (var diskService = new DiskService())
        {
            var container = diskService.OpenDisk(imagePath, true);
            var resolver = new ExplicitFileSystemResolver();
            var fs = resolver.Create("hu-basic", container);
            Assert.Empty(fs.GetFiles());

            var bootArea = fs.ReadBootArea();
            Assert.Equal(0x01, bootArea[0]); // Bootable flag

            var bootInfo = new Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.HuBasicBootRecordParser().Parse(bootArea);
            Assert.NotNull(bootInfo);
            Assert.Equal("BASIC_CZ", bootInfo!.Name);
            Assert.Equal("Sys", bootInfo.Extension);
            Assert.Equal(256, bootInfo.Size);
            Assert.Equal(0x1234, bootInfo.LoadAddress);
            Assert.Equal(0x5678, bootInfo.ExecutionAddress);
            Assert.Equal(32, bootInfo.StartRecord);
        }
    }
}
