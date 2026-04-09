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
    public async Task FileImport_XDos_PreservesPeriodInUnifiedName()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var imagePath = Path.Combine(tempDirectory, "xdos.d88");
        var sourcePath = Path.Combine(tempDirectory, "mml.txt");
        await File.WriteAllTextAsync(sourcePath, "MML");

        try
        {
            var createResult = await CliCommandRunner.RunAsync(
                "disk", "create", imagePath,
                "--disk-type", "2d",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, createResult.ExitCode);

            var importResult = await CliCommandRunner.RunAsync(
                "file", "import", imagePath, sourcePath,
                "--target-name", "MML.DOC",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, importResult.ExitCode);

            var listResult = await CliCommandRunner.RunAsync(
                "list", imagePath,
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, listResult.ExitCode);
            Assert.Contains("MML.DOC", listResult.StandardOutput);
            Assert.DoesNotContain("MML_DOC", listResult.StandardOutput);
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
    public async Task FileCrossCopy_XDos_PreservesPeriodInUnifiedName()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var sourceImagePath = Path.Combine(tempDirectory, "xdos-src.d88");
        var destImagePath = Path.Combine(tempDirectory, "xdos-dest.d88");
        var sourcePath = Path.Combine(tempDirectory, "mml.txt");
        await File.WriteAllTextAsync(sourcePath, "MML");

        try
        {
            var createSourceResult = await CliCommandRunner.RunAsync(
                "disk", "create", sourceImagePath,
                "--disk-type", "2d",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, createSourceResult.ExitCode);

            var createDestResult = await CliCommandRunner.RunAsync(
                "disk", "create", destImagePath,
                "--disk-type", "2d",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, createDestResult.ExitCode);

            var importResult = await CliCommandRunner.RunAsync(
                "file", "import", sourceImagePath, sourcePath,
                "--target-name", "MML.DOC",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, importResult.ExitCode);

            var crossCopyResult = await CliCommandRunner.RunAsync(
                "file", "cross-copy", sourceImagePath, destImagePath, "MML.DOC",
                "--language", "en");
            Assert.Equal(0, crossCopyResult.ExitCode);

            var listResult = await CliCommandRunner.RunAsync(
                "list", destImagePath,
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, listResult.ExitCode);
            Assert.Contains("MML.DOC", listResult.StandardOutput);
            Assert.DoesNotContain("MML_DOC", listResult.StandardOutput);
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

    [Fact]
    public async Task FileInject_Overwrite_ReplacesExistingFile()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var imagePath = Path.Combine(tempDirectory, "overwrite.d88");
        var firstPath = Path.Combine(tempDirectory, "first.txt");
        var secondPath = Path.Combine(tempDirectory, "second.txt");
        var exportPath = Path.Combine(tempDirectory, "out.txt");
        await File.WriteAllTextAsync(firstPath, "FIRST");
        await File.WriteAllTextAsync(secondPath, "SECOND");

        try
        {
            var createResult = await CliCommandRunner.RunAsync(
                "disk", "create", imagePath,
                "--disk-type", "2d",
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, createResult.ExitCode);

            var firstInjectResult = await CliCommandRunner.RunAsync(
                "file", "import", imagePath, firstPath,
                "--target-name", "TEST.TXT",
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, firstInjectResult.ExitCode);

            var overwriteInjectResult = await CliCommandRunner.RunAsync(
                "file", "import", imagePath, secondPath,
                "--target-name", "TEST.TXT",
                "--file-system", "hu-basic",
                "--image-file-overwrite",
                "--language", "en");
            Assert.Equal(0, overwriteInjectResult.ExitCode);

            var exportResult = await CliCommandRunner.RunAsync(
                "file", "export", imagePath, "TEST.TXT", exportPath,
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, exportResult.ExitCode);
            Assert.Equal("SECOND", await File.ReadAllTextAsync(exportPath));
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
    public async Task FileInject_NoOverwrite_GeneratesAlias()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var imagePath = Path.Combine(tempDirectory, "alias.d88");
        var firstPath = Path.Combine(tempDirectory, "first.txt");
        var secondPath = Path.Combine(tempDirectory, "second.txt");
        await File.WriteAllTextAsync(firstPath, "FIRST");
        await File.WriteAllTextAsync(secondPath, "SECOND");

        try
        {
            var createResult = await CliCommandRunner.RunAsync(
                "disk", "create", imagePath,
                "--disk-type", "2d",
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, createResult.ExitCode);

            var firstInjectResult = await CliCommandRunner.RunAsync(
                "file", "import", imagePath, firstPath,
                "--target-name", "TEST.TXT",
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, firstInjectResult.ExitCode);

            var secondInjectResult = await CliCommandRunner.RunAsync(
                "file", "import", imagePath, secondPath,
                "--target-name", "TEST.TXT",
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, secondInjectResult.ExitCode);

            var listResult = await CliCommandRunner.RunAsync(
                "list", imagePath,
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, listResult.ExitCode);
            Assert.Contains("TEST", listResult.StandardOutput);
            Assert.Contains("TEST001", listResult.StandardOutput);
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
    public async Task FileInject_OverwriteOnXDos_ReplacesExistingFile()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var imagePath = Path.Combine(tempDirectory, "xdos-overwrite.d88");
        var firstPath = Path.Combine(tempDirectory, "first.txt");
        var secondPath = Path.Combine(tempDirectory, "second.txt");
        await File.WriteAllTextAsync(firstPath, "FIRST");
        await File.WriteAllTextAsync(secondPath, "SECOND");

        try
        {
            var createResult = await CliCommandRunner.RunAsync(
                "disk", "create", imagePath,
                "--disk-type", "2d",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, createResult.ExitCode);

            var firstInjectResult = await CliCommandRunner.RunAsync(
                "file", "import", imagePath, firstPath,
                "--target-name", "TEST",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, firstInjectResult.ExitCode);

            var overwriteInjectResult = await CliCommandRunner.RunAsync(
                "file", "import", imagePath, secondPath,
                "--target-name", "TEST",
                "--file-system", "xdos",
                "--image-file-overwrite",
                "--language", "en");
            Assert.Equal(0, overwriteInjectResult.ExitCode);
            Assert.DoesNotContain("filesystem constraints", overwriteInjectResult.StandardOutput + overwriteInjectResult.StandardError);

            var listResult = await CliCommandRunner.RunAsync(
                "list", imagePath,
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, listResult.ExitCode);
            Assert.Contains("TEST", listResult.StandardOutput);
            Assert.DoesNotContain("TES001", listResult.StandardOutput);
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
    public async Task FileCrossCopy_Overwrite_XDos_ReplacesExistingFile()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var sourceImagePath = Path.Combine(tempDirectory, "xdos-src.d88");
        var destImagePath = Path.Combine(tempDirectory, "xdos-dest.d88");
        var sourcePath = Path.Combine(tempDirectory, "source.txt");
        var destSeedPath = Path.Combine(tempDirectory, "dest-seed.txt");
        await File.WriteAllTextAsync(sourcePath, "SOURCE");
        await File.WriteAllTextAsync(destSeedPath, "DEST");

        try
        {
            var createSourceResult = await CliCommandRunner.RunAsync(
                "disk", "create", sourceImagePath,
                "--disk-type", "2d",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, createSourceResult.ExitCode);

            var createDestResult = await CliCommandRunner.RunAsync(
                "disk", "create", destImagePath,
                "--disk-type", "2d",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, createDestResult.ExitCode);

            var sourceInjectResult = await CliCommandRunner.RunAsync(
                "file", "import", sourceImagePath, sourcePath,
                "--target-name", "MML.DOC",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, sourceInjectResult.ExitCode);

            var destInjectResult = await CliCommandRunner.RunAsync(
                "file", "import", destImagePath, destSeedPath,
                "--target-name", "MML.DOC",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, destInjectResult.ExitCode);

            var overwriteCrossCopyResult = await CliCommandRunner.RunAsync(
                "file", "cross-copy", sourceImagePath, destImagePath, "MML.DOC",
                "--image-file-overwrite",
                "--language", "en");
            Assert.Equal(0, overwriteCrossCopyResult.ExitCode);

            var listResult = await CliCommandRunner.RunAsync(
                "list", destImagePath,
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, listResult.ExitCode);
            Assert.Contains("MML.DOC", listResult.StandardOutput);
            Assert.DoesNotContain("MML.DOC001", listResult.StandardOutput);
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
    public async Task FileCopy_Overwrite_DeletesExistingTarget()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var imagePath = Path.Combine(tempDirectory, "copy-overwrite.d88");
        var sourcePath = Path.Combine(tempDirectory, "source.txt");
        await File.WriteAllTextAsync(sourcePath, "SOURCE");

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
                "--target-name", "ORIGINAL.TXT",
                "--file-system", "hu-basic",
                "--language", "en");
            Assert.Equal(0, importResult.ExitCode);

            var copyResult = await CliCommandRunner.RunAsync(
                "file", "copy", imagePath, "ORIGINAL.TXT", "TARGET.TXT",
                "--language", "en");
            Assert.Equal(0, copyResult.ExitCode);

            var overwriteCopyResult = await CliCommandRunner.RunAsync(
                "file", "copy", imagePath, "ORIGINAL.TXT", "TARGET.TXT",
                "--image-file-overwrite",
                "--language", "en");
            Assert.Equal(0, overwriteCopyResult.ExitCode);
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
    public async Task FileCopy_Overwrite_XDos_ReplacesExistingFile()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var imagePath = Path.Combine(tempDirectory, "copy-overwrite-xdos.d88");
        var sourcePath = Path.Combine(tempDirectory, "source.txt");
        var targetSeedPath = Path.Combine(tempDirectory, "target-seed.txt");
        var exportPath = Path.Combine(tempDirectory, "out.txt");
        await File.WriteAllTextAsync(sourcePath, "SOURCE");
        await File.WriteAllTextAsync(targetSeedPath, "TARGET");

        try
        {
            var createResult = await CliCommandRunner.RunAsync(
                "disk", "create", imagePath,
                "--disk-type", "2d",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, createResult.ExitCode);

            var sourceImportResult = await CliCommandRunner.RunAsync(
                "file", "import", imagePath, sourcePath,
                "--target-name", "SOURCE.TXT",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, sourceImportResult.ExitCode);

            var seedImportResult = await CliCommandRunner.RunAsync(
                "file", "import", imagePath, targetSeedPath,
                "--target-name", "TARGET.TXT",
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, seedImportResult.ExitCode);

            var overwriteCopyResult = await CliCommandRunner.RunAsync(
                "file", "copy", imagePath, "SOURCE.TXT", "TARGET.TXT",
                "--image-file-overwrite",
                "--language", "en");
            Assert.Equal(0, overwriteCopyResult.ExitCode);

            var exportResult = await CliCommandRunner.RunAsync(
                "file", "export", imagePath, "TARGET.TXT", exportPath,
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, exportResult.ExitCode);
            Assert.Equal("SOURCE", await File.ReadAllTextAsync(exportPath));

            var listResult = await CliCommandRunner.RunAsync(
                "list", imagePath,
                "--file-system", "xdos",
                "--language", "en");
            Assert.Equal(0, listResult.ExitCode);
            Assert.Contains("TARGET.TXT", listResult.StandardOutput);
            Assert.DoesNotContain("TARGET.TXT001", listResult.StandardOutput);
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
