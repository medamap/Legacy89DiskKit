using System.Runtime.InteropServices;
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Interface.Registry;
using Legacy89DiskKit.Native.Domain;
using Legacy89DiskKit.NativeInterop.Exports;
using Legacy89DiskKit.NativeInterop.Types;

namespace Legacy89DiskKit.Tests;

internal sealed class FakeNativeDiskSession : INativeDiskSession
{
    public bool IsDisposed { get; private set; }

    public IFileSystem? FileSystem => null;

    public DiskContainerMetadata? GetContainerMetadata()
    {
        return null;
    }

    public void CloseDisk()
    {
    }

    // IDiskContainer
    public string FilePath => "";
    public bool IsReadOnly => true;
    public DiskType DiskType => DiskType.TwoD;
    public DiskContainerMetadata GetMetadata() => throw new NotImplementedException();
    public byte[] ReadSector(int cylinder, int head, int sector) => throw new NotImplementedException();
    public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted) => throw new NotImplementedException();
    public void WriteSector(int cylinder, int head, int sector, byte[] data) => throw new NotImplementedException();
    public bool SectorExists(int cylinder, int head, int sector) => false;
    public IEnumerable<SectorInfo> GetAllSectors() => Enumerable.Empty<SectorInfo>();
    public void Save() { }
    public void SaveAs(string filePath) => throw new NotImplementedException();

    public void Dispose()
    {
        IsDisposed = true;
    }
}

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

        using var service = CreateDiskService();
        service.CreateDisk(ImagePath, DiskType.TwoD, "NATIVETEST");

        var resolver = new ExplicitFileSystemResolver();
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

    private static DiskService CreateDiskService()
    {
        return new DiskService(fsRegistry: CreateFileSystemRegistry());
    }

    private static IFileSystemRegistry CreateFileSystemRegistry()
    {
        var registry = new FileSystemRegistry();
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.XDos.Provider.XDosFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Provider.HuBasicFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Cpm.Provider.CpmFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Pc88.Provider.N88BasicFileSystemProvider());
        registry.Register(new Legacy89DiskKit.FileSystem.Infrastructure.Msx.Provider.MsxDosFileSystemProvider());
        return registry;
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
    private delegate int IntPtrIntFlagDelegate(IntPtr pointer, int flag);

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

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int IntIntPtrIntDelegate(int handle, IntPtr pointer, int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int IntIntPtrUshortDelegate(int handle, IntPtr pointer, ushort value);

    public static int GetAbiVersion() => GetDelegate<NoArgIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetAbiVersion))();
    public static int GetCapabilityFlags() => GetDelegate<NoArgIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetCapabilityFlags))();
    public static int GetCapabilitySummary(IntPtr pointer, int capacity) => GetDelegate<IntPtrIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetCapabilitySummary))(pointer, capacity);
    public static int GetStatusName(int statusCode, IntPtr pointer, int capacity) => GetDelegate<StatusBufferDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetStatusName))(statusCode, pointer, capacity);
    public static int GetStatusCount() => GetDelegate<NoArgIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetStatusCount))();
    public static int GetStatusCodeAt(int index) => GetDelegate<IntIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetStatusCodeAt))(index);
    public static int GetStatusNameAt(int index, IntPtr pointer, int capacity) => GetDelegate<StatusBufferDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetStatusNameAt))(index, pointer, capacity);
    public static int GetSupportedFileSystemCount() => GetDelegate<NoArgIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetSupportedFileSystemCount))();
    public static int GetSupportedFileSystemName(int index, IntPtr pointer, int capacity) => GetDelegate<StatusBufferDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetSupportedFileSystemName))(index, pointer, capacity);
    public static int GetSupportedPlatformCount() => GetDelegate<NoArgIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetSupportedPlatformCount))();
    public static int GetSupportedPlatformName(int index, IntPtr pointer, int capacity) => GetDelegate<StatusBufferDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetSupportedPlatformName))(index, pointer, capacity);
    public static int GetSupportedImageFormatCount() => GetDelegate<NoArgIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetSupportedImageFormatCount))();
    public static int GetSupportedImageFormatName(int index, IntPtr pointer, int capacity) => GetDelegate<StatusBufferDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetSupportedImageFormatName))(index, pointer, capacity);
    public static int GetInvalidHandleValue() => GetDelegate<NoArgIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetInvalidHandleValue))();
    public static int GetHandleLifecycleSummary(IntPtr pointer, int capacity) => GetDelegate<IntPtrIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetHandleLifecycleSummary))(pointer, capacity);
    public static int GetHandleValueSummary(IntPtr pointer, int capacity) => GetDelegate<IntPtrIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetHandleValueSummary))(pointer, capacity);
    public static int GetBufferStringPolicySummary(IntPtr pointer, int capacity) => GetDelegate<IntPtrIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetBufferStringPolicySummary))(pointer, capacity);
    public static int GetMutationPolicySummary(IntPtr pointer, int capacity) => GetDelegate<IntPtrIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetMutationPolicySummary))(pointer, capacity);
    public static int GetBackendKind(IntPtr pointer, int capacity) => GetDelegate<IntPtrIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetBackendKind))(pointer, capacity);
    public static int GetBackendImplementation(IntPtr pointer, int capacity) => GetDelegate<IntPtrIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetBackendImplementation))(pointer, capacity);
    public static int GetBackendTarget(IntPtr pointer, int capacity) => GetDelegate<IntPtrIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetBackendTarget))(pointer, capacity);
    public static int GetBackendSummary(IntPtr pointer, int capacity) => GetDelegate<IntPtrIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetBackendSummary))(pointer, capacity);
    public static int GetExportCount() => GetDelegate<NoArgIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetExportCount))();
    public static int GetExportNameAt(int index, IntPtr pointer, int capacity) => GetDelegate<StatusBufferDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetExportNameAt))(index, pointer, capacity);
    public static int GetExportGroupAt(int index, IntPtr pointer, int capacity) => GetDelegate<StatusBufferDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetExportGroupAt))(index, pointer, capacity);
    public static int GetMutatingOperationCount() => GetDelegate<NoArgIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetMutatingOperationCount))();
    public static int GetMutatingOperationNameAt(int index, IntPtr pointer, int capacity) => GetDelegate<StatusBufferDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetMutatingOperationNameAt))(index, pointer, capacity);
    public static int GetOpenModeSummary(IntPtr pointer, int capacity) => GetDelegate<IntPtrIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetOpenModeSummary))(pointer, capacity);
    public static int GetOpenModeCount() => GetDelegate<NoArgIntDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetOpenModeCount))();
    public static int GetOpenModeNameAt(int index, IntPtr pointer, int capacity) => GetDelegate<StatusBufferDelegate>(typeof(NativeInfoExports), nameof(NativeInfoExports.GetOpenModeNameAt))(index, pointer, capacity);
    public static int IsHandleValid(int handle) => GetDelegate<IntIntDelegate>(typeof(NativeHandleExports), nameof(NativeHandleExports.IsHandleValid))(handle);
    public static int GetOpenHandleCount() => GetDelegate<NoArgIntDelegate>(typeof(NativeHandleExports), nameof(NativeHandleExports.GetOpenHandleCount))();
    public static int GetHandleSourceOperation(int handle, IntPtr pointer, int capacity) => GetDelegate<IntIntPtrIntDelegate>(typeof(NativeHandleExports), nameof(NativeHandleExports.GetHandleSourceOperation))(handle, pointer, capacity);
    public static int GetHandleIsWritable(int handle) => GetDelegate<IntIntDelegate>(typeof(NativeHandleExports), nameof(NativeHandleExports.GetHandleIsWritable))(handle);
    public static int GetHandleSummary(int handle, IntPtr pointer, int capacity) => GetDelegate<IntIntPtrIntDelegate>(typeof(NativeHandleExports), nameof(NativeHandleExports.GetHandleSummary))(handle, pointer, capacity);
    public static int CloseAllHandles() => GetDelegate<NoArgIntDelegate>(typeof(NativeHandleExports), nameof(NativeHandleExports.CloseAllHandles))();
    public static int OpenDisk(IntPtr path, bool readOnly) => GetDelegate<IntPtrIntFlagDelegate>(typeof(DiskExports), nameof(DiskExports.OpenDisk))(path, NativeBoolean.FromManagedBoolean(readOnly));
    public static int CreateDisk(IntPtr path, int diskType, IntPtr name) => GetDelegate<IntPtrIntIntPtrDelegate>(typeof(DiskExports), nameof(DiskExports.CreateDisk))(path, diskType, name);
    public static int CloseDisk(int handle) => GetDelegate<IntIntDelegate>(typeof(DiskExports), nameof(DiskExports.CloseDisk))(handle);
    public static int GetFileSystemInfo(int handle, IntPtr pointer) => GetDelegate<IntIntPtrDelegate>(typeof(DiskExports), nameof(DiskExports.GetFileSystemInfo))(handle, pointer);
    public static int GetContainerMetadata(int handle, IntPtr pointer) => GetDelegate<IntIntPtrDelegate>(typeof(DiskExports), nameof(DiskExports.GetContainerMetadata))(handle, pointer);
    public static int GetFilesCount(int handle, IntPtr pointer) => GetDelegate<IntIntPtrDelegate>(typeof(DiskExports), nameof(DiskExports.GetFilesCount))(handle, pointer);
    public static int GetFiles(int handle, IntPtr pointer, int capacity) => GetDelegate<IntIntPtrIntDelegate>(typeof(DiskExports), nameof(DiskExports.GetFiles))(handle, pointer, capacity);
    public static int ReadBootArea(int handle, IntPtr pointer, int capacity) => GetDelegate<IntIntPtrIntDelegate>(typeof(DiskExports), nameof(DiskExports.ReadBootArea))(handle, pointer, capacity);
    public static int WriteBootArea(int handle, IntPtr pointer, int length) => GetDelegate<IntIntPtrIntDelegate>(typeof(DiskExports), nameof(DiskExports.WriteBootArea))(handle, pointer, length);
    public static int Format(int handle) => GetDelegate<IntIntDelegate>(typeof(DiskExports), nameof(DiskExports.Format))(handle);
    public static int ReadFile(int handle, IntPtr name, IntPtr buffer, int capacity) => GetDelegate<HandleNameBufferDelegate>(typeof(FileExports), nameof(FileExports.ReadFile))(handle, name, buffer, capacity);
    public static int DeleteFile(int handle, IntPtr name) => GetDelegate<IntIntPtrDelegate>(typeof(FileExports), nameof(FileExports.DeleteFile))(handle, name);
    public static int WriteFile(int handle, IntPtr name, IntPtr data, int length, ushort attributes) => GetDelegate<IntIntPtrIntUshortDelegate>(typeof(FileExports), nameof(FileExports.WriteFile))(handle, name, data, length, attributes);
    public static int RenameFile(int handle, IntPtr oldName, IntPtr newName) => GetDelegate<IntIntPtrIntPtrDelegate>(typeof(FileExports), nameof(FileExports.RenameFile))(handle, oldName, newName);
    public static int UpdateAttributes(int handle, IntPtr name, ushort attributes) => GetDelegate<IntIntPtrUshortDelegate>(typeof(FileExports), nameof(FileExports.UpdateAttributes))(handle, name, attributes);

    private static T GetDelegate<T>(Type type, string methodName) where T : Delegate
    {
        var method = type.GetMethod(methodName) ?? throw new InvalidOperationException($"Method not found: {type.FullName}.{methodName}");
        return Marshal.GetDelegateForFunctionPointer<T>(method.MethodHandle.GetFunctionPointer());
    }
}

internal sealed class NativeFileEntryBufferScope : IDisposable
{
    public NativeFileEntryBufferScope(int capacity)
    {
        Capacity = capacity;
        EntrySize = Marshal.SizeOf<NativeFileEntry>();
        Pointer = Marshal.AllocHGlobal(EntrySize * capacity);
    }

    public int Capacity { get; }
    public int EntrySize { get; }
    public IntPtr Pointer { get; }

    public NativeFileEntry ReadEntry(int index)
    {
        return Marshal.PtrToStructure<NativeFileEntry>(Pointer + (index * EntrySize));
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(Pointer);
    }
}
