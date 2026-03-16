using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.Registry;

namespace Legacy89DiskKit.NativeInterop.Core;

public static class NativeSessionFactory
{
    public static ManagedNativeDiskSession OpenDisk(
        string path,
        bool readOnly,
        IFileSystemRegistry registry)
    {
        var service = new DiskService(null, registry);
        service.OpenDisk(path, readOnly);
        return new ManagedNativeDiskSession(service);
    }

    public static ManagedNativeDiskSession CreateDisk(
        string path,
        DiskType diskType,
        string diskName,
        IFileSystemRegistry registry)
    {
        var service = new DiskService(null, registry);
        service.CreateDisk(path, diskType, diskName);
        return new ManagedNativeDiskSession(service);
    }

    public static ManagedNativeDiskSession FromService(DiskService service)
    {
        return new ManagedNativeDiskSession(service);
    }
}
