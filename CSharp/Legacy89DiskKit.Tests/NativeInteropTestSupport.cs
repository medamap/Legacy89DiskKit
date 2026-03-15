using System.Runtime.InteropServices;
using Legacy89DiskKit.Application;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.NativeInterop.Exports;

namespace Legacy89DiskKit.Tests;

internal sealed class Utf8StringScope : IDisposable
{
    public Utf8StringScope(string value)
    {
        Pointer = Marshal.StringToCoTaskMemUTF8(value);
    }

    public IntPtr Pointer { get; }

    public void Dispose()
    {
        Marshal.FreeCoTaskMem(Pointer);
    }
}

internal sealed class TempFormattedDiskScope : IDisposable
{
    public TempFormattedDiskScope(string fileSystemName = "hu-basic")
    {
        ImagePath = Path.Combine(Path.GetTempPath(), $"ldk-native-{Guid.NewGuid():N}.d88");

        using var service = Legacy89DiskKitApplication.CreateDiskService();
        service.CreateDisk(ImagePath, DiskType.TwoD, "NATIVETEST");

        var resolver = Legacy89DiskKitApplication.CreateExplicitFileSystemResolver();
        var container = service.OpenDisk(ImagePath, readOnly: false);
        using var fileSystem = resolver.Create(fileSystemName, container);
        fileSystem.Format();
        resolver.InitializeForDetection(fileSystem);
    }

    public string ImagePath { get; }

    public void Dispose()
    {
        if (File.Exists(ImagePath))
        {
            File.Delete(ImagePath);
        }
    }
}

internal static class NativeExportInvoker
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int NoArgIntDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int IntIntDelegate(int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int IntPtrIntDelegate(IntPtr pointer, int capacity);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int StatusBufferDelegate(int value, IntPtr pointer, int capacity);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int IntPtrBoolDelegate(IntPtr pointer, bool flag);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int IntPtrIntIntPtrDelegate(IntPtr path, int diskType, IntPtr name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int IntIntPtrDelegate(int handle, IntPtr pointer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int HandleBufferDelegate(int handle, IntPtr pointer, int length);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int HandleNameBufferDelegate(int handle, IntPtr name, IntPtr buffer, int capacity);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int IntIntPtrIntUshortDelegate(int handle, IntPtr pointer, IntPtr data, int length, ushort attributes);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int IntIntPtrIntPtrDelegate(int handle, IntPtr first, IntPtr second);

    public static int GetAbiVersion() => GetDelegate<NoArgIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetAbiVersion))();
    public static int GetCapabilityFlags() => GetDelegate<NoArgIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetCapabilityFlags))();
    public static int GetCapabilitySummary(IntPtr pointer, int capacity) => GetDelegate<IntPtrIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetCapabilitySummary))(pointer, capacity);
    public static int GetStatusName(int statusCode, IntPtr pointer, int capacity) => GetDelegate<StatusBufferDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetStatusName))(statusCode, pointer, capacity);
    public static int IsHandleValid(int handle) => GetDelegate<IntIntDelegate>(typeof(NativeHandleExports), nameof(NativeHandleExports.IsHandleValid))(handle);
    public static int GetOpenHandleCount() => GetDelegate<NoArgIntDelegate>(typeof(NativeHandleExports), nameof(NativeHandleExports.GetOpenHandleCount))();
    public static int CloseAllHandles() => GetDelegate<NoArgIntDelegate>(typeof(NativeHandleExports), nameof(NativeHandleExports.CloseAllHandles))();
    public static int OpenDisk(IntPtr path, bool readOnly) => GetDelegate<IntPtrBoolDelegate>(typeof(DiskExports), nameof(DiskExports.OpenDisk))(path, readOnly);
    public static int CreateDisk(IntPtr path, int diskType, IntPtr name) => GetDelegate<IntPtrIntIntPtrDelegate>(typeof(DiskExports), nameof(DiskExports.CreateDisk))(path, diskType, name);
    public static int CloseDisk(int handle) => GetDelegate<IntIntDelegate>(typeof(DiskExports), nameof(DiskExports.CloseDisk))(handle);
    public static int GetFileSystemInfo(int handle, IntPtr pointer) => GetDelegate<IntIntPtrDelegate>(typeof(DiskExports), nameof(DiskExports.GetFileSystemInfo))(handle, pointer);
    public static int GetFilesCount(int handle, IntPtr pointer) => GetDelegate<IntIntPtrDelegate>(typeof(DiskExports), nameof(DiskExports.GetFilesCount))(handle, pointer);
    public static int ReadFile(int handle, IntPtr name, IntPtr buffer, int capacity) => GetDelegate<HandleNameBufferDelegate>(typeof(FileExports), nameof(FileExports.ReadFile))(handle, name, buffer, capacity);
    public static int DeleteFile(int handle, IntPtr name) => GetDelegate<IntIntPtrDelegate>(typeof(FileExports), nameof(FileExports.DeleteFile))(handle, name);
    public static int WriteFile(int handle, IntPtr name, IntPtr data, int length, ushort attributes) => GetDelegate<IntIntPtrIntUshortDelegate>(typeof(FileExports), nameof(FileExports.WriteFile))(handle, name, data, length, attributes);
    public static int RenameFile(int handle, IntPtr oldName, IntPtr newName) => GetDelegate<IntIntPtrIntPtrDelegate>(typeof(FileExports), nameof(FileExports.RenameFile))(handle, oldName, newName);

    private static T GetDelegate<T>(Type type, string methodName) where T : Delegate
    {
        var method = type.GetMethod(methodName) ?? throw new InvalidOperationException($"Method not found: {type.FullName}.{methodName}");
        return Marshal.GetDelegateForFunctionPointer<T>(method.MethodHandle.GetFunctionPointer());
    }
}
