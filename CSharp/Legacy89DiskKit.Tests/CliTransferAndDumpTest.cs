using Xunit;

namespace Legacy89DiskKit.Tests;

public sealed class CliTransferAndDumpTest
{
    [Fact]
    public async Task FileImportExport_Aliases_RoundTripPayload()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var imagePath = Path.Combine(tempDirectory, "sample.d88");
        var sourcePath = Path.Combine(tempDirectory, "hello.txt");
        var exportPath = Path.Combine(tempDirectory, "hello-out.txt");
        await File.WriteAllTextAsync(sourcePath, "HELLO");

        try
        {
            var createResult = await CliCommandRunner.RunAsync(
                "disk", "create", imagePath,
                "--disk-type", "2d",
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, createResult.ExitCode);

            var importResult = await CliCommandRunner.RunAsync(
                "file", "import", imagePath, sourcePath,
                "--target-name", "HELLO.TXT",
                "--language", "en");
            Assert.Equal(0, importResult.ExitCode);

            var exportResult = await CliCommandRunner.RunAsync(
                "file", "export", imagePath, "HELLO.TXT", exportPath,
                "--language", "en");
            Assert.Equal(0, exportResult.ExitCode);
            Assert.Equal("HELLO", await File.ReadAllTextAsync(exportPath));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task SectorExportImport_And_Dump_Work()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var imagePath = Path.Combine(tempDirectory, "sample.d88");
        var sourcePath = Path.Combine(tempDirectory, "sector.bin");
        var exportPath = Path.Combine(tempDirectory, "sector-out.bin");
        await File.WriteAllBytesAsync(sourcePath, Enumerable.Repeat((byte)0x41, 256).ToArray());

        try
        {
            var createResult = await CliCommandRunner.RunAsync(
                "disk", "create", imagePath,
                "--disk-type", "2d",
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, createResult.ExitCode);

            var importResult = await CliCommandRunner.RunAsync(
                "disk", "sector", "import", imagePath, "0", sourcePath,
                "--count", "1",
                "--language", "en");
            Assert.Equal(0, importResult.ExitCode);

            var exportResult = await CliCommandRunner.RunAsync(
                "disk", "sector", "export", imagePath, "0", "1", exportPath,
                "--language", "en");
            Assert.Equal(0, exportResult.ExitCode);
            Assert.Equal(await File.ReadAllBytesAsync(sourcePath), await File.ReadAllBytesAsync(exportPath));

            var dumpResult = await CliCommandRunner.RunAsync(
                "disk", "dump", imagePath, "cylinder0,side0,sector1", "8",
                "--language", "en");
            Assert.Equal(0, dumpResult.ExitCode);
            Assert.Contains("00000000", dumpResult.StandardOutput);
            Assert.Contains("41 41 41 41", dumpResult.StandardOutput);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }
}
