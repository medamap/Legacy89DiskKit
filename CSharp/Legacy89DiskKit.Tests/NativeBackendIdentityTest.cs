using System.Runtime.InteropServices;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.Native.Domain;
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeBackendIdentityTest
{
    public NativeBackendIdentityTest()
    {
        NativeBridgeBackend.Reset();
    }

    [Fact]
    public void BackendIdentity_ReturnsExpectedManagedValues()
    {
        Assert.Equal("managed-bridge", NativeBackendIdentity.BackendKind);
        Assert.Equal("Legacy89DiskKit.NativeInterop", NativeBackendIdentity.BackendImplementation);
        Assert.Equal("Legacy89DiskKit.Application", NativeBackendIdentity.BackendTarget);
        Assert.Contains("managed-bridge", NativeBackendIdentity.GetBackendSummary());
    }

    [Fact]
    public void BackendIdentityExports_ReturnExpectedStrings()
    {
        Assert.Equal("managed-bridge", ReadSummary(NativeExportInvoker.GetBackendKind));
        Assert.Equal("Legacy89DiskKit.NativeInterop", ReadSummary(NativeExportInvoker.GetBackendImplementation));
        Assert.Equal("Legacy89DiskKit.Application", ReadSummary(NativeExportInvoker.GetBackendTarget));
        Assert.Contains("managed-bridge", ReadSummary(NativeExportInvoker.GetBackendSummary));
    }

    [Fact]
    public void BackendIdentity_FollowsConfiguredBackend()
    {
        NativeBridgeBackend.SetCurrent(new FakeNativeBridgeBackend());

        Assert.Equal("cpp-bridge", NativeBackendIdentity.BackendKind);
        Assert.Equal("Legacy89DiskKit.CppBridge", NativeBackendIdentity.BackendImplementation);
        Assert.Equal("Legacy89DiskKit.Cpp", NativeBackendIdentity.BackendTarget);
        Assert.Contains("cpp-bridge", NativeBackendIdentity.GetBackendSummary());
    }

    private static string ReadSummary(Func<IntPtr, int, int> reader)
    {
        var buffer = Marshal.AllocHGlobal(256);
        try
        {
            var length = reader(buffer, 256);
            return Marshal.PtrToStringUTF8(buffer, length) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private sealed class FakeNativeBridgeBackend : INativeBridgeBackend
    {
        public string BackendKind => "cpp-bridge";

        public string BackendImplementation => "Legacy89DiskKit.CppBridge";

        public string BackendTarget => "Legacy89DiskKit.Cpp";

        public INativeDiskSession CreateDisk(string path, DiskType diskType, string diskName)
        {
            throw new NotSupportedException();
        }

        public INativeDiskSession OpenDisk(string path, bool readOnly)
        {
            throw new NotSupportedException();
        }

        public INativeDiskSession OpenDisk(byte[] imageData, string imageFormat, bool readOnly)
        {
            throw new NotSupportedException();
        }
    }
}
