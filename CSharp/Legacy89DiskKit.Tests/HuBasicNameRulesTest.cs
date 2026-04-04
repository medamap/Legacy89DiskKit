using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HuBasicNameRulesTest
{
    [Fact]
    public void ParseFileName_TrimsNameAndExtensionToHuBasicLimits()
    {
        var result = HuBasicNameRules.ParseFileName("ABCDEFGHIJKLMN.EXTENDED");

        Assert.Equal("ABCDEFGHIJKLM", result.FileName);
        Assert.Equal("EXT", result.Extension);
    }

    [Fact]
    public void BuildDisplayName_OmitsDotWhenExtensionIsEmpty()
    {
        Assert.Equal("TEST", HuBasicNameRules.BuildDisplayName("TEST", string.Empty));
        Assert.Equal("TEST.BIN", HuBasicNameRules.BuildDisplayName("TEST", "BIN"));
    }
}
