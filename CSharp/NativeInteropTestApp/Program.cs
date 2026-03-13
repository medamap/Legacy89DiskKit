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
    private delegate int OpenDiskDelegate(IntPtr path, bool readOnly);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int CloseDiskDelegate(int handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetFileSystemInfoDelegate(int handle, ref NativeFileSystemInfo info);

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
        var openDisk = LoadDelegate<OpenDiskDelegate>(libraryHandle, "ldk_open_disk");
        var closeDisk = LoadDelegate<CloseDiskDelegate>(libraryHandle, "ldk_close_disk");
        var getFileSystemInfo = LoadDelegate<GetFileSystemInfoDelegate>(libraryHandle, "ldk_get_file_system_info");
        var getFilesCount = LoadDelegate<GetFilesCountDelegate>(libraryHandle, "ldk_get_files_count");
        var getFiles = LoadDelegate<GetFilesDelegate>(libraryHandle, "ldk_get_files");

        Console.WriteLine("Native Interop Test Application");
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
        NativeLibrary.Free(libraryHandle);
        Console.WriteLine("Disk closed.");
        return 0;
    }

    private static T LoadDelegate<T>(IntPtr libraryHandle, string exportName) where T : Delegate
    {
        var symbol = NativeLibrary.GetExport(libraryHandle, exportName);
        return Marshal.GetDelegateForFunctionPointer<T>(symbol);
    }
}
