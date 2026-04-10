using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Exports;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeCatalogExportsTest
{
    [Fact]
    public void SupportedFileSystemExports_ReturnExpectedNames()
    {
        Assert.Equal(NativeSurfaceCatalog.GetSupportedFileSystems().Count, NativeExportInvoker.GetSupportedFileSystemCount());
        Assert.Equal("hu-basic", ReadCatalogItem(NativeExportInvoker.GetSupportedFileSystemName, 0));
        Assert.Equal("msx-dos", ReadCatalogItem(NativeExportInvoker.GetSupportedFileSystemName, 2));
    }

    [Fact]
    public void SupportedPlatformExports_ReturnExpectedNames()
    {
        Assert.Equal(NativeSurfaceCatalog.GetSupportedPlatforms().Count, NativeExportInvoker.GetSupportedPlatformCount());
        Assert.Equal("X1", ReadCatalogItem(NativeExportInvoker.GetSupportedPlatformName, 0));
        Assert.Equal("MSX", ReadCatalogItem(NativeExportInvoker.GetSupportedPlatformName, 2));
    }

    [Fact]
    public void SupportedImageFormatExports_ReturnExpectedNames()
    {
        Assert.Equal(NativeSurfaceCatalog.GetSupportedImageFormats().Count, NativeExportInvoker.GetSupportedImageFormatCount());
        Assert.Equal("d88", ReadCatalogItem(NativeExportInvoker.GetSupportedImageFormatName, 0));
        Assert.Equal("dsk", ReadCatalogItem(NativeExportInvoker.GetSupportedImageFormatName, 3));
    }

    [Fact]
    public void CatalogExport_ReturnsInvalidArgumentForOutOfRangeIndex()
    {
        var buffer = Marshal.AllocHGlobal(64);

        try
        {
            var result = NativeExportInvoker.GetSupportedFileSystemName(999, buffer, 64);
            Assert.Equal((int)LdkStatus.ErrorInvalidArgument, result);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [Fact]
    public void CatalogExport_TruncatesToTinyBuffer()
    {
        var buffer = Marshal.AllocHGlobal(3);

        try
        {
            var result = NativeExportInvoker.GetSupportedImageFormatName(0, buffer, 3);
            Assert.Equal(2, result);
            Assert.Equal("d8", Marshal.PtrToStringUTF8(buffer));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static string ReadCatalogItem(Func<int, IntPtr, int, int> reader, int index)
    {
        var buffer = Marshal.AllocHGlobal(64);

        try
        {
            var length = reader(index, buffer, 64);
            return Marshal.PtrToStringUTF8(buffer, length) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
