using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.NativeInterop.Core;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeHandleManagerMetadataTest
{
    [Fact]
    public void Register_StoresMetadataForHandle()
    {
        HandleManager.Clear();
        NativeBridgeBackend.SetCurrent(new ManagedNativeBridgeBackend());

        using var service = Legacy89DiskKitApplication.CreateDiskService();
        var handle = HandleManager.Register(service.Session!, new HandleMetadata("open-disk", true));

        try
        {
            Assert.True(HandleManager.TryGetMetadata(handle, out var metadata));
            Assert.Equal("open-disk", metadata.SourceOperation);
            Assert.True(metadata.IsWritable);
        }
        finally
        {
            HandleManager.Clear();
        }
    }

    [Fact]
    public void TryGetMetadata_ReturnsFalseForUnknownHandle()
    {
        HandleManager.Clear();

        Assert.False(HandleManager.TryGetMetadata(12345, out _));
    }
}
