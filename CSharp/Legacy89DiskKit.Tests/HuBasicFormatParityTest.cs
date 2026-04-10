using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HuBasicFormatParityTest
{
    [Theory]
    [InlineData(DiskType.TwoD)]
    [InlineData(DiskType.TwoDD)]
    [InlineData(DiskType.TwoHD)]
    public void Format_InitializesKeySectorsLikeTrueFormatter(DiskType diskType)
    {
        var path = Path.Combine(Path.GetTempPath(), $"hubasic-format-{diskType}-{Guid.NewGuid():N}.d88");
        try
        {
            CreateDisk(path, diskType);
            using var container = new D88DiskContainer(path, false);
            using var fs = new HuBasicFileSystem(container);

            fs.Format();

            switch (diskType)
            {
                case DiskType.TwoD:
                    AssertSectorPrefix(container.ReadSector(0, 0, 15), new byte[] { 0x01, 0x8F });
                    Assert.All(container.ReadSector(0, 0, 16), b => Assert.Equal((byte)0xE5, b));
                    Assert.All(container.ReadSector(0, 1, 1), b => Assert.Equal((byte)0xFF, b));
                    Assert.All(container.ReadSector(1, 0, 1), b => Assert.Equal((byte)0xE5, b));
                    Assert.Equal(0x8F, container.ReadSector(0, 0, 15)[80]);
                    Assert.Equal(0x8F, container.ReadSector(0, 0, 15)[127]);
                    break;
                case DiskType.TwoDD:
                    AssertSectorPrefix(container.ReadSector(0, 0, 15), new byte[] { 0x01, 0x8F });
                    Assert.Equal(0x8F, container.ReadSector(0, 0, 16)[32]);
                    Assert.Equal(0x8F, container.ReadSector(0, 0, 16)[127]);
                    Assert.Equal(0x00, container.ReadSector(0, 0, 16)[128]);
                    Assert.All(container.ReadSector(0, 1, 1), b => Assert.Equal((byte)0xFF, b));
                    Assert.All(container.ReadSector(1, 0, 1), b => Assert.Equal((byte)0xE5, b));
                    break;
                case DiskType.TwoHD:
                    AssertSectorPrefix(container.ReadSector(0, 1, 3), new byte[] { 0x01, 0x8F, 0x8F });
                    Assert.Equal(0x8F, container.ReadSector(0, 1, 4)[122]);
                    Assert.Equal(0x8F, container.ReadSector(0, 1, 4)[127]);
                    Assert.All(container.ReadSector(0, 1, 7), b => Assert.Equal((byte)0xFF, b));
                    Assert.All(container.ReadSector(0, 1, 22), b => Assert.Equal((byte)0xFF, b));
                    Assert.All(container.ReadSector(0, 1, 23), b => Assert.Equal((byte)0xE5, b));
                    break;
            }
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static void CreateDisk(string path, DiskType diskType)
    {
        if (diskType == DiskType.TwoHD)
        {
            using var created = D88DiskContainer.CreateNew(
                path,
                diskType,
                "TEST",
                (c, h) => c < 77 && h < 2 ? (26, (ushort)256, (byte)0x00) : null);
            created.Save();
            return;
        }

        using var created2 = D88DiskContainer.CreateNew(
            path,
            diskType,
            "TEST",
            (c, h) => c < (diskType == DiskType.TwoD ? 40 : 80) && h < 2 ? (16, (ushort)256, (byte)0x00) : null);
        created2.Save();
    }

    private static void AssertSectorPrefix(byte[] actual, byte[] expected)
    {
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], actual[i]);
        }
    }
}
