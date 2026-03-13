using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HuBasicFatRulesTest
{
    [Fact]
    public void GetClusterChain_FollowsLinksUntilTerminal()
    {
        var fat = new byte[16];
        var config = HuBasicConfiguration.GetDefault(DiskType.TwoD);

        HuBasicFatRules.SetEntry(fat, 2, 3);
        HuBasicFatRules.SetEntry(fat, 3, 4);
        HuBasicFatRules.SetEntry(fat, 4, 0x82);

        var result = HuBasicFatRules.GetClusterChain(fat, config, 2);

        Assert.Equal(new[] { 2, 3, 4 }, result.Chain);
        Assert.Equal(0x82, result.TerminalFlag);
    }

    [Fact]
    public void GetLastClusterUsedSectors_ReturnsZeroForNonTerminal()
    {
        Assert.Equal(0, HuBasicFatRules.GetLastClusterUsedSectors(0x05));
        Assert.Equal(0, HuBasicFatRules.GetLastClusterUsedSectors(0xFF));
        Assert.Equal(3, HuBasicFatRules.GetLastClusterUsedSectors(0x82));
    }

    [Fact]
    public void GetEntry_ReturnsTerminalForOutOfRangeCluster()
    {
        var fat = new byte[4];

        Assert.Equal(0x8F, HuBasicFatRules.GetEntry(fat, 9));
    }

    [Fact]
    public void ApplyChain_WritesTerminalFlagToLastCluster()
    {
        var fat = new byte[16];

        HuBasicFatRules.ApplyChain(fat, new[] { 2, 3, 4 }, 0x82);

        Assert.Equal(3, HuBasicFatRules.GetEntry(fat, 2));
        Assert.Equal(4, HuBasicFatRules.GetEntry(fat, 3));
        Assert.Equal(0x82, HuBasicFatRules.GetEntry(fat, 4));
    }
}
