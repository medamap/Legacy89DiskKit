using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeSurfaceInfoTest
{
    [Fact]
    public void AbiVersion_IsStablePositiveValue()
    {
        Assert.True(NativeSurfaceInfo.AbiVersion > 0);
    }

    [Fact]
    public void GetCapabilityFlags_ContainsManagedBridgeAndFileRead()
    {
        var flags = NativeSurfaceInfo.GetCapabilityFlags();

        Assert.NotEqual(0, flags & NativeSurfaceInfo.CapabilityManagedBridge);
        Assert.NotEqual(0, flags & NativeSurfaceInfo.CapabilityFileRead);
        Assert.NotEqual(0, flags & NativeSurfaceInfo.CapabilityPathOpen);
    }

    [Fact]
    public void GetCapabilitySummary_ListsStableCapabilities()
    {
        var summary = NativeSurfaceInfo.GetCapabilitySummary();

        Assert.Contains("path-open", summary);
        Assert.Contains("file-read", summary);
        Assert.Contains("managed-bridge", summary);
    }
}
