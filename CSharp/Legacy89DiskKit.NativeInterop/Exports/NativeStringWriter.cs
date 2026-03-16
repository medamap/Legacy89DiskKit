using System.Runtime.InteropServices;
using System.Text;

namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeStringWriter
{
    public static int WriteUtf8(IntPtr bufferPtr, int capacity, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (bufferPtr == IntPtr.Zero || capacity <= 0)
        {
            return 0;
        }

        var bytes = Encoding.UTF8.GetBytes(value);
        var writableLength = Math.Min(bytes.Length, capacity - 1);

        Marshal.Copy(bytes, 0, bufferPtr, writableLength);
        Marshal.WriteByte(bufferPtr, writableLength, 0);
        return writableLength;
    }
}
