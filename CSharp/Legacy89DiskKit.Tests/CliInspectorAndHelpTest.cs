using Xunit;

namespace Legacy89DiskKit.Tests;

public sealed class CliInspectorAndHelpTest
{
    [Fact]
    public async Task FullHelp_ListsInspectorAndSectorCommands()
    {
        var result = await CliCommandRunner.RunAsync("--full-help", "--language", "en");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("l89 disk inspector", result.StandardOutput);
        Assert.Contains("l89 disk sector export", result.StandardOutput);
        Assert.Contains("--image-format", result.StandardOutput);
    }

    [Fact]
    public async Task BareImagePath_RoutesToInspector()
    {
        var imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");

        try
        {
            var createResult = await CliCommandRunner.RunAsync(
                "disk", "create", imagePath,
                "--disk-type", "2d",
                "--file-system", "hu-basic",
                "--language", "en");

            Assert.Equal(0, createResult.ExitCode);

            var result = await CliCommandRunner.RunAsync(imagePath, "--language", "en");

            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Container:", result.StandardOutput);
            Assert.Contains("File System: Hu-BASIC", result.StandardOutput);
        }
        finally
        {
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }
    }

    [Fact]
    public async Task DiskCreate_WithImageFormat_AppendsExtension()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var imagePathWithoutExtension = Path.Combine(tempDirectory, "sample");
        var expectedPath = $"{imagePathWithoutExtension}.d77";

        try
        {
            var result = await CliCommandRunner.RunAsync(
                "disk", "create", imagePathWithoutExtension,
                "--image-format", "d77",
                "--disk-type", "2d",
                "--language", "en");

            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(expectedPath), $"Expected disk image was not created: {expectedPath}");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }
}
