using Xunit;

namespace Legacy89DiskKit.Tests;

public sealed class CliFileInspectorAndCsvTest
{
    [Fact]
    public async Task CliList_Csv_PrintsCsvHeader()
    {
        var path = TestDiskFixtureFactory.CreateFormattedXDosDisk("xdos-list-csv.d88", fileCount: 1);

        var result = await CliCommandRunner.RunAsync("list", path, "--language", "en", "--output-format", "csv");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Name,Type,Attr,Size,Load,Exec,DIR-ADR,BDY-ADR", result.StandardOutput);
        Assert.Contains("FILE00.BIN", result.StandardOutput);
    }

    [Fact]
    public async Task DiskInspector_Csv_PrintsSectionRows()
    {
        var path = TestDiskFixtureFactory.CreateFormattedHuBasicDisk("hubasic-inspector-csv.d88", writeSampleFile: true);

        var result = await CliCommandRunner.RunAsync("disk", "inspector", path, "--detail", "normal", "--file-system", "hu-basic", "--language", "en", "--output-format", "csv");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Section,Key,Value", result.StandardOutput);
        Assert.Contains("Disk,File System,Hu-BASIC", result.StandardOutput);
    }

    [Fact]
    public async Task FileInspector_HuBasicFull_PrintsDirectoryDumpAndOffsets()
    {
        var path = TestDiskFixtureFactory.CreateFormattedHuBasicDisk("hubasic-file-inspector.d88", writeSampleFile: true);

        var result = await CliCommandRunner.RunAsync("file", "inspector", path, "HELLO.BAS", "--detail", "full", "--file-system", "hu-basic", "--language", "en");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("[File]", result.StandardOutput);
        Assert.Contains("[Addresses]", result.StandardOutput);
        Assert.Contains("DIR-ADR:", result.StandardOutput);
        Assert.Contains("BDY-ADR:", result.StandardOutput);
        Assert.Contains("Directory Entry Hex:", result.StandardOutput);
        Assert.Contains("Clusters:", result.StandardOutput);
    }

    [Fact]
    public async Task FileInspector_XDosCsv_PrintsFamAndRawType()
    {
        var path = TestDiskFixtureFactory.CreateFormattedXDosDisk("xdos-file-inspector.d88", fileCount: 1);

        var result = await CliCommandRunner.RunAsync("file", "inspector", path, "FILE00.BIN", "--detail", "full", "--language", "en", "--output-format", "csv");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Section,Key,Value", result.StandardOutput);
        Assert.Contains("X-DOS,Raw Type,0100", result.StandardOutput);
        Assert.Contains("Chain,FAM,", result.StandardOutput);
    }
}
