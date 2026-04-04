using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Cli.Presentation.FileSystem;
using Xunit;
using System.Text.Json;

namespace Legacy89DiskKit.Tests;

public class CliBootExportTest : IDisposable
{
    private readonly string _tempDir;

    public CliBootExportTest()
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
    public async Task BootExport_OnEmptyImage_FailsClearly()
    {
        var imagePath = Path.Combine(_tempDir, "empty.d88");
        var outputPath = Path.Combine(_tempDir, "output");

        // Create empty hu-basic disk
        var resultCreate = await CliCommandRunner.RunAsync("disk", "create", imagePath, "--disk-type", "2d", "--file-system", "hu-basic", "--language", "en");
        Assert.Equal(0, resultCreate.ExitCode);

        // Export boot entries
        var resultExport = await CliCommandRunner.RunAsync("boot", "export", imagePath, "--output", outputPath, "--language", "en");

        // The CLI handles custom errors via PrintError with an Error prefix or similar (for English: 'Error:')
        // Depending on PrintError implementation, exit code might be 0 but stdout/stderr contains the error message.
        // Wait, checking the message: "No exportable boot entries found on this disk."
        Assert.Contains("No exportable boot entries", resultExport.StandardError + resultExport.StandardOutput);
    }

    [Fact]
    public async Task BootExport_WithEntries_ExportsFilesAndJSON()
    {
        var imagePath = Path.Combine(_tempDir, "bootable.d88");
        var outputPath = Path.Combine(_tempDir, "output_export");

        // Create hu-basic disk
        var resultCreate = await CliCommandRunner.RunAsync("disk", "create", imagePath, "--disk-type", "2d", "--file-system", "hu-basic", "--language", "en");
        Assert.Equal(0, resultCreate.ExitCode);

        // Inject a dummy boot record
        using (var diskService = new DiskService())
        {
            var container = diskService.OpenDisk(imagePath, false);
            var resolver = new ExplicitFileSystemResolver();
            var fs = resolver.Create("hu-basic", container);
            
            var bootArea = new byte[256];
            bootArea[0] = 0x01; // file-backed flag
            System.Text.Encoding.ASCII.GetBytes("BASIC CZ8FB01".PadRight(13)).CopyTo(bootArea, 1);
            System.Text.Encoding.ASCII.GetBytes("Sys").CopyTo(bootArea, 0x0E);
            BitConverter.GetBytes((ushort)300).CopyTo(bootArea, 0x12);
            BitConverter.GetBytes((ushort)0x1234).CopyTo(bootArea, 0x14);
            BitConverter.GetBytes((ushort)0x5678).CopyTo(bootArea, 0x16);
            BitConverter.GetBytes((ushort)2).CopyTo(bootArea, 0x1E); // start_record = 2

            fs!.WriteBootArea(bootArea);

            var payload = new byte[300];
            payload[0] = 0x99;
            container.WriteSector(0, 0, 3, payload.Take(256).ToArray());
            container.WriteSector(0, 0, 4, payload.Skip(256).ToArray().Concat(new byte[256 - 44]).ToArray());

            container.Save();
        }

        // Export boot entries
        var resultExport = await CliCommandRunner.RunAsync("boot", "export", imagePath, "--output", outputPath, "--language", "en");

        Assert.Equal(0, resultExport.ExitCode);
        Assert.Contains("Boot entries exported", resultExport.StandardOutput);

        var binFile = Path.Combine(outputPath, "X1_BootRecord_BASIC_CZ8FB01.Sys.bin");
        var jsonFile = Path.Combine(outputPath, "X1_BootRecord_BASIC_CZ8FB01.Sys.json");

        Assert.True(File.Exists(binFile));
        Assert.True(File.Exists(jsonFile));

        var binData = File.ReadAllBytes(binFile);
        Assert.Equal(300, binData.Length);
        Assert.Equal(0x99, binData[0]);

        var jsonText = File.ReadAllText(jsonFile);
        Assert.Contains("\"machineFamily\": \"X1\"", jsonText);
        Assert.Contains("\"mode\": \"FileBacked\"", jsonText);
        Assert.Contains("\"payloadLength\": 300", jsonText);
    }
}
