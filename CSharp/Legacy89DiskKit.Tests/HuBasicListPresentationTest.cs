using Legacy89DiskKit.Cli.Presentation.FileSystem;
using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;
using Xunit;
using DomainFileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.Tests;
public class HuBasicListPresentationTest
{
    private static readonly IFileListLocalizer Localizer = FileListLocalizer.CreateCurrent();
    [Fact]
    public void HuBasicDirParser_ParsesMetadata()
    {
        var parser = new HuBasicDirParser(HuBasicConfiguration.GetDefault(DiskType.TwoD));
        var entryData = CreateDirectoryEntry(0x71, "PROGRAM", "BIN", 0x41, 0x0020, 0x1200, 0x1300, 5);
        var entry = parser.Parse(entryData);
        var metadata = Assert.IsType<HuBasicFileMetadata>(entry.FileSystemMetadata);
        Assert.Equal(HuBasicFileType.Binary, metadata.FileType);
        Assert.True(metadata.HasPassword);
        Assert.True(metadata.IsHidden);
        Assert.True(metadata.IsVerify);
        Assert.True(metadata.IsWriteProtected);
        Assert.False(metadata.IsDirectory);
        Assert.Equal((ushort)0x0020, metadata.RecordedSize);
        Assert.Equal((ushort)0x1200, metadata.LoadAddress);
        Assert.Equal((ushort)0x1300, metadata.ExecutionAddress);
        Assert.Equal(5, metadata.StartCluster);
    }

    [Fact]
    public void HuBasicDirParser_PrioritizesBinaryOverBasicAndAscii()
    {
        var parser = new HuBasicDirParser(HuBasicConfiguration.GetDefault(DiskType.TwoD));
        var entryData = CreateDirectoryEntry(0x07, "MIXED", "DAT", 0x20, 0x0010, 0x1000, 0x1000, 3);
        var entry = parser.Parse(entryData);
        var metadata = Assert.IsType<HuBasicFileMetadata>(entry.FileSystemMetadata);
        Assert.Equal(HuBasicFileType.Binary, metadata.FileType);
    }

    [Fact]
    public void HuBasicFileListFormatter_FormatsFlagsAndAddresses()
    {
        var formatter = new HuBasicFileListFormatter();
        var entry = new FileEntry("PROGRAM", "BIN", 32, null, DateTime.UtcNow, new ExtendedFileAttributes(DomainFileAttributes.None, 0x71, false, "Hu-BASIC"), 5, 0x1200, 0x121F, 0x1300, null, null, new HuBasicFileMetadata(HuBasicFileType.Binary, true, true, true, true, false, 32, 0x1200, 0x1300, 5, 0x71, 0x41));
        var view = formatter.Format(new FileListFormatContext(new DiskFileSystemInfo("Hu-BASIC", 320 * 1024, 64 * 1024, 4096, 32, "X1", "X1"), new[] { new FileListEntryContext(entry, "PROGRAM.BIN", "PROGRAM", "BIN", 32) }), Localizer);
        Assert.Equal(new[] { Localizer.NameHeader, Localizer.TypeHeader, Localizer.FlagsHeader, Localizer.SizeHeader, Localizer.LoadHeader, Localizer.EndHeader, Localizer.ExecHeader, Localizer.DirectoryAddressHeader, Localizer.BodyAddressHeader, Localizer.NoteHeader }, view.Columns.Select(column => column.Header).ToArray());
        Assert.Single(view.Rows);
        Assert.Equal(new[] { "PROGRAM      .BIN", "BIN", "PHVW", "32", "1200", "121F", "1300", "--------", "--------", "" }, view.Rows[0].Values.ToArray());
        Assert.NotEmpty(view.Summary);
        Assert.NotEmpty(view.Legends);
    }

