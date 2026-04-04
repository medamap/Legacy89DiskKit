using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Exception;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HuBasicWriteLimitTest
{
    [Fact]
    public void WriteFile_RejectsPayloadLargerThan65535Bytes()
    {
        var resolver = new ExplicitFileSystemResolver();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");

        try
        {
            using var container = D88DiskContainer.CreateNew(path, DiskType.TwoD, "TEST");
            using var fs = resolver.Create("hu-basic", container);
            fs.Format();
            resolver.InitializeForDetection(fs);

            var data = new byte[65536];
            var attributes = fs.CreateDefaultAttributes(false);

            var ex = Assert.Throws<FileSystemException>(() => fs.WriteFile("TOO-LARGE", data, attributes));
            Assert.Contains("65535", ex.Message);
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
    public void WriteFile_RejectsAsciiPayloadThatExceedsLimitAfterTerminator()
    {
        var resolver = new ExplicitFileSystemResolver();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");

        try
        {
            using var container = D88DiskContainer.CreateNew(path, DiskType.TwoD, "TEST");
            using var fs = resolver.Create("hu-basic", container);
            fs.Format();
            resolver.InitializeForDetection(fs);

            var data = new byte[65535];
            Array.Fill(data, (byte)'A');
            var attributes = fs.CreateDefaultAttributes(true);

            var ex = Assert.Throws<FileSystemException>(() => fs.WriteFile("ASCII-LIMIT", data, attributes));
            Assert.Contains("65535", ex.Message);
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
