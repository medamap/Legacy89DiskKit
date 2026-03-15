using System.Runtime.InteropServices;
using System.Text;
using Legacy89DiskKit.NativeInterop.Exports;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class NativeStringWriterTest
{
    [Fact]
    public void WriteUtf8_WritesNullTerminatedUtf8()
    {
        var buffer = Marshal.AllocHGlobal(16);
        try
        {
            var written = NativeStringWriter.WriteUtf8(buffer, 16, "abc");
            var bytes = new byte[written + 1];
            Marshal.Copy(buffer, bytes, 0, bytes.Length);

            Assert.Equal(3, written);
            Assert.Equal("abc", Encoding.UTF8.GetString(bytes, 0, written));
            Assert.Equal(0, bytes[^1]);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [Fact]
    public void WriteUtf8_TruncatesToLeaveNullTerminator()
    {
        var buffer = Marshal.AllocHGlobal(4);
        try
        {
            var written = NativeStringWriter.WriteUtf8(buffer, 4, "abcdef");
            var bytes = new byte[4];
            Marshal.Copy(buffer, bytes, 0, bytes.Length);

            Assert.Equal(3, written);
            Assert.Equal("abc", Encoding.UTF8.GetString(bytes, 0, written));
            Assert.Equal(0, bytes[3]);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
