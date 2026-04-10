namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeExportCatalog
{
    private static readonly ExportEntry[] Entries =
    [
        new("ldk_open_disk", "disk"),
        new("ldk_create_disk", "disk"),
        new("ldk_close_disk", "disk"),
        new("ldk_get_abi_version", "info"),
        new("ldk_get_capability_flags", "info"),
        new("ldk_get_capability_summary", "info"),
        new("ldk_get_status_name", "info"),
        new("ldk_get_status_count", "info"),
        new("ldk_get_status_code_at", "info"),
        new("ldk_get_status_name_at", "info"),
        new("ldk_get_supported_file_system_count", "info"),
        new("ldk_get_supported_file_system_name", "info"),
        new("ldk_get_supported_platform_count", "info"),
        new("ldk_get_supported_platform_name", "info"),
        new("ldk_get_supported_image_format_count", "info"),
        new("ldk_get_supported_image_format_name", "info"),
        new("ldk_get_invalid_handle_value", "info"),
        new("ldk_get_handle_lifecycle_summary", "info"),
        new("ldk_get_handle_value_summary", "info"),
        new("ldk_get_buffer_string_policy_summary", "info"),
        new("ldk_get_backend_kind", "info"),
        new("ldk_get_backend_implementation", "info"),
        new("ldk_get_backend_target", "info"),
        new("ldk_get_backend_summary", "info"),
        new("ldk_get_export_count", "info"),
        new("ldk_get_export_name_at", "info"),
        new("ldk_get_export_group_at", "info"),
        new("ldk_is_handle_valid", "handle"),
        new("ldk_get_open_handle_count", "handle"),
        new("ldk_close_all_handles", "handle"),
        new("ldk_get_file_system_info", "disk"),
        new("ldk_get_container_metadata", "disk"),
        new("ldk_get_files_count", "disk"),
        new("ldk_get_files", "disk"),
        new("ldk_read_file", "file"),
        new("ldk_delete_file", "file"),
        new("ldk_write_file", "file"),
        new("ldk_rename_file", "file"),
        new("ldk_update_attributes", "file"),
        new("ldk_read_boot_area", "disk"),
        new("ldk_write_boot_area", "disk"),
        new("ldk_format", "disk"),
    ];

    public static IReadOnlyList<ExportEntry> GetEntries() => Entries;

    public readonly record struct ExportEntry(string Name, string Group);
}
