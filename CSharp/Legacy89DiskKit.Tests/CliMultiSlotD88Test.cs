using Xunit;

namespace Legacy89DiskKit.Tests;

public sealed class CliMultiSlotD88Test
{
    [Fact]
    public async Task List_OnMultiSlotD88_ReadsFirstSlot()
    {
        var imagePath = TestDiskFixtureFactory.CreateMultiSlotD88WithFormattedHuBasicFirstSlot("MULTISLOT_READ.D88");

        try
        {
            var result = await CliCommandRunner.RunAsync("list", imagePath, "--language", "en");

            Assert.Equal(0, result.ExitCode);
            Assert.DoesNotContain("multi-slot", result.StandardError, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Listing files for:", result.StandardOutput);
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
    public async Task Format_OnMultiSlotD88_IsRejectedInEnglish()
    {
        var imagePath = TestDiskFixtureFactory.CreateMultiSlotD88WithFormattedHuBasicFirstSlot("MULTISLOT_WRITE_EN.D88");

        try
        {
            var result = await CliCommandRunner.RunAsync("disk", "format", imagePath, "--file-system", "hu-basic", "--language", "en");

            Assert.Contains("Write operations for multi-slot D88 containers are not supported yet.", result.StandardError);
            Assert.Contains("Read operations currently target only the first slot.", result.StandardError);
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
    public async Task Format_OnMultiSlotD88_IsRejectedInJapanese()
    {
        var imagePath = TestDiskFixtureFactory.CreateMultiSlotD88WithFormattedHuBasicFirstSlot("MULTISLOT_WRITE_JA.D88");

        try
        {
            var result = await CliCommandRunner.RunAsync("disk", "format", imagePath, "--file-system", "hu-basic", "--language", "ja");

            Assert.Contains("複数スロットを含む D88 コンテナへの書き込みはまだ未対応です。", result.StandardError);
            Assert.Contains("読み込みは現在先頭スロットのみ対応しています。", result.StandardError);
        }
        finally
        {
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }
    }
}
