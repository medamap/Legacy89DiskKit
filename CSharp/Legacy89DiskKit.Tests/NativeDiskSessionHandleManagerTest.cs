using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.Native.Domain;
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.NativeInterop.Core;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeDiskSessionHandleManagerTest
{
    [Fact]
    public void Register_StoresNativeSessionAndDisposesOnUnregister()
    {
        HandleManager.Clear();
        var session = new FakeNativeDiskSession();

        var handle = HandleManager.Register(session, new HandleMetadata("native-session", false));

        Assert.True(HandleManager.TryGet(handle, out var storedSession));
        Assert.Same(session, storedSession);
        Assert.True(HandleManager.Unregister(handle));
        Assert.True(session.IsDisposed);
    }
}