    [Fact]
    public void HuBasicFileListFormatter_DoesNotTreatSysExtensionAsSystemAttribute()
    {
        var formatter = new HuBasicFileListFormatter();
        var entry = new FileEntry("STARTUP", "Sys", 64, null, DateTime.UtcNow, new ExtendedFileAttributes(DomainFileAttributes.None, 0x01, false, "Hu-BASIC"), 8, 0x2000, 0x203F, 0x2000, null, null, new HuBasicFileMetadata(HuBasicFileType.Binary, false, false, false, false, false, 64, 0x2000, 0x2000, 8, 0x01));
        var view = formatter.Format(new FileListFormatContext(new DiskFileSystemInfo("Hu-BASIC", 320 * 1024, 64 * 1024, 4096, 32, "X1", "X1"), new[] { new FileListEntryContext(entry, "STARTUP.Sys", "STARTUP", "Sys", 64) }), Localizer);
        Assert.Equal("BIN", view.Rows[0].Values[1]);
        Assert.Equal("----", view.Rows[0].Values[2]);
    }

    [Fact]
    public void HuBasicFileListFormatter_PadsFullWidthNamesByDisplayWidth()
    {
        var formatter = new HuBasicFileListFormatter();
        var entry = new FileEntry("音訓     変換", "DIC", 212, null, DateTime.UtcNow, new ExtendedFileAttributes(DomainFileAttributes.None, 0x44, true, "Hu-BASIC"), 34, 0xF000, 0xF300, 0x0000, null, null, new HuBasicFileMetadata(HuBasicFileType.Ascii, false, false, false, true, false, 769, 0xF000, 0x0000, 34, 0x44));
        var view = formatter.Format(new FileListFormatContext(new DiskFileSystemInfo("Hu-BASIC", 320 * 1024, 64 * 1024, 4096, 32, "X1", "X1"), new[] { new FileListEntryContext(entry, "音訓     変換.DIC", "音訓     変換", "DIC", 212) }), Localizer);
        Assert.Equal(17, DisplayWidthUtility.GetWidth(view.Rows[0].Values[0]));
        Assert.Equal("音訓     変換.DIC", view.Rows[0].Values[0]);
    }

    [Fact]
    public void HuBasicFileListFormatter_AssignsDynamicFootnotes()
    {
        var formatter = new HuBasicFileListFormatter();
        var asciiEntry = new FileEntry("README", "DOC", 12288, null, DateTime.UtcNow, new ExtendedFileAttributes(DomainFileAttributes.None, 0x04, true, "Hu-BASIC"), 11, 0x0000, 0x2FFF, 0x0000, null, null, new HuBasicFileMetadata(HuBasicFileType.Ascii, false, false, false, false, false, 12288, 0x0000, 0x0000, 11, 0x04));
        var basicEntry = new FileEntry("HELLO", "BAS", 135, null, DateTime.UtcNow, new ExtendedFileAttributes(DomainFileAttributes.None, 0x02, false, "Hu-BASIC"), 42, 0x0000, 0x0086, 0x0000, null, null, new HuBasicFileMetadata(HuBasicFileType.Basic, false, false, false, false, false, 135, 0x0000, 0x0000, 42, 0x02));
        var view = formatter.Format(new FileListFormatContext(new DiskFileSystemInfo("Hu-BASIC", 320 * 1024, 64 * 1024, 4096, 32, "X1", "X1"), new[] { new FileListEntryContext(asciiEntry, "README.DOC", "README", "DOC", 12288), new FileListEntryContext(basicEntry, "HELLO.BAS", "HELLO", "BAS", 135) }), Localizer);
        Assert.Equal("*1", view.Rows[0].Values[9]);
        Assert.Equal("*2", view.Rows[1].Values[9]);
        Assert.Equal(2, view.Footnotes.Count);
    }

    [Fact]
    public void HuBasicFileListFormatter_AddsLabelEntryFootnote()
    {
        var formatter = new HuBasicFileListFormatter();
        var entry = new FileEntry("-------------", "---", 0, null, DateTime.UtcNow, new ExtendedFileAttributes(DomainFileAttributes.ReadOnly, 0x44, true, "Hu-BASIC"), 0x7FFF, 0xFFFF, 0xFFFF, 0xFFFF, null, null, new HuBasicFileMetadata(HuBasicFileType.Ascii, true, false, false, true, false, 0, 0xFFFF, 0xFFFF, 0x7FFF, 0x44, 0x01));
        var view = formatter.Format(new FileListFormatContext(new DiskFileSystemInfo("Hu-BASIC", 320 * 1024, 64 * 1024, 4096, 32, "X1", "X1"), new[] { new FileListEntryContext(entry, "------------- .---", "-------------", "---", 0) }), Localizer);
        Assert.Equal("*1", view.Rows[0].Values[9]);
        Assert.Single(view.Footnotes);
        Assert.Contains(view.Footnotes, item => item.Text == Localizer.HuBasicLabelEntryNote);
    }

