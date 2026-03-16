using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
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

    private sealed class FakeNativeDiskSession : INativeDiskSession
    {
        public bool IsDisposed { get; private set; }

        public IFileSystem? FileSystem => null;

        public DiskContainerMetadata? GetContainerMetadata()
        {
            return null;
        }

        public void CloseDisk()
        {
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
