using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Legacy89DiskKit.DependencyInjection;
using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Infrastructure.Utility;
using System.Globalization;

namespace Legacy89DiskKit.CLI;

class Program
{
    private static IDiskContainerFactory _diskContainerFactory = null!;
    private static IFileSystemFactory _fileSystemFactory = null!;

    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        _diskContainerFactory = host.Services.GetRequiredService<IDiskContainerFactory>();
        _fileSystemFactory = host.Services.GetRequiredService<IFileSystemFactory>();

        try
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var command = args[0].ToLower();
            var parameters = args.Skip(1).ToArray();

            switch (command)
            {
                case "create":
                    CreateDiskImage(parameters);
                    break;
                case "format":
                    FormatDiskImage(parameters);
                    break;
                case "list":
                    ListFiles(parameters);
                    break;
                case "import-text":
                    ImportTextFile(parameters);
                    break;
                case "export-text":
                    ExportTextFile(parameters);
                    break;
                case "import-binary":
                    ImportBinaryFile(parameters);
                    break;
                case "export-binary":
                    ExportBinaryFile(parameters);
                    break;
                case "import-boot":
                    ImportBootSector(parameters);
                    break;
                case "export-boot":
                    ExportBootSector(parameters);
                    break;
                case "delete":
                    DeleteFile(parameters);
                    break;
                case "info":
                    ShowDiskInfo(parameters);
                    break;
                case "recover-text":
                    RecoverTextFile(parameters);
                    break;
                case "recover-binary":
                    RecoverBinaryFile(parameters);
                    break;
                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    Console.WriteLine("Use 'help' to see available commands.");
                    Environment.Exit(1);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddLegacy89DiskKit();
            });

    private static void CreateDiskImage(string[] parameters)
    {
        if (parameters.Length < 3)
        {
            Console.WriteLine("Usage: create <disk-file> <type> <disk-name>");
            Console.WriteLine("Types: 2D, 2DD, 2HD");
            return;
        }

        var diskFile = parameters[0];
        var diskTypeStr = parameters[1].ToUpper();
        var diskName = parameters[2];

        if (!Enum.TryParse<DiskType>(diskTypeStr.Replace("2", "Two"), out var diskType))
        {
            Console.WriteLine($"Invalid disk type: {diskTypeStr}. Use 2D, 2DD, or 2HD.");
            return;
        }

        try
        {
            using var container = _diskContainerFactory.CreateNewDiskImage(diskFile, diskType, diskName);
            Console.WriteLine($"Created new disk image: {diskFile} ({diskTypeStr}) \"{diskName}\"");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create disk image: {ex.Message}");
        }
    }

    private static void FormatDiskImage(string[] parameters)
    {
        if (parameters.Length < 1)
        {
            Console.WriteLine("Usage: format <disk-file> [filesystem-type]");
            return;
        }

        var diskFile = parameters[0];
        var fileSystemTypeStr = parameters.Length > 1 ? parameters[1] : "hu-basic";

        if (!Enum.TryParse<FileSystemType>(ConvertFileSystemName(fileSystemTypeStr), true, out var fileSystemType))
        {
            Console.WriteLine($"Invalid filesystem type: {fileSystemTypeStr}");
            Console.WriteLine("Supported types: hu-basic");
            return;
        }

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile);
            var fileSystem = _fileSystemFactory.CreateFileSystem(container, fileSystemType);
            fileSystem.Format();
            Console.WriteLine($"Formatted disk: {diskFile} with {fileSystemTypeStr} filesystem");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to format disk: {ex.Message}");
        }
    }

    private static void ListFiles(string[] parameters)
    {
        if (parameters.Length < 1)
        {
            Console.WriteLine("Usage: list <disk-file> [filesystem-type]");
            return;
        }

        var diskFile = parameters[0];
        var fileSystemTypeStr = parameters.Length > 1 ? parameters[1] : null;

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile, readOnly: true);
            
            FileSystemType? fileSystemType = null;
            if (fileSystemTypeStr != null)
            {
                if (!Enum.TryParse<FileSystemType>(ConvertFileSystemName(fileSystemTypeStr), true, out var type))
                {
                    Console.WriteLine($"Invalid filesystem type: {fileSystemTypeStr}");
                    return;
                }
                fileSystemType = type;
            }

            var fileSystem = _fileSystemFactory.OpenFileSystem(container, fileSystemType);
            var files = fileSystem.ListFiles();

            Console.WriteLine($"Files in {diskFile}:");
            Console.WriteLine("Name".PadRight(17) + "Size".PadLeft(8) + " Mode  Date");
            Console.WriteLine(new string('-', 40));

            foreach (var file in files)
            {
                var fileName = $"{file.FileName}.{file.Extension}";
                var size = file.Size.ToString();
                var mode = GetModeString(file.Mode);
                var date = FormatDate(file.ModifiedDate);
                
                Console.WriteLine($"{fileName.PadRight(17)}{size.PadLeft(8)} {mode} {date}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to list files: {ex.Message}");
        }
    }

    private static void ImportTextFile(string[] parameters)
    {
        if (parameters.Length < 3)
        {
            Console.WriteLine("Usage: import-text <disk-file> <host-file> <disk-filename>");
            return;
        }

        var diskFile = parameters[0];
        var hostFile = parameters[1];
        var diskFileName = parameters[2];

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container);
            
            var hostText = File.ReadAllText(hostFile);
            var converter = new X1Converter();
            var x1Text = converter.ToX1(hostText);
            
            fileSystem.WriteFile(diskFileName, x1Text, isText: true);
            Console.WriteLine($"Imported text file: {hostFile} -> {diskFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to import text file: {ex.Message}");
        }
    }

    private static void ExportTextFile(string[] parameters)
    {
        if (parameters.Length < 3)
        {
            Console.WriteLine("Usage: export-text <disk-file> <disk-filename> <host-file>");
            return;
        }

        var diskFile = parameters[0];
        var diskFileName = parameters[1];
        var hostFile = parameters[2];

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile, readOnly: true);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container);
            
            var x1Data = fileSystem.ReadFile(diskFileName);
            var converter = new X1Converter();
            var unicodeText = converter.ToUnicode(x1Data);
            
            File.WriteAllText(hostFile, unicodeText);
            Console.WriteLine($"Exported text file: {diskFileName} -> {hostFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to export text file: {ex.Message}");
        }
    }

    private static void ImportBinaryFile(string[] parameters)
    {
        if (parameters.Length < 3)
        {
            Console.WriteLine("Usage: import-binary <disk-file> <host-file> <disk-filename> [load-address] [exec-address]");
            return;
        }

        var diskFile = parameters[0];
        var hostFile = parameters[1];
        var diskFileName = parameters[2];
        
        ushort loadAddress = 0;
        ushort execAddress = 0;
        
        if (parameters.Length > 3 && !ushort.TryParse(parameters[3], NumberStyles.HexNumber, null, out loadAddress))
        {
            Console.WriteLine("Invalid load address (use hex format, e.g., 8000)");
            return;
        }
        
        if (parameters.Length > 4 && !ushort.TryParse(parameters[4], NumberStyles.HexNumber, null, out execAddress))
        {
            Console.WriteLine("Invalid exec address (use hex format, e.g., 8000)");
            return;
        }

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container);
            
            var binaryData = File.ReadAllBytes(hostFile);
            fileSystem.WriteFile(diskFileName, binaryData, isText: false, loadAddress, execAddress);
            Console.WriteLine($"Imported binary file: {hostFile} -> {diskFileName} (Load: ${loadAddress:X4}, Exec: ${execAddress:X4})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to import binary file: {ex.Message}");
        }
    }

    private static void ExportBinaryFile(string[] parameters)
    {
        if (parameters.Length < 3)
        {
            Console.WriteLine("Usage: export-binary <disk-file> <disk-filename> <host-file>");
            return;
        }

        var diskFile = parameters[0];
        var diskFileName = parameters[1];
        var hostFile = parameters[2];

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile, readOnly: true);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container);
            
            var binaryData = fileSystem.ReadFile(diskFileName);
            File.WriteAllBytes(hostFile, binaryData);
            Console.WriteLine($"Exported binary file: {diskFileName} -> {hostFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to export binary file: {ex.Message}");
        }
    }

    private static void ImportBootSector(string[] parameters)
    {
        if (parameters.Length < 3)
        {
            Console.WriteLine("Usage: import-boot <disk-file> <host-file> <boot-label>");
            return;
        }

        var diskFile = parameters[0];
        var hostFile = parameters[1];
        var bootLabel = parameters[2];

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container);
            
            var bootData = File.ReadAllBytes(hostFile);
            fileSystem.WriteBootSector(bootLabel, bootData);
            Console.WriteLine($"Imported boot sector: {hostFile} -> \"{bootLabel}\"");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to import boot sector: {ex.Message}");
        }
    }

    private static void ExportBootSector(string[] parameters)
    {
        if (parameters.Length < 2)
        {
            Console.WriteLine("Usage: export-boot <disk-file> <output-file>");
            return;
        }

        var diskFile = parameters[0];
        var outputFile = parameters[1];

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile, readOnly: true);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container);
            
            var bootInfo = fileSystem.ReadBootSector();
            var output = $"Boot Sector Information:\n" +
                        $"Label: {bootInfo.Label}\n" +
                        $"Load Address: ${bootInfo.LoadAddress:X4}\n" +
                        $"Exec Address: ${bootInfo.ExecAddress:X4}\n" +
                        $"Size: {bootInfo.Size} bytes\n" +
                        $"Date: {FormatDate(bootInfo.ModifiedDate)}\n";
            
            File.WriteAllText(outputFile, output);
            Console.WriteLine($"Exported boot sector info: {outputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to export boot sector: {ex.Message}");
        }
    }

    private static void DeleteFile(string[] parameters)
    {
        if (parameters.Length < 2)
        {
            Console.WriteLine("Usage: delete <disk-file> <disk-filename>");
            return;
        }

        var diskFile = parameters[0];
        var diskFileName = parameters[1];

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container);
            
            fileSystem.DeleteFile(diskFileName);
            Console.WriteLine($"Deleted file: {diskFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete file: {ex.Message}");
        }
    }

    private static void ShowDiskInfo(string[] parameters)
    {
        if (parameters.Length < 1)
        {
            Console.WriteLine("Usage: info <disk-file>");
            return;
        }

        var diskFile = parameters[0];

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile, readOnly: true);
            var detectedType = _fileSystemFactory.DetectFileSystemType(container);
            
            Console.WriteLine($"Disk Information for: {diskFile}");
            Console.WriteLine($"Container Type: {Path.GetExtension(diskFile).ToUpper()}");
            Console.WriteLine($"Disk Type: {container.DiskType}");
            Console.WriteLine($"Detected Filesystem: {detectedType}");
            Console.WriteLine($"Read-Only: {container.IsReadOnly}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get disk info: {ex.Message}");
        }
    }

    private static void RecoverTextFile(string[] parameters)
    {
        if (parameters.Length < 3)
        {
            Console.WriteLine("Usage: recover-text <disk-file> <disk-filename> <host-file>");
            return;
        }

        var diskFile = parameters[0];
        var diskFileName = parameters[1];
        var hostFile = parameters[2];

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile, readOnly: true);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container);
            
            Console.WriteLine("WARNING: Attempting to recover from damaged disk. Some data may be lost or corrupted.");
            
            var x1Data = fileSystem.ReadFile(diskFileName, allowPartialRead: true);
            var converter = new X1Converter();
            var unicodeText = converter.ToUnicode(x1Data);
            
            File.WriteAllText(hostFile, unicodeText);
            Console.WriteLine($"Recovered text file: {diskFileName} -> {hostFile}");
            Console.WriteLine("Please verify the recovered data for completeness and accuracy.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to recover text file: {ex.Message}");
        }
    }

    private static void RecoverBinaryFile(string[] parameters)
    {
        if (parameters.Length < 3)
        {
            Console.WriteLine("Usage: recover-binary <disk-file> <disk-filename> <host-file>");
            return;
        }

        var diskFile = parameters[0];
        var diskFileName = parameters[1];
        var hostFile = parameters[2];

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile, readOnly: true);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container);
            
            Console.WriteLine("WARNING: Attempting to recover from damaged disk. Some data may be lost or corrupted.");
            
            var binaryData = fileSystem.ReadFile(diskFileName, allowPartialRead: true);
            File.WriteAllBytes(hostFile, binaryData);
            Console.WriteLine($"Recovered binary file: {diskFileName} -> {hostFile}");
            Console.WriteLine("Please verify the recovered data for completeness and accuracy.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to recover binary file: {ex.Message}");
        }
    }

    private static string ConvertFileSystemName(string name)
    {
        return name.ToLower() switch
        {
            "hu-basic" or "hubasic" => "HuBasic",
            "fat12" => "Fat12",
            "fat16" => "Fat16", 
            "cpm" => "Cpm",
            "n88-basic" or "n88basic" => "N88Basic",
            "msx-dos" or "msxdos" => "MsxDos",
            _ => name
        };
    }

    private static string GetModeString(byte mode)
    {
        var result = "";
        if ((mode & 0x01) != 0) result += "BIN";
        else if ((mode & 0x02) != 0) result += "BAS";
        else if ((mode & 0x04) != 0 || (mode & 0x08) != 0) result += "ASC";
        else result += "---";
        
        if ((mode & 0x40) != 0) result += " R";
        if ((mode & 0x10) != 0) result += " H";
        
        return result.PadRight(5);
    }

    private static string FormatDate(DateTime date)
    {
        if (date == DateTime.MinValue)
            return "----/--/--";
        
        return date.ToString("yyyy/MM/dd");
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Legacy89DiskKit CLI - Retro Computer Disk Image Tool");
        Console.WriteLine();
        Console.WriteLine("Usage: Legacy89DiskKit <command> [parameters]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  create <disk-file> <type> <name>           Create new disk image");
        Console.WriteLine("  format <disk-file> [filesystem]            Format disk with filesystem");
        Console.WriteLine("  list <disk-file> [filesystem]              List files on disk");
        Console.WriteLine("  import-text <disk-file> <host-file> <name> Import text file");
        Console.WriteLine("  export-text <disk-file> <name> <host-file> Export text file");
        Console.WriteLine("  import-binary <disk-file> <host-file> <name> [load] [exec]");
        Console.WriteLine("                                              Import binary file");
        Console.WriteLine("  export-binary <disk-file> <name> <host-file>");
        Console.WriteLine("                                              Export binary file");
        Console.WriteLine("  import-boot <disk-file> <host-file> <label>");
        Console.WriteLine("                                              Import boot sector");
        Console.WriteLine("  export-boot <disk-file> <output-file>      Export boot sector info");
        Console.WriteLine("  delete <disk-file> <name>                  Delete file");
        Console.WriteLine("  info <disk-file>                           Show disk information");
        Console.WriteLine("  recover-text <disk-file> <name> <host-file>");
        Console.WriteLine("                                              Recover damaged text file");
        Console.WriteLine("  recover-binary <disk-file> <name> <host-file>");
        Console.WriteLine("                                              Recover damaged binary file");
        Console.WriteLine("  help                                        Show this help");
        Console.WriteLine();
        Console.WriteLine("Disk Types: 2D, 2DD, 2HD");
        Console.WriteLine("Filesystems: hu-basic (default)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  Legacy89DiskKit create mydisk.d88 2D \"My Disk\"");
        Console.WriteLine("  Legacy89DiskKit format mydisk.d88");
        Console.WriteLine("  Legacy89DiskKit list mydisk.d88");
        Console.WriteLine("  Legacy89DiskKit import-text mydisk.d88 readme.txt README.TXT");
    }
}