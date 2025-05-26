using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Legacy89DiskKit.DependencyInjection;
using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface.Factory;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using Legacy89DiskKit.CharacterEncoding.Application;
using System.Globalization;

namespace Legacy89DiskKit.CLI;

class Program
{
    private static IDiskContainerFactory _diskContainerFactory = null!;
    private static IFileSystemFactory _fileSystemFactory = null!;
    private static CharacterEncodingService _characterEncodingService = null!;

    static Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        _diskContainerFactory = host.Services.GetRequiredService<IDiskContainerFactory>();
        _fileSystemFactory = host.Services.GetRequiredService<IFileSystemFactory>();
        _characterEncodingService = host.Services.GetRequiredService<CharacterEncodingService>();

        try
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return Task.CompletedTask;
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
        
        return Task.CompletedTask;
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
            Console.WriteLine("Supported types: hu-basic, fat12");
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
            Console.WriteLine("Usage: list <disk-file> [--filesystem <type>]");
            return;
        }

        var diskFile = parameters[0];
        var fileSystemTypeStr = GetFileSystemParameter(parameters);

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile, readOnly: true);
            
            IFileSystem fileSystem;
            if (fileSystemTypeStr != null)
            {
                if (!Enum.TryParse<FileSystemType>(ConvertFileSystemName(fileSystemTypeStr), true, out var type))
                {
                    Console.WriteLine($"Invalid filesystem type: {fileSystemTypeStr}");
                    Console.WriteLine("Supported types: hu-basic, fat12");
                    return;
                }
                fileSystem = _fileSystemFactory.OpenFileSystem(container, type);
            }
            else
            {
                // Use read-only auto-detection for list operation
                fileSystem = _fileSystemFactory.OpenFileSystemReadOnly(container);
            }

            var files = fileSystem.GetFiles();

            Console.WriteLine($"Files in {diskFile}:");
            Console.WriteLine("Name".PadRight(17) + "Size".PadLeft(8) + " Mode  Date");
            Console.WriteLine(new string('-', 40));

            foreach (var file in files)
            {
                var fileName = $"{file.FileName}.{file.Extension}";
                var size = file.Size.ToString();
                var mode = GetModeString((byte)file.Mode);
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
            Console.WriteLine("Usage: import-text <disk-file> <host-file> <disk-filename> --filesystem <type> [--machine <machine>]");
            Console.WriteLine("Required: --filesystem parameter (hu-basic, fat12)");
            Console.WriteLine("Optional: --machine parameter (x1, pc8801, msx1, etc.)");
            return;
        }

        var diskFile = parameters[0];
        var hostFile = parameters[1];
        var diskFileName = parameters[2];
        var fileSystemTypeStr = GetFileSystemParameter(parameters);
        var machineTypeStr = GetMachineParameter(parameters);

        if (fileSystemTypeStr == null)
        {
            Console.WriteLine("Error: --filesystem parameter is required for write operations");
            Console.WriteLine("Supported filesystems: hu-basic, fat12");
            Console.WriteLine("Supported machines: x1, x1turbo, pc8801, pc8801mk2, msx1, msx2");
            Console.WriteLine("");
            Console.WriteLine("To detect filesystem type (read-only):");
            Console.WriteLine($"  ./Legacy89DiskKit info {diskFile}");
            Console.WriteLine("");
            Console.WriteLine("Example with explicit filesystem:");
            Console.WriteLine($"  ./Legacy89DiskKit import-text {diskFile} {hostFile} {diskFileName} --filesystem hu-basic");
            Console.WriteLine($"  ./Legacy89DiskKit import-text {diskFile} {hostFile} {diskFileName} --filesystem hu-basic --machine x1");
            return;
        }

        if (!Enum.TryParse<FileSystemType>(ConvertFileSystemName(fileSystemTypeStr), true, out var fileSystemType))
        {
            Console.WriteLine($"Invalid filesystem type: {fileSystemTypeStr}");
            Console.WriteLine("Supported types: hu-basic, fat12");
            return;
        }

        // Determine machine type
        MachineType machineType;
        if (machineTypeStr != null)
        {
            if (!Enum.TryParse<MachineType>(ConvertMachineName(machineTypeStr), true, out machineType))
            {
                Console.WriteLine($"Invalid machine type: {machineTypeStr}");
                Console.WriteLine("Supported machines: x1, x1turbo, pc8801, pc8801mk2, msx1, msx2, mz80k, mz700, mz1500, mz2500, fm7, fm77, fm77av, pc8001, pc8001mk2, pc6001, pc6601, fc");
                return;
            }
        }
        else
        {
            machineType = GetDefaultMachineType(fileSystemTypeStr);
            Console.WriteLine($"Using default machine type for {fileSystemTypeStr}: {machineType}");
        }

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container, fileSystemType);
            
            var hostText = File.ReadAllText(hostFile);
            var machineData = _characterEncodingService.EncodeText(hostText, machineType);
            
            fileSystem.WriteFile(diskFileName, machineData, isText: true);
            Console.WriteLine($"Imported text file: {hostFile} -> {diskFileName} (machine: {machineType})");
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
            Console.WriteLine("Usage: export-text <disk-file> <disk-filename> <host-file> --filesystem <type> [--machine <machine>]");
            Console.WriteLine("Required: --filesystem parameter (hu-basic, fat12)");
            Console.WriteLine("Optional: --machine parameter (x1, pc8801, msx1, etc.)");
            return;
        }

        var diskFile = parameters[0];
        var diskFileName = parameters[1];
        var hostFile = parameters[2];
        var fileSystemTypeStr = GetFileSystemParameter(parameters);
        var machineTypeStr = GetMachineParameter(parameters);

        if (fileSystemTypeStr == null)
        {
            Console.WriteLine("Error: --filesystem parameter is required for write operations");
            Console.WriteLine("Supported filesystems: hu-basic, fat12");
            Console.WriteLine("Supported machines: x1, x1turbo, pc8801, pc8801mk2, msx1, msx2");
            Console.WriteLine("");
            Console.WriteLine("To detect filesystem type (read-only):");
            Console.WriteLine($"  ./Legacy89DiskKit info {diskFile}");
            Console.WriteLine("");
            Console.WriteLine("Example with explicit filesystem:");
            Console.WriteLine($"  ./Legacy89DiskKit export-text {diskFile} {diskFileName} {hostFile} --filesystem fat12");
            Console.WriteLine($"  ./Legacy89DiskKit export-text {diskFile} {diskFileName} {hostFile} --filesystem hu-basic --machine x1");
            return;
        }

        if (!Enum.TryParse<FileSystemType>(ConvertFileSystemName(fileSystemTypeStr), true, out var fileSystemType))
        {
            Console.WriteLine($"Invalid filesystem type: {fileSystemTypeStr}");
            Console.WriteLine("Supported types: hu-basic, fat12");
            return;
        }

        // Determine machine type
        MachineType machineType;
        if (machineTypeStr != null)
        {
            if (!Enum.TryParse<MachineType>(ConvertMachineName(machineTypeStr), true, out machineType))
            {
                Console.WriteLine($"Invalid machine type: {machineTypeStr}");
                Console.WriteLine("Supported machines: x1, x1turbo, pc8801, pc8801mk2, msx1, msx2, mz80k, mz700, mz1500, mz2500, fm7, fm77, fm77av, pc8001, pc8001mk2, pc6001, pc6601, fc");
                return;
            }
        }
        else
        {
            machineType = GetDefaultMachineType(fileSystemTypeStr);
            Console.WriteLine($"Using default machine type for {fileSystemTypeStr}: {machineType}");
        }

        try
        {
            using var container = _diskContainerFactory.OpenDiskImage(diskFile, readOnly: true);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container, fileSystemType);
            
            var machineData = fileSystem.ReadFile(diskFileName);
            var unicodeText = _characterEncodingService.DecodeText(machineData, machineType);
            
            File.WriteAllText(hostFile, unicodeText);
            Console.WriteLine($"Exported text file: {diskFileName} -> {hostFile} (machine: {machineType})");
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
            var fileSystemType = _fileSystemFactory.GuessFileSystemType(container);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container, fileSystemType);
            
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
            var fileSystemType = _fileSystemFactory.GuessFileSystemType(container);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container, fileSystemType);
            
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
            var fileSystemType = _fileSystemFactory.GuessFileSystemType(container);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container, fileSystemType);
            
            var bootData = File.ReadAllBytes(hostFile);
            var bootSector = new BootSector(
                IsBootable: true,
                Label: bootLabel,
                Extension: "",
                Size: bootData.Length,
                LoadAddress: 0,
                ExecuteAddress: 0,
                ModifiedDate: DateTime.Now,
                StartSector: 0
            );
            fileSystem.WriteBootSector(bootSector);
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
            var fileSystemType = _fileSystemFactory.GuessFileSystemType(container);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container, fileSystemType);
            
            var bootInfo = fileSystem.GetBootSector();
            var output = $"Boot Sector Information:\n" +
                        $"Label: {bootInfo.Label}\n" +
                        $"Load Address: ${bootInfo.LoadAddress:X4}\n" +
                        $"Exec Address: ${bootInfo.ExecuteAddress:X4}\n" +
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
            var fileSystemType = _fileSystemFactory.GuessFileSystemType(container);
            var fileSystem = _fileSystemFactory.OpenFileSystem(container, fileSystemType);
            
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
            
            Console.WriteLine($"Disk Information for: {diskFile}");
            Console.WriteLine($"Container Type: {Path.GetExtension(diskFile).ToUpper()}");
            Console.WriteLine($"Disk Type: {container.DiskType}");
            Console.WriteLine($"Read-Only: {container.IsReadOnly}");
            
            try
            {
                var detectedType = _fileSystemFactory.GuessFileSystemType(container);
                Console.WriteLine($"Detected Filesystem: {detectedType}");
            }
            catch (Exception fsEx)
            {
                Console.WriteLine($"Filesystem Detection: {fsEx.Message}");
            }
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
            // Recovery operations use read-only auto-detection (safe)
            var fileSystem = _fileSystemFactory.OpenFileSystemReadOnly(container);
            
            Console.WriteLine("WARNING: Attempting to recover from damaged disk. Some data may be lost or corrupted.");
            
            var machineData = fileSystem.ReadFile(diskFileName, allowPartialRead: true);
            // Use default machine type for recovery (X1 - most common for legacy disks)
            var unicodeText = _characterEncodingService.DecodeText(machineData, MachineType.X1);
            
            File.WriteAllText(hostFile, unicodeText);
            Console.WriteLine($"Recovered text file: {diskFileName} -> {hostFile} (machine: X1)");
            Console.WriteLine("Please verify the recovered data for completeness and accuracy.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to recover text file: {ex.Message}");
        }
    }

    private static string? GetFileSystemParameter(string[] parameters)
    {
        for (int i = 0; i < parameters.Length - 1; i++)
        {
            if (parameters[i] == "--filesystem" || parameters[i] == "-fs")
            {
                return parameters[i + 1];
            }
        }
        return null;
    }

    private static string? GetMachineParameter(string[] parameters)
    {
        for (int i = 0; i < parameters.Length - 1; i++)
        {
            if (parameters[i] == "--machine" || parameters[i] == "-m")
            {
                return parameters[i + 1];
            }
        }
        return null;
    }

    private static MachineType GetDefaultMachineType(string fileSystemType)
    {
        return fileSystemType?.ToLower() switch
        {
            "hu-basic" => MachineType.X1,
            "fat12" => MachineType.Pc8801,
            _ => MachineType.X1
        };
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
            // Recovery operations use read-only auto-detection (safe)
            var fileSystem = _fileSystemFactory.OpenFileSystemReadOnly(container);
            
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

    private static string ConvertMachineName(string name)
    {
        return name.ToLower() switch
        {
            "x1" => "X1",
            "x1turbo" or "x1-turbo" => "X1Turbo",
            "pc8801" or "pc-8801" => "Pc8801",
            "pc8801mk2" or "pc-8801mk2" or "pc8801-mk2" => "Pc8801Mk2",
            "msx1" or "msx-1" => "Msx1",
            "msx2" or "msx-2" => "Msx2",
            "mz80k" or "mz-80k" => "Mz80k",
            "mz700" or "mz-700" => "Mz700",
            "mz1500" or "mz-1500" => "Mz1500",
            "mz2500" or "mz-2500" => "Mz2500",
            "fm7" => "Fm7",
            "fm77" => "Fm77",
            "fm77av" or "fm77-av" => "Fm77av",
            "pc8001" or "pc-8001" => "Pc8001",
            "pc8001mk2" or "pc-8001mk2" or "pc8001-mk2" => "Pc8001Mk2",
            "pc6001" or "pc-6001" => "Pc6001",
            "pc6601" or "pc-6601" => "Pc6601",
            "fc" or "famicom" or "familycomputer" => "Fc",
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
        Console.WriteLine("  format <disk-file> --filesystem <type>     Format disk with filesystem");
        Console.WriteLine("  list <disk-file> [--filesystem <type>]     List files on disk");
        Console.WriteLine("  import-text <disk-file> <host-file> <name> --filesystem <type> [--machine <machine>]");
        Console.WriteLine("                                              Import text file");
        Console.WriteLine("  export-text <disk-file> <name> <host-file> --filesystem <type> [--machine <machine>]");
        Console.WriteLine("                                              Export text file");
        Console.WriteLine("  import-binary <disk-file> <host-file> <name> [load] [exec] --filesystem <type>");
        Console.WriteLine("                                              Import binary file");
        Console.WriteLine("  export-binary <disk-file> <name> <host-file> --filesystem <type>");
        Console.WriteLine("                                              Export binary file");
        Console.WriteLine("  import-boot <disk-file> <host-file> <label> --filesystem <type>");
        Console.WriteLine("                                              Import boot sector");
        Console.WriteLine("  export-boot <disk-file> <output-file> --filesystem <type>");
        Console.WriteLine("                                              Export boot sector info");
        Console.WriteLine("  delete <disk-file> <name> --filesystem <type>");
        Console.WriteLine("                                              Delete file");
        Console.WriteLine("  info <disk-file>                           Show disk information");
        Console.WriteLine("  recover-text <disk-file> <name> <host-file>");
        Console.WriteLine("                                              Recover damaged text file");
        Console.WriteLine("  recover-binary <disk-file> <name> <host-file>");
        Console.WriteLine("                                              Recover damaged binary file");
        Console.WriteLine("  help                                        Show this help");
        Console.WriteLine();
        Console.WriteLine("Disk Types: 2D, 2DD, 2HD");
        Console.WriteLine("Filesystems: hu-basic (default), fat12");
        Console.WriteLine("Machine Types: x1, x1turbo, pc8801, pc8801mk2, msx1, msx2, mz80k, mz700, mz1500, mz2500, fm7, fm77, fm77av, pc8001, pc8001mk2, pc6001, pc6601, fc");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  Legacy89DiskKit create mydisk.d88 2D \"My Disk\"");
        Console.WriteLine("  Legacy89DiskKit format mydisk.d88 --filesystem hu-basic");
        Console.WriteLine("  Legacy89DiskKit list mydisk.d88");
        Console.WriteLine("  Legacy89DiskKit info mydisk.d88");
        Console.WriteLine("  Legacy89DiskKit import-text mydisk.d88 readme.txt README.TXT --filesystem hu-basic --machine x1");
        Console.WriteLine("  Legacy89DiskKit export-text disk.dsk README.TXT readme.txt --filesystem fat12 --machine pc8801");
    }
}