using DomainFileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HuBasicDirectoryRulesTest
{
    [Fact]
    public void CreateFileEntryForWrite_TrimsNameAndSetsMetadata()
    {
        var attributes = new ExtendedFileAttributes(DomainFileAttributes.None, 0x01, false, "Hu-BASIC");
        var data = new byte[16];

        var entry = HuBasicDirectoryRules.CreateFileEntryForWrite("ABCDEFGHIJKLMN.BINARY", data, attributes, 5, 0x1000, 0x1200);

        Assert.Equal("ABCDEFGHIJKLM", entry.FileName);
        Assert.Equal("BIN", entry.Extension);
        Assert.Equal(5, entry.StartCluster);
        Assert.Equal((ushort)0x1000, entry.LoadAddress);
        Assert.Equal((ushort)0x100F, entry.EndAddress);
        Assert.Equal((ushort)0x1200, entry.ExecutionAddress);
        var metadata = Assert.IsType<HuBasicFileMetadata>(entry.FileSystemMetadata);
        Assert.Equal(HuBasicFileType.Binary, metadata.FileType);
        Assert.Equal((ushort)16, metadata.RecordedSize);
    }
}
