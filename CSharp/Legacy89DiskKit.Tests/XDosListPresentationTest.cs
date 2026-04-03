using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.Cli.Presentation.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;
using Xunit;
using DomainFileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Tests;

public class XDosListPresentationTest
{
    private static readonly IFileListLocalizer Localizer = FileListLocalizer.Create("en");

    [Fact]
    public void XDosFileListFormatter_FormatsTypeAddressesAndAttributes()
    {
        var formatter = new XDosFileListFormatter();
        var entry = new FileEntry(
            "X-DOS System",
            string.Empty,
            10240,
            null,
            new DateTime(2026, 4, 3, 22, 50, 0, DateTimeKind.Utc),
            new ExtendedFileAttributes(DomainFileAttributes.None, 0x80, false, "X-DOS"),
            2,
            0x8000,
            0xA7FF,
            0x8100,
            null,
            null,
            new XDosFileMetadata(XDosFileType.Sys, (ushort)XDosFileType.Sys, 0x80, 0x260403)
        );

        var view = formatter.Format(new FileListFormatContext(
            new DiskFileSystemInfo("X-DOS", 399360, 45056, 1024, 74, "X1", "X1"),
            new[]
            {
                new FileListEntryContext(entry, "X-DOS System", "X-DOS System", string.Empty, 10240)
            }), Localizer);

        Assert.Equal(
            new[]
            {
                Localizer.NameHeader,
                Localizer.TypeHeader,
                Localizer.AttrHeader,
                Localizer.SizeHeader,
                Localizer.LoadHeader,
                Localizer.ExecHeader,
                Localizer.ClusterHeader
            },
            view.Columns.Select(column => column.Header).ToArray());

        Assert.Single(view.Rows);
        Assert.Equal(
            new[] { "X-DOS System", "SYS", "H---:0", "10240", "8000", "8100", "2" },
            view.Rows[0].Values.ToArray());
        Assert.Contains(view.Legends, item => item.Key == "H" && item.Description == Localizer.XDosFlagSecret);
        Assert.Contains(view.Legends, item => item.Key == "W" && item.Description == Localizer.XDosFlagWriteProtect);
        Assert.Contains(view.Legends, item => item.Key == "S" && item.Description == Localizer.XDosFlagSystem);
        Assert.Contains(view.Legends, item => item.Key == "K" && item.Description == Localizer.XDosFlagKanji);
    }

    [Fact]
    public async Task CliList_XDosDisk_UsesExtendedFormatter()
    {
        var path = TestDiskFixtureFactory.CreateFormattedXDosDisk("xdos-list-view.d88", fileCount: 1);
        var result = await CliCommandRunner.RunAsync(["list", path, "--language", "en"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Name", result.StandardOutput);
        Assert.Contains("Type", result.StandardOutput);
        Assert.Contains("Load", result.StandardOutput);
        Assert.Contains("Exec", result.StandardOutput);
        Assert.Contains("FILE00.BIN | BIN", result.StandardOutput);
        Assert.Contains("----:0", result.StandardOutput);
        Assert.Contains("Legends:", result.StandardOutput);
        Assert.Contains("8000", result.StandardOutput);
    }
}
