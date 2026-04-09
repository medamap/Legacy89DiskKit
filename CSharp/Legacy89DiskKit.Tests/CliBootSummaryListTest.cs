using Xunit;

namespace Legacy89DiskKit.Tests;

public sealed class CliBootSummaryListTest
{
    [Fact]
    public async Task CliList_HuBasicFileBackedBoot_ReportsFileBackedSummary()
    {
        var imagePath = Path.Combine(
            "/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1",
            "CZ8FB01.d88");

        var result = await CliCommandRunner.RunAsync("list", imagePath, "--language", "en");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Boot: file-backed", result.StandardOutput);
        Assert.Contains("Boot File: BASIC CZ8FB01.Sys", result.StandardOutput);
    }
}
