using Legacy89DiskKit.NativeInterop.Types;

namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeStatusCatalog
{
    private static readonly (LdkStatus Status, string Name)[] Entries =
    [
        (LdkStatus.Success, "success"),
        (LdkStatus.ErrorGeneric, "error-generic"),
        (LdkStatus.ErrorInvalidHandle, "error-invalid-handle"),
        (LdkStatus.ErrorInvalidArgument, "error-invalid-argument"),
        (LdkStatus.ErrorFileNotFound, "error-file-not-found"),
        (LdkStatus.ErrorDiskFull, "error-disk-full"),
        (LdkStatus.ErrorReadOnly, "error-read-only"),
        (LdkStatus.ErrorNotImplemented, "error-not-implemented"),
        (LdkStatus.ErrorBufferTooSmall, "error-buffer-too-small")
    ];

    public static IReadOnlyList<(LdkStatus Status, string Name)> GetEntries() => Entries;

    public static string GetName(LdkStatus status)
    {
        foreach (var entry in Entries)
        {
            if (entry.Status == status)
            {
                return entry.Name;
            }
        }

        return "unknown-status";
    }
}
