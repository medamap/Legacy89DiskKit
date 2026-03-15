using Legacy89DiskKit.NativeInterop.Exports;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeStatusMapperTest
{
    [Fact]
    public void FromException_MapsFileNotFound()
    {
        var status = NativeStatusMapper.FromException(new FileNotFoundException("missing"));
        Assert.Equal(LdkStatus.ErrorFileNotFound, status);
    }

    [Fact]
    public void FromException_MapsUnauthorizedAccess()
    {
        var status = NativeStatusMapper.FromException(new UnauthorizedAccessException("denied"));
        Assert.Equal(LdkStatus.ErrorReadOnly, status);
    }

    [Fact]
    public void FromException_MapsNotSupported()
    {
        var status = NativeStatusMapper.FromException(new NotSupportedException("unsupported"));
        Assert.Equal(LdkStatus.ErrorNotImplemented, status);
    }

    [Fact]
    public void FromException_FallsBackToGeneric()
    {
        var status = NativeStatusMapper.FromException(new Exception("boom"));
        Assert.Equal(LdkStatus.ErrorGeneric, status);
    }
}
