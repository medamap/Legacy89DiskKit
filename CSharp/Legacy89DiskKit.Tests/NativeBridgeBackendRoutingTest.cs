using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.Native;
using Legacy89DiskKit.DiskImage.Application;
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

        public INativeDiskSession OpenDisk(byte[] imageData, string imageFormat, bool readOnly)
        {
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

        // IDiskContainer
        public string FilePath => "";
        public bool IsReadOnly => true;
        public DiskType DiskType => DiskType.TwoD;
        public DiskContainerMetadata GetMetadata() => throw new NotImplementedException();
        public byte[] ReadSector(int cylinder, int head, int sector) => throw new NotImplementedException();
        public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted) => throw new NotImplementedException();
        public void WriteSector(int cylinder, int head, int sector, byte[] data) => throw new NotImplementedException();
        public bool SectorExists(int cylinder, int head, int sector) => false;
        public IEnumerable<SectorInfo> GetAllSectors() => Enumerable.Empty<SectorInfo>();
        public void Save() { }
        public void SaveAs(string filePath) => throw new NotImplementedException();

        public void Dispose()
        {
        }
    }
}
