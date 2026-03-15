using Legacy89DiskKit.Application;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class NativeHandleExportsTest
{
    [Fact]
    public void HandleExports_ReflectRegisteredHandleState()
    {
        HandleManager.Clear();

        using var service = Legacy89DiskKitApplication.CreateDiskService();
        var handle = HandleManager.Register(service);

        Assert.Equal(1, NativeHandleExports.IsHandleValid(handle));
        Assert.True(NativeHandleExports.GetOpenHandleCount() >= 1);

        Assert.True(HandleManager.Unregister(handle));
        Assert.Equal(0, NativeHandleExports.IsHandleValid(handle));
        Assert.Equal(0, NativeHandleExports.GetOpenHandleCount());
    }
}
