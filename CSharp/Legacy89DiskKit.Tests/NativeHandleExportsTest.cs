using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.Native.Application;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeHandleExportsTest
{
    [Fact]
    public void HandleExports_ReflectRegisteredHandleState()
    {
        HandleManager.Clear();
        NativeBridgeBackend.SetCurrent(new ManagedNativeBridgeBackend());

        var session = new FakeNativeDiskSession();
        var handle = HandleManager.Register(session);

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
        NativeBridgeBackend.SetCurrent(new ManagedNativeBridgeBackend());

        var first = new FakeNativeDiskSession();
        var second = new FakeNativeDiskSession();
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
