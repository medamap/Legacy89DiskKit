using System.Text.RegularExpressions;
using Xunit;

namespace Legacy89DiskKit.Tests;

public sealed class CliVersionTest
{
    [Fact]
    public async Task Version_PrintsHumanReadableBuildStamp()
    {
        var result = await CliCommandRunner.RunAsync("--version");

        Assert.Equal(0, result.ExitCode);
        Assert.Matches(@"^2\.1\.0 build-\d{14}-\d{5}\r?\n?$", result.StandardOutput);
    }
}
