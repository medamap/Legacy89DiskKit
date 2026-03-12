using System.Text;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;
using Legacy89DiskKit.Infrastructure.FileSystem.Msx;
using Legacy89DiskKit.Infrastructure.FileSystem.Pc88;

namespace Legacy89DiskKit.Application.FileSystem;

public sealed class ExplicitFileSystemResolver
{
    public string GetCanonicalName(string fileSystemName)
    {
        return Normalize(fileSystemName) switch
        {
            "hubasic" => "Hu-BASIC",
            "n88basic" => "N88-BASIC",
            "msxdos" => "MSX-DOS",
            _ => throw new InvalidOperationException($"Unsupported file system: {fileSystemName}")
        };
    }

    public IFileSystem Create(string fileSystemName, IDiskContainer container)
    {
        return Normalize(fileSystemName) switch
        {
            "hubasic" => new HuBasicFileSystem(container),
            "n88basic" => CreateN88Basic(container),
            "msxdos" => CreateMsxDos(container),
            _ => throw new InvalidOperationException($"Unsupported file system: {fileSystemName}")
        };
    }

    public void InitializeForDetection(IFileSystem fileSystem)
    {
        if (fileSystem.GetFileSystemInfo().FileSystemName == "Hu-BASIC")
        {
            var bootArea = fileSystem.ReadBootArea();
            if (bootArea.Length == 0)
            {
                return;
            }

            Array.Clear(bootArea, 0, Math.Min(32, bootArea.Length));
            var signature = Encoding.ASCII.GetBytes("Hu-BASIC");
            Array.Copy(signature, 0, bootArea, 1, Math.Min(signature.Length, Math.Max(0, bootArea.Length - 1)));
            fileSystem.WriteBootArea(bootArea);
        }
    }

    private static IFileSystem CreateN88Basic(IDiskContainer container)
    {
        if (container.DiskType == DiskType.TwoHD)
        {
            throw new InvalidOperationException("N88-BASIC currently supports only 2D and 2DD media.");
        }

        return new N88BasicFileSystem(container);
    }

    private static IFileSystem CreateMsxDos(IDiskContainer container)
    {
        if (container.DiskType != DiskType.TwoDD)
        {
            throw new InvalidOperationException("MSX-DOS currently supports only 2DD media in the CLI create/format flow.");
        }

        return new MsxDosFileSystem(container);
    }

    private static string Normalize(string fileSystemName)
    {
        if (string.IsNullOrWhiteSpace(fileSystemName))
        {
            throw new InvalidOperationException("File system is required.");
        }

        return new string(fileSystemName.Trim().ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
    }
}
