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

        Assert.Equal(1, NativeExportInvoker.IsHandleValid(handle));
        Assert.True(NativeExportInvoker.GetOpenHandleCount() >= 1);

        Assert.True(HandleManager.Unregister(handle));
        Assert.Equal(0, NativeExportInvoker.IsHandleValid(handle));
        Assert.Equal(0, NativeExportInvoker.GetOpenHandleCount());
    }

    [Fact]
    public void CloseAllHandles_ClearsRegisteredHandleState()
    {
        HandleManager.Clear();

        using var first = Legacy89DiskKitApplication.CreateDiskService();
        using var second = Legacy89DiskKitApplication.CreateDiskService();
        var firstHandle = HandleManager.Register(first);
        var secondHandle = HandleManager.Register(second);

        Assert.True(NativeExportInvoker.GetOpenHandleCount() >= 2);

        var result = NativeExportInvoker.CloseAllHandles();

        Assert.Equal(0, result);
        Assert.Equal(0, NativeExportInvoker.GetOpenHandleCount());
        Assert.Equal(0, NativeExportInvoker.IsHandleValid(firstHandle));
        Assert.Equal(0, NativeExportInvoker.IsHandleValid(secondHandle));
    }
}
