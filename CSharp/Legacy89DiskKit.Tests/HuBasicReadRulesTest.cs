using Legacy89DiskKit.Domain.DiskImage.Model;
using DomainFileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HuBasicReadRulesTest
{
    [Fact]
    public void ResolveReadPayload_UsesTerminalLengthForTwoHdWhenRecordedSizeIsZero()
    {
        var config = HuBasicConfiguration.GetDefault(DiskType.TwoHD);
        var file = new FileEntry("TEST", "BIN", 0, null, null, new ExtendedFileAttributes(DomainFileAttributes.None, 0x01, false, "Hu-BASIC"));
        var data = Enumerable.Range(0, config.ClusterSize * 2).Select(i => (byte)(i & 0xFF)).ToArray();

        var result = HuBasicReadRules.ResolveReadPayload(data, file, DiskType.TwoHD, config, 2, 0x82);

        Assert.Equal((config.ClusterSize / config.SectorSize + 3) * config.SectorSize, result.Length);
    }

    [Fact]
    public void ResolveReadPayload_UsesAsciiEndMarkerForAsciiFiles()
    {
        var config = HuBasicConfiguration.GetDefault(DiskType.TwoD);
        var attributes = new ExtendedFileAttributes(DomainFileAttributes.None, 0x04, true, "Hu-BASIC");
        var file = new FileEntry("TEST", "ASC", 100, null, null, attributes);
        var data = new byte[] { 0x41, 0x42, 0x1A, 0x43 };

        var result = HuBasicReadRules.ResolveReadPayload(data, file, DiskType.TwoD, config, 1, 0x8F);

        Assert.Equal(new byte[] { 0x41, 0x42 }, result);
    }

    [Fact]
    public void ResolveReadPayload_UsesRecordedSizeForNonAsciiFiles()
    {
        var config = HuBasicConfiguration.GetDefault(DiskType.TwoD);
        var file = new FileEntry("TEST", "BIN", 3, null, null, new ExtendedFileAttributes(DomainFileAttributes.None, 0x01, false, "Hu-BASIC"));
        var data = new byte[] { 1, 2, 3, 4, 5 };

        var result = HuBasicReadRules.ResolveReadPayload(data, file, DiskType.TwoD, config, 1, 0x8F);

        Assert.Equal(new byte[] { 1, 2, 3 }, result);
    }
}
