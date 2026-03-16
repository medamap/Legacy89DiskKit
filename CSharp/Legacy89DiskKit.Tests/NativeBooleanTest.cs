using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeBooleanTest
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(-1, true)]
    [InlineData(42, true)]
    public void ToManagedBoolean_UsesZeroAsFalse(int value, bool expected)
    {
        Assert.Equal(expected, NativeBoolean.ToManagedBoolean(value));
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public void FromManagedBoolean_ReturnsStableIntegerFlag(bool value, int expected)
    {
        Assert.Equal(expected, NativeBoolean.FromManagedBoolean(value));
    }
}
