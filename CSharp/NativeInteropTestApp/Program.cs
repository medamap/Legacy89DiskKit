using System;
using System.Runtime.InteropServices;

namespace NativeInteropTestApp;

class Program
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeFileSystemInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FileSystemName;
        public long TotalCapacity;
        public long FreeSpace;
        public int ClusterSize;
        public int ReservedSectors;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string PlatformId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeDiskContainerMetadata
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string ImageFormat;
        public int DiskType;
        public int Cylinders;
        public int Heads;
        public int SectorsPerTrack;
        public int BytesPerSector;
        public int IsWriteProtected;
        public long DeclaredImageSize;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeFileEntry
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string FileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string Extension;
        public int Size;
        public ushort LoadAddress;
        public ushort ExecutionAddress;
        public ushort Attributes;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetAbiVersionDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetCapabilityFlagsDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetCapabilitySummaryDelegate(IntPtr buffer, int capacity);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetStatusNameDelegate(int statusCode, IntPtr buffer, int capacity);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int IsHandleValidDelegate(int handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetOpenHandleCountDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int CloseAllHandlesDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int OpenDiskDelegate(IntPtr path, bool readOnly);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int CloseDiskDelegate(int handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetFileSystemInfoDelegate(int handle, ref NativeFileSystemInfo info);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetContainerMetadataDelegate(int handle, ref NativeDiskContainerMetadata metadata);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetFilesCountDelegate(int handle, out int count);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetFilesDelegate(int handle, IntPtr buffer, int capacity);

    static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Usage: NativeInteropTestApp <library-path> <disk-image-path>");
            return 1;
        }

        string libPath = Path.GetFullPath(args[0]);
        string diskImagePath = Path.GetFullPath(args[1]);

        if (!File.Exists(libPath))
        {
            Console.Error.WriteLine($"Native library not found: {libPath}");
            return 2;
        }

        if (!File.Exists(diskImagePath))
        {
            Console.Error.WriteLine($"Disk image not found: {diskImagePath}");
            return 3;
        }

        IntPtr libraryHandle = NativeLibrary.Load(libPath);
        var getAbiVersion = LoadDelegate<GetAbiVersionDelegate>(libraryHandle, "ldk_get_abi_version");
        var getCapabilityFlags = LoadDelegate<GetCapabilityFlagsDelegate>(libraryHandle, "ldk_get_capability_flags");
        var getCapabilitySummary = LoadDelegate<GetCapabilitySummaryDelegate>(libraryHandle, "ldk_get_capability_summary");
        var getStatusName = LoadDelegate<GetStatusNameDelegate>(libraryHandle, "ldk_get_status_name");
        var isHandleValid = LoadDelegate<IsHandleValidDelegate>(libraryHandle, "ldk_is_handle_valid");
        var getOpenHandleCount = LoadDelegate<GetOpenHandleCountDelegate>(libraryHandle, "ldk_get_open_handle_count");
        var closeAllHandles = LoadDelegate<CloseAllHandlesDelegate>(libraryHandle, "ldk_close_all_handles");
        var openDisk = LoadDelegate<OpenDiskDelegate>(libraryHandle, "ldk_open_disk");
        var closeDisk = LoadDelegate<CloseDiskDelegate>(libraryHandle, "ldk_close_disk");
        var getFileSystemInfo = LoadDelegate<GetFileSystemInfoDelegate>(libraryHandle, "ldk_get_file_system_info");
        var getContainerMetadata = LoadDelegate<GetContainerMetadataDelegate>(libraryHandle, "ldk_get_container_metadata");
        var getFilesCount = LoadDelegate<GetFilesCountDelegate>(libraryHandle, "ldk_get_files_count");
        var getFiles = LoadDelegate<GetFilesDelegate>(libraryHandle, "ldk_get_files");

        Console.WriteLine("Native Interop Test Application");
        Console.WriteLine($"ABI Version: {getAbiVersion()}");
        Console.WriteLine($"Capability Flags: 0x{getCapabilityFlags():X}");
        Console.WriteLine($"Capability Summary: {ReadString(getCapabilitySummary)}");
        Console.WriteLine($"Success Status Name: {ReadStatusName(getStatusName, 0)}");
        Console.WriteLine($"Opening disk: {diskImagePath}");

        IntPtr diskPathPtr = Marshal.StringToCoTaskMemUTF8(diskImagePath);
        int handle;
        try
        {
            handle = openDisk(diskPathPtr, true);
        }
        finally
        {
            Marshal.FreeCoTaskMem(diskPathPtr);
        }

        if (handle <= 0)
        {
            Console.WriteLine($"Failed to open disk. Error code: {handle}");
            NativeLibrary.Free(libraryHandle);
            return 4;
        }

        Console.WriteLine($"Disk opened. Handle: {handle}");
        Console.WriteLine($"Handle valid: {isHandleValid(handle) != 0}");
        Console.WriteLine($"Open handle count: {getOpenHandleCount()}");

        NativeFileSystemInfo info = new NativeFileSystemInfo();
        int res = getFileSystemInfo(handle, ref info);
        if (res == 0)
        {
            Console.WriteLine($"--- File System Info ---");
            Console.WriteLine($"Name: {info.FileSystemName}");
            Console.WriteLine($"Platform: {info.PlatformId}");
            Console.WriteLine($"Capacity: {info.TotalCapacity}");
            Console.WriteLine($"Free Space: {info.FreeSpace}");
        }
        else
        {
            Console.WriteLine($"Failed to get file system info. Error code: {res}");
        }

        NativeDiskContainerMetadata metadata = new NativeDiskContainerMetadata();
        res = getContainerMetadata(handle, ref metadata);
        if (res == 0)
        {
            Console.WriteLine($"--- Container Metadata ---");
            Console.WriteLine($"Format: {metadata.ImageFormat}");
            Console.WriteLine($"Disk Type: {metadata.DiskType}");
            Console.WriteLine($"Geometry: {metadata.Cylinders}/{metadata.Heads}/{metadata.SectorsPerTrack}/{metadata.BytesPerSector}");
            Console.WriteLine($"Write Protected: {metadata.IsWriteProtected != 0}");
            Console.WriteLine($"Declared Size: {metadata.DeclaredImageSize}");
        }
        else
        {
            Console.WriteLine($"Failed to get container metadata. Error code: {res}");
        }

        int count;
        res = getFilesCount(handle, out count);
        if (res == 0)
        {
            Console.WriteLine($"Files count: {count}");
            
            if (count > 0)
            {
                int structSize = Marshal.SizeOf<NativeFileEntry>();
                IntPtr buffer = Marshal.AllocHGlobal(structSize * count);
                try
                {
                    int actualCount = getFiles(handle, buffer, count);
                    Console.WriteLine($"Retrieved {actualCount} files:");
                    
                    for (int i = 0; i < actualCount; i++)
                    {
                        IntPtr current = buffer + (i * structSize);
                        NativeFileEntry entry = Marshal.PtrToStructure<NativeFileEntry>(current);
                        Console.WriteLine($"- {entry.FileName}.{entry.Extension} ({entry.Size} bytes, Attr: {entry.Attributes:X2})");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }
        else
        {
            Console.WriteLine($"Failed to get files count. Error code: {res}");
        }

        closeDisk(handle);
        Console.WriteLine($"Handle valid after close: {isHandleValid(handle) != 0}");
        Console.WriteLine($"Open handle count after close: {getOpenHandleCount()}");
        Console.WriteLine($"Close all handles result: {closeAllHandles()}");
        Console.WriteLine($"Open handle count after reset: {getOpenHandleCount()}");
        NativeLibrary.Free(libraryHandle);
        Console.WriteLine("Disk closed.");
        return 0;
    }

    private static T LoadDelegate<T>(IntPtr libraryHandle, string exportName) where T : Delegate
    {
        var symbol = NativeLibrary.GetExport(libraryHandle, exportName);
        return Marshal.GetDelegateForFunctionPointer<T>(symbol);
    }

    private static string ReadString(GetCapabilitySummaryDelegate reader)
    {
        IntPtr buffer = Marshal.AllocHGlobal(256);
        try
        {
            int length = reader(buffer, 256);
            return Marshal.PtrToStringUTF8(buffer, length) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static string ReadStatusName(GetStatusNameDelegate reader, int statusCode)
    {
        IntPtr buffer = Marshal.AllocHGlobal(256);
        try
        {
            int length = reader(statusCode, buffer, 256);
            return Marshal.PtrToStringUTF8(buffer, length) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
