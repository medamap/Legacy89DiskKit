using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NativeInteropTestApp;

class Program
{
    private const string LibPath = "/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/csharp/Legacy89DiskKit.NativeInterop/bin/Release/net9.0/osx-arm64/publish/Legacy89DiskKit.NativeInterop.dylib";

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

    [DllImport(LibPath, EntryPoint = "ldk_open_disk", CallingConvention = CallingConvention.Cdecl)]
    public static extern int OpenDisk(string path, bool readOnly);

    [DllImport(LibPath, EntryPoint = "ldk_close_disk", CallingConvention = CallingConvention.Cdecl)]
    public static extern int CloseDisk(int handle);

    [DllImport(LibPath, EntryPoint = "ldk_get_file_system_info", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetFileSystemInfo(int handle, ref NativeFileSystemInfo info);

    [DllImport(LibPath, EntryPoint = "ldk_get_files_count", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetFilesCount(int handle, out int count);

    [DllImport(LibPath, EntryPoint = "ldk_get_files", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetFiles(int handle, IntPtr buffer, int capacity);

    static void Main(string[] args)
    {
        Console.WriteLine("Native Interop Test Application");
        
        string testFile = "/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/CZ8FB01.d88";
        Console.WriteLine($"Opening disk: {testFile}");

        int handle = OpenDisk(testFile, true);
        if (handle <= 0)
        {
            Console.WriteLine($"Failed to open disk. Error code: {handle}");
            return;
        }

        Console.WriteLine($"Disk opened. Handle: {handle}");

        NativeFileSystemInfo info = new NativeFileSystemInfo();
        int res = GetFileSystemInfo(handle, ref info);
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
        res = GetFilesCount(handle, out count);
        if (res == 0)
        {
            Console.WriteLine($"Files count: {count}");
            
            if (count > 0)
            {
                int structSize = Marshal.SizeOf<NativeFileEntry>();
                IntPtr buffer = Marshal.AllocHGlobal(structSize * count);
                try
                {
                    int actualCount = GetFiles(handle, buffer, count);
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

        CloseDisk(handle);
        Console.WriteLine("Disk closed.");
    }
}
