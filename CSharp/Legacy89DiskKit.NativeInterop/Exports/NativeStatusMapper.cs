using System.IO;
using Legacy89DiskKit.NativeInterop.Types;

namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeStatusMapper
{
    public static LdkStatus FromException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            FileNotFoundException => LdkStatus.ErrorFileNotFound,
            DirectoryNotFoundException => LdkStatus.ErrorFileNotFound,
            UnauthorizedAccessException => LdkStatus.ErrorReadOnly,
            NotSupportedException => LdkStatus.ErrorNotImplemented,
            ArgumentException => LdkStatus.ErrorInvalidArgument,
            InvalidOperationException => LdkStatus.ErrorGeneric,
            _ => LdkStatus.ErrorGeneric
        };
    }
}
