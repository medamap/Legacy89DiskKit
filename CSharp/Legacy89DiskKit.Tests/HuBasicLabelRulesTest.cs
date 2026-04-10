using DomainFileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HuBasicLabelRulesTest
{
    [Fact]
    public void IsVirtualLabelEntry_ReturnsTrueForSentinelAsciiLabel()
    {
        var metadata = new HuBasicFileMetadata(HuBasicFileType.Ascii, true, false, false, true, false, 0, 0xFFFF, 0xFFFF, 0x7FFF, 0x44, 0x01);
        var entry = new FileEntry("-------------", "---", 0, null, null, new ExtendedFileAttributes(DomainFileAttributes.ReadOnly, 0x44, true, "Hu-BASIC"), 0x7FFF, 0xFFFF, 0xFFFF, 0xFFFF, null, null, metadata);

        Assert.True(HuBasicLabelRules.IsVirtualLabelEntry(entry));
    }

    [Fact]
    public void CanMergeLabelEntries_RequiresMatchingMetadataAndDotPrefixedSecondName()
    {
        var previous = new VirtualDirectoryLabelEntry("-------------", string.Empty, 0x44, 0x01, 0, 0xFFFF, 0xFFFF, 0xFFFF, 0x7FFF);
        var current = new VirtualDirectoryLabelEntry(".---", string.Empty, 0x44, 0x01, 0, 0xFFFF, 0xFFFF, 0xFFFF, 0x7FFF);

        Assert.True(HuBasicLabelRules.CanMergeLabelEntries(previous, current));
    }
}
