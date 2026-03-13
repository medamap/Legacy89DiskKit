using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HuBasicAllocationRulesTest
{
    [Fact]
    public void IsAllocatableCluster_RejectsHoleyFatRangeOnTwoHd()
    {
        var config = HuBasicConfiguration.GetDefault(DiskType.TwoHD);

        Assert.False(HuBasicAllocationRules.IsAllocatableCluster(DiskType.TwoHD, config, 0x80));
        Assert.False(HuBasicAllocationRules.IsAllocatableCluster(DiskType.TwoHD, config, 0x8F));
        Assert.True(HuBasicAllocationRules.IsAllocatableCluster(DiskType.TwoHD, config, 0x90));
    }

    [Fact]
    public void CollectFreeClusters_SkipsReservedAndAllocatedClusters()
    {
        var config = HuBasicConfiguration.GetDefault(DiskType.TwoD);
        var fat = new byte[config.TotalClusters];
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
    public void CollectFreeClusters_SkipsTwoHdUpperHalfEntries()
    {
        var config = HuBasicConfiguration.GetDefault(DiskType.TwoHD);
        var fat = new byte[512];

        var result = HuBasicAllocationRules.CollectFreeClusters(fat, DiskType.TwoHD, config, 4);

        Assert.DoesNotContain(0x80, result);
        Assert.Equal(4, result.Count);
    }
}
