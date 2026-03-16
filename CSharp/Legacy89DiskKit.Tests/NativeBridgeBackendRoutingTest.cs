using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeBridgeBackendRoutingTest
{
    [Fact]
    public void OpenDisk_UsesConfiguredBackend()
    {
        HandleManager.Clear();
        var backend = new FakeNativeBridgeBackend();
        NativeBridgeBackend.SetCurrent(backend);

        using var path = new Utf8StringScope("fake.img");
        var handle = NativeExportInvoker.OpenDisk(path.Pointer, true);

        try
        {
            Assert.True(handle > 0);
            Assert.Equal("fake.img", backend.LastOpenedPath);
            Assert.True(backend.LastOpenedReadOnly);
            Assert.Equal(1, NativeExportInvoker.IsHandleValid(handle));
        }
        finally
        {
            NativeBridgeBackend.Reset();
            HandleManager.Clear();
        }
    }

    [Fact]
    public void CreateDisk_UsesConfiguredBackend()
    {
        HandleManager.Clear();
        var backend = new FakeNativeBridgeBackend();
        NativeBridgeBackend.SetCurrent(backend);

        using var path = new Utf8StringScope("fake.d88");
        using var name = new Utf8StringScope("FAKE");
        var handle = NativeExportInvoker.CreateDisk(path.Pointer, (int)LdkDiskType.TwoD, name.Pointer);

        try
        {
            Assert.True(handle > 0);
            Assert.Equal("fake.d88", backend.LastCreatedPath);
            Assert.Equal(DiskType.TwoD, backend.LastCreatedDiskType);
            Assert.Equal("FAKE", backend.LastCreatedDiskName);
            Assert.Equal(1, NativeExportInvoker.IsHandleValid(handle));
        }
        finally
        {
            NativeBridgeBackend.Reset();
            HandleManager.Clear();
        }
    }

    private sealed class FakeNativeBridgeBackend : INativeBridgeBackend
    {
        public string BackendKind => "fake";
        public string BackendImplementation => "Tests";
        public string BackendTarget => "Fake";
        public string? LastOpenedPath { get; private set; }
        public bool LastOpenedReadOnly { get; private set; }
        public string? LastCreatedPath { get; private set; }
        public DiskType LastCreatedDiskType { get; private set; }
        public string? LastCreatedDiskName { get; private set; }

        public INativeDiskSession OpenDisk(string path, bool readOnly)
        {
            LastOpenedPath = path;
            LastOpenedReadOnly = readOnly;
            return new FakeNativeDiskSession();
        }

        public INativeDiskSession CreateDisk(string path, DiskType diskType, string diskName)
        {
            LastCreatedPath = path;
            LastCreatedDiskType = diskType;
            LastCreatedDiskName = diskName;
            return new FakeNativeDiskSession();
        }
    }

    private sealed class FakeNativeDiskSession : INativeDiskSession
    {
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
        }
    }
}
