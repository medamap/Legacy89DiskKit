using Legacy89DiskKit.Native.Application;
using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.FileSystem.Domain.Interface.Registry;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.NativeInterop.Core;
using Legacy89DiskKit.NativeInterop.Types;
using Xunit;
using DiskFileAttributes = Legacy89DiskKit.FileSystem.Domain.Model.FileAttributes;

namespace Legacy89DiskKit.Tests;

[Collection("NativeInterop")]
public class NativeFormatExportsTest
{
    [Fact]
    public void Format_ClearsWrittenFiles()
    {
        HandleManager.Clear();
        NativeBridgeBackend.SetCurrent(new ManagedNativeBridgeBackend());

        using var disk = new TempFormattedDiskScope();
        using var service = CreateDiskService();
        service.OpenDisk(disk.ImagePath, readOnly: false);
        service.FileSystem!.WriteFile("HELLO", [0x01], new ExtendedFileAttributes(DiskFileAttributes.None, 0, false));

        var handle = HandleManager.Register(service.Session!);

        try
        {
            var result = NativeExportInvoker.Format(handle);
            Assert.Equal((int)LdkStatus.Success, result);
            Assert.Empty(service.FileSystem!.GetFiles());
        }
        finally
        {
            HandleManager.Clear();
        }
    }

    [Fact]
    public void Format_ReturnsInvalidHandleForUnknownHandle()
    {
        var result = NativeExportInvoker.Format(-22);
        Assert.Equal((int)LdkStatus.ErrorInvalidHandle, result);
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
