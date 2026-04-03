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
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, importResult.ExitCode);

            var exportResult = await CliCommandRunner.RunAsync(
                "file", "export", imagePath, "HELLO.TXT", exportPath,
                "--file-system", "hu-basic",
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
    public async Task FileImportExport_TextTabs_CanKeepOrExpandTabs()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var keepImagePath = Path.Combine(tempDirectory, "keep.d88");
        var spacesImagePath = Path.Combine(tempDirectory, "spaces.d88");
        var sourcePath = Path.Combine(tempDirectory, "tabs.txt");
        var keepExportPath = Path.Combine(tempDirectory, "keep-out.txt");
        var spacesExportPath = Path.Combine(tempDirectory, "spaces-out.txt");
        await File.WriteAllTextAsync(sourcePath, "A\tB");

        try
        {
            var createKeepResult = await CliCommandRunner.RunAsync(
                "disk", "create", keepImagePath,
                "--disk-type", "2d",
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, createKeepResult.ExitCode);

            var createSpacesResult = await CliCommandRunner.RunAsync(
                "disk", "create", spacesImagePath,
                "--disk-type", "2d",
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, createSpacesResult.ExitCode);

            var importKeepResult = await CliCommandRunner.RunAsync(
                "file", "import", keepImagePath, sourcePath,
                "--target-name", "KEEP.TXT",
                "--file-system", "hu-basic",
                "--tab-mode", "keep",
                "--language", "en");
            Assert.Equal(0, importKeepResult.ExitCode);

            var exportKeepResult = await CliCommandRunner.RunAsync(
                "file", "export", keepImagePath, "KEEP.TXT", keepExportPath,
                "--file-system", "hu-basic",
                "--tab-mode", "keep",
                "--language", "en");
            Assert.Equal(0, exportKeepResult.ExitCode);
            Assert.Equal("A\tB", await File.ReadAllTextAsync(keepExportPath));

            var importSpacesResult = await CliCommandRunner.RunAsync(
                "file", "import", spacesImagePath, sourcePath,
                "--target-name", "SPACES.TXT",
                "--file-system", "hu-basic",
                "--tab-mode", "spaces",
                "--tab-width", "4",
                "--language", "en");
            Assert.Equal(0, importSpacesResult.ExitCode);

            var exportSpacesResult = await CliCommandRunner.RunAsync(
                "file", "export", spacesImagePath, "SPACES.TXT", spacesExportPath,
                "--file-system", "hu-basic",
                "--tab-mode", "keep",
                "--language", "en");
            Assert.Equal(0, exportSpacesResult.ExitCode);
            Assert.Equal("A   B", await File.ReadAllTextAsync(spacesExportPath));
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
    public async Task FileImport_HuBasicTextOverflow_FailsOrTruncatesByOption()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var imagePath = Path.Combine(tempDirectory, "overflow.d88");
        var truncatedImagePath = Path.Combine(tempDirectory, "overflow-truncated.d88");
        var sourcePath = Path.Combine(tempDirectory, "overflow.txt");
        var exportPath = Path.Combine(tempDirectory, "overflow-out.txt");
        await File.WriteAllTextAsync(sourcePath, new string('\t', 9000));

        try
        {
            var createResult = await CliCommandRunner.RunAsync(
                "disk", "create", imagePath,
                "--disk-type", "2d",
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, createResult.ExitCode);

            var createTruncatedResult = await CliCommandRunner.RunAsync(
                "disk", "create", truncatedImagePath,
                "--disk-type", "2d",
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, createTruncatedResult.ExitCode);

            var importFailResult = await CliCommandRunner.RunAsync(
                "file", "import", imagePath, sourcePath,
                "--target-name", "OVER.TXT",
                "--file-system", "hu-basic",
                "--tab-mode", "spaces",
                "--tab-width", "8",
                "--language", "en");
            Assert.Contains("65535-byte limit", importFailResult.StandardError + importFailResult.StandardOutput);

            var importTruncateResult = await CliCommandRunner.RunAsync(
                "file", "import", truncatedImagePath, sourcePath,
                "--target-name", "OVER.TXT",
                "--file-system", "hu-basic",
                "--tab-mode", "spaces",
                "--tab-width", "8",
                "--truncate-text-on-overflow",
                "--language", "en");
            Assert.Equal(0, importTruncateResult.ExitCode);

            var exportResult = await CliCommandRunner.RunAsync(
                "file", "export", truncatedImagePath, "OVER.TXT", exportPath,
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, exportResult.ExitCode);
            Assert.Equal(65534, (await File.ReadAllTextAsync(exportPath)).Length);
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
