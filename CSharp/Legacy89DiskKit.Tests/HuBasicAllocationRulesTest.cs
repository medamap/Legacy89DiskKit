using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Models;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HuBasicAllocationRulesTest
{
    [Fact]
    public void IsAllocatableCluster_UsesConfiguredClusterRangeOnTwoHd()
    {
        var config = HuBasicConfiguration.GetDefault(DiskType.TwoHD);

        Assert.True(HuBasicAllocationRules.IsAllocatableCluster(DiskType.TwoHD, config, 0x80));
        Assert.True(HuBasicAllocationRules.IsAllocatableCluster(DiskType.TwoHD, config, 0xF9));
        Assert.False(HuBasicAllocationRules.IsAllocatableCluster(DiskType.TwoHD, config, 0xFA));
    }

    [Fact]
    public void CollectFreeClusters_SkipsReservedAndAllocatedClusters()
    {
        var config = HuBasicConfiguration.GetDefault(DiskType.TwoD);
        var fat = new byte[256];
        HuBasicFatRules.SetEntry(fat, config.ReservedClusters, 0x8F);

        var result = HuBasicAllocationRules.CollectFreeClusters(fat, DiskType.TwoD, config, 3);

        Assert.Equal(new[]
        {
            config.ReservedClusters + 1,
            config.ReservedClusters + 2,
            config.ReservedClusters + 3
        }, result);
    }

    [Fact]
    public void CollectFreeClusters_UsesConfiguredTwoHdClusterCount()
    {
        var config = HuBasicConfiguration.GetDefault(DiskType.TwoHD);
        var fat = new byte[512];

        var result = HuBasicAllocationRules.CollectFreeClusters(fat, DiskType.TwoHD, config, 4);

        Assert.Equal(config.ReservedClusters, result[0]);
        Assert.Equal(4, result.Count);
    }
}
