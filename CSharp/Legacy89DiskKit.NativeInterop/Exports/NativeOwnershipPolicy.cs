namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeOwnershipPolicy
{
    public static string GetHandleLifecycleSummary()
    {
        return "open/create return owned handles; close releases one handle; close-all-handles releases every registered handle";
    }

    public static string GetBufferStringPolicySummary()
    {
        return "utf8 strings are caller-allocated; fixed buffers truncate and null-terminate; structured outputs use caller-allocated structs";
    }
}
