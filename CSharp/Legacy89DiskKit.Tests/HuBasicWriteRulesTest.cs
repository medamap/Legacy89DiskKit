using Legacy89DiskKit.DiskImage.Domain.Model;
using DomainFileAttributes = Legacy89DiskKit.FileSystem.Domain.Model.FileAttributes;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Models;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HuBasicWriteRulesTest
{
    [Fact]
    public void PrepareWritePayload_AppendsAsciiEofWhenMissing()
    {
        var attributes = new ExtendedFileAttributes(DomainFileAttributes.None, 0x04, true, "Hu-BASIC");
        var result = HuBasicWriteRules.PrepareWritePayload(new byte[] { 0x41, 0x42 }, attributes);

        Assert.Equal(new byte[] { 0x41, 0x42, 0x1A }, result);
    }

    [Fact]
    public void GetClustersNeeded_ReturnsAtLeastOneCluster()
    {
        var config = HuBasicConfiguration.GetDefault(DiskType.TwoD);

        Assert.Equal(1, HuBasicWriteRules.GetClustersNeeded(0, config));
        Assert.Equal(2, HuBasicWriteRules.GetClustersNeeded(config.ClusterSize + 1, config));
    }

    [Fact]
    public void GetTerminalFlagForLength_UsesLastClusterSectorCount()
    {
        var config = HuBasicConfiguration.GetDefault(DiskType.TwoD);
        int length = config.ClusterSize + (3 * config.SectorSize);

        var result = HuBasicWriteRules.GetTerminalFlagForLength(length, config);

        Assert.Equal(0x82, result);
    }
}
