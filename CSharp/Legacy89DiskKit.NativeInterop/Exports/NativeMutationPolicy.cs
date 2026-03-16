namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeMutationPolicy
{
    private static readonly string[] MutatingOperations =
    [
        "ldk_create_disk",
        "ldk_write_file",
        "ldk_delete_file",
        "ldk_rename_file",
        "ldk_update_attributes",
        "ldk_write_boot_area",
        "ldk_format",
    ];

    public static IReadOnlyList<string> GetMutatingOperations() => MutatingOperations;

    public static string GetMutationPolicySummary()
    {
        return "mutating operations require writable handles; open-disk read-only handles are query-only; create-disk returns writable handles";
    }
}
