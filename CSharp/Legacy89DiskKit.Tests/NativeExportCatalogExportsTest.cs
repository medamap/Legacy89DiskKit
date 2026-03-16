using System.Runtime.InteropServices;
using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeExportCatalogExportsTest
{
    [Fact]
    public void GetExportCount_ReturnsCatalogSize()
    {
        Assert.Equal(NativeExportCatalog.GetEntries().Count, NativeExportInvoker.GetExportCount());
    }

    [Fact]
    public void GetExportNameAndGroupAt_ReturnConfiguredEntry()
    {
        var entries = NativeExportCatalog.GetEntries();
        var index = -1;
        for (var i = 0; i < entries.Count; i++)
        {
            if (entries[i].Name == "ldk_open_disk")
            {
                index = i;
                break;
            }
        }

        var nameBuffer = Marshal.AllocHGlobal(128);
        var groupBuffer = Marshal.AllocHGlobal(64);
        try
        {
            var nameLength = NativeExportInvoker.GetExportNameAt(index, nameBuffer, 128);
            var groupLength = NativeExportInvoker.GetExportGroupAt(index, groupBuffer, 64);

            Assert.Equal("ldk_open_disk", Marshal.PtrToStringUTF8(nameBuffer, nameLength));
            Assert.Equal("disk", Marshal.PtrToStringUTF8(groupBuffer, groupLength));
        }
        finally
        {
            Marshal.FreeHGlobal(nameBuffer);
            Marshal.FreeHGlobal(groupBuffer);
        }
    }

    [Fact]
    public void GetExportNameAt_ReturnsInvalidArgumentForOutOfRangeIndex()
    {
        var buffer = Marshal.AllocHGlobal(64);
        try
        {
            var status = NativeExportInvoker.GetExportNameAt(-1, buffer, 64);
            Assert.Equal((int)Legacy89DiskKit.NativeInterop.Types.LdkStatus.ErrorInvalidArgument, status);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