    [Fact]
    public void HuBasicBootRecordParser_ParsesBootRecordSeparately()
    {
        var parser = new HuBasicBootRecordParser();
        var bootArea = CreateDirectoryEntry(0x01, "IPLBOOT", "Sys", 0x42, 0x0400, 0x8000, 0x8100, 0);
        bootArea[0x00] = 0x01;
        bootArea[0x1E] = 48;
        bootArea[0x1F] = 0;
        var record = parser.Parse(bootArea);
        Assert.NotNull(record);
        Assert.Equal((byte)0x01, record!.BootFlag);
        Assert.Equal("IPLBOOT", record.Name);
        Assert.Equal("Sys", record.Extension);
        Assert.True(record.HasPassword);
        Assert.Equal((ushort)0x0400, record.Size);
        Assert.Equal((ushort)0x8000, record.LoadAddress);
        Assert.Equal((ushort)0x8100, record.ExecutionAddress);
        Assert.Equal((ushort)48, record.StartRecord);
    }

    [Fact]
    public void HuBasicFileListFormatter_AddsBootSummary()
    {
        var formatter = new HuBasicFileListFormatter();
        var entry = new FileEntry("STARTUP", "Sys", 64, null, DateTime.UtcNow, new ExtendedFileAttributes(DomainFileAttributes.None, 0x01, false, "Hu-BASIC"), 8, 0x2000, 0x203F, 0x2000, null, null, new HuBasicFileMetadata(HuBasicFileType.Binary, false, false, false, false, false, 64, 0x2000, 0x2000, 8, 0x01));
        var view = formatter.Format(new FileListFormatContext(new DiskFileSystemInfo("Hu-BASIC", 320 * 1024, 64 * 1024, 4096, 32, "X1", "X1"), new[] { new FileListEntryContext(entry, "STARTUP.Sys", "STARTUP", "Sys", 64) }, null, new BootInfoSummary(BootInfoMode.FileBacked, "STARTUP.Sys", 0x2000, 0x2000)), Localizer);
        Assert.Contains(view.Summary, item => item.Label == Localizer.BootLabel && item.Value == Localizer.BootModeFileBacked);
        Assert.Contains(view.Summary, item => item.Label == Localizer.BootFileLabel && item.Value == "STARTUP.Sys");
    }

    [Fact]
    public void FileListLocalizer_CreatesExplicitEnglish()
    {
        var localizer = FileListLocalizer.Create("en");
        Assert.Equal("File System", localizer.FileSystemLabel);
        Assert.Equal("Layout plan is valid.", localizer.LayoutValidMessage);
    }

    private static byte[] CreateDirectoryEntry(byte mode, string name, string extension, byte password, ushort size, ushort load, ushort exec, int startCluster)
    {
        var entry = Enumerable.Repeat((byte)0x20, 32).ToArray();
        entry[0] = mode;
        WriteAscii(entry, 1, 13, name);
        WriteAscii(entry, 0x0E, 3, extension);
        entry[0x11] = password;
        BitConverter.GetBytes(size).CopyTo(entry, 0x12);
        BitConverter.GetBytes(load).CopyTo(entry, 0x14);
        BitConverter.GetBytes(exec).CopyTo(entry, 0x16);
        entry[0x1D] = (byte)((startCluster >> 14) & 0x7F);
        entry[0x1E] = (byte)(startCluster & 0x7F);
        entry[0x1F] = (byte)((startCluster >> 7) & 0x7F);
        return entry;
    }

    private static void WriteAscii(byte[] buffer, int offset, int length, string text)
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(text);
        Array.Copy(bytes, 0, buffer, offset, Math.Min(length, bytes.Length));
    }
}