using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class CliCreateBlankTest
{
    [Fact]
    public async Task Create_WithoutFileSystem_CreatesBlankDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");

        try
        {
            var result = await CliCommandRunner.RunAsync("disk", "create", path, "--disk-type", "2hd", "--language", "en");
            
            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(path));

            // Verify it's a valid D88 but likely empty/not formatted
            using var container = new D88DiskContainer(path, true);
            Assert.Equal(Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoHD, container.DiskType);
            
            // If it's not formatted, 'list' command should fail to detect a filesystem.
            // (N88-BASIC does not detect 2HD by default, and we didn't format it).
            var listResult = await CliCommandRunner.RunAsync("list", path, "--language", "en");
            Assert.True(listResult.StandardError.Contains("Could not detect a supported file system"), 
                $"Expected error not found.\nStdOut: {listResult.StandardOutput}\nStdErr: {listResult.StandardError}\nExitCode: {listResult.ExitCode}");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task Create_WithFileSystem_FormatsDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");

        try
        {
            var result = await CliCommandRunner.RunAsync("disk", "create", path, "--disk-type", "2d", "--file-system", "hu-basic", "--language", "en");
            
            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(path));

            // Verify it's formatted as Hu-BASIC
            var listResult = await CliCommandRunner.RunAsync("list", path, "--language", "en");
            Assert.Equal(0, listResult.ExitCode);
            Assert.Contains("File System: Hu-BASIC", listResult.StandardOutput);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
