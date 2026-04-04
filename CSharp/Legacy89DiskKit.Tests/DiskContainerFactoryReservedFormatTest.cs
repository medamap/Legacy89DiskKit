using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.DiskImage.Infrastructure.Factory;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class DiskContainerFactoryReservedFormatTest
{
    [Theory]
    [InlineData(".xdf")]
    [InlineData(".hdi")]
    [InlineData(".xhd")]
    [InlineData(".mo")]
    [InlineData(".iso")]
    [InlineData(".img")]
    public void Open_ReservedFormat_ThrowsReservedMessage(string extension)
    {
        var factory = new DiskContainerFactory();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");

        try
        {
            File.WriteAllBytes(path, Array.Empty<byte>());
            var ex = Assert.Throws<NotSupportedException>(() => factory.Open(path, true));
            Assert.Contains("This feature is reserved, please request!!", ex.Message);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Theory]
    [InlineData(".xdf")]
    [InlineData(".hdf")]
    [InlineData(".hdi")]
    [InlineData(".img")]
    public void Create_ReservedFormat_ThrowsReservedMessage(string extension)
    {
        var factory = new DiskContainerFactory();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");

        try
        {
            var ex = Assert.Throws<NotSupportedException>(() => factory.Create(path, DiskType.TwoD, "TEST"));
            Assert.Contains("This feature is reserved, please request!!", ex.Message);
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
