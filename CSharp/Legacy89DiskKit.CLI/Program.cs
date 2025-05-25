using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Exception;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Infrastructure.Utility;
using System.Globalization;

namespace Legacy89DiskKit.CLI;

class Program
{
    private static readonly DiskImageService _diskImageService = new();
    private static readonly FileSystemService _fileSystemService = new();

    static void Main(string[] args)
    {
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
                case "recover-text":
                    RecoverTextFile(parameters);
                    break;
                case "recover-binary":
                    RecoverBinaryFile(parameters);
                    break;
                case "info":
                    ShowDiskInfo(parameters);
                    break;
                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowHelp();
                    break;
            }
        }
        catch (InvalidDiskFormatException ex)
        {
            Console.WriteLine($"Invalid disk format: {ex.Message}");
            Environment.Exit(2);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File not found: {ex.Message}");
            Environment.Exit(3);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied: {ex.Message}");
            Environment.Exit(4);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error: {ex.Message}");
            Environment.Exit(5);
        }
        catch (OutOfMemoryException ex)
        {
            Console.WriteLine($"Out of memory: {ex.Message}");
            Environment.Exit(6);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Invalid argument: {ex.Message}");
            Environment.Exit(7);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"  Inner exception: {ex.InnerException.Message}");
            Environment.Exit(1);
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Legacy89DiskKit CLI - D88 Disk Image Tool for Hu-BASIC File System");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  create <disk-file> <type> [disk-name]    Create new D88 disk image");
        Console.WriteLine("  format <disk-file>                       Format disk with Hu-BASIC file system");
        Console.WriteLine("  list <disk-file>                         List files on disk");
        Console.WriteLine("  import-text <disk-file> <host-file> <disk-filename>");
        Console.WriteLine("  export-text <disk-file> <disk-filename> <host-file>");
        Console.WriteLine("  import-binary <disk-file> <host-file> <disk-filename> [load-addr] [exec-addr]");
        Console.WriteLine("  export-binary <disk-file> <disk-filename> <host-file>");
        Console.WriteLine("  import-boot <disk-file> <host-file> [label]");
        Console.WriteLine("  export-boot <disk-file> <host-file>");
        Console.WriteLine("  delete <disk-file> <disk-filename>");
        Console.WriteLine("  recover-text <disk-file> <disk-filename> <host-file>    Recover corrupted text file");
        Console.WriteLine("  recover-binary <disk-file> <disk-filename> <host-file>  Recover corrupted binary file");
        Console.WriteLine("  info <disk-file>                         Show disk information");
        Console.WriteLine();
        Console.WriteLine("Disk types: 2D, 2DD, 2HD");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  Legacy89DiskKit.CLI create disk.d88 2D \"My Disk\"");
        Console.WriteLine("  Legacy89DiskKit.CLI format disk.d88");
        Console.WriteLine("  Legacy89DiskKit.CLI import-text disk.d88 readme.txt README.TXT");
        Console.WriteLine("  Legacy89DiskKit.CLI list disk.d88");
    }

    static void CreateDiskImage(string[] parameters)
    {
        if (parameters.Length < 2)
        {
            Console.WriteLine("Usage: create <disk-file> <type> [disk-name]");
            return;
        }

        var diskFile = parameters[0];
        var diskTypeStr = parameters[1].ToUpper();
        var diskName = parameters.Length > 2 ? parameters[2] : "";

        var diskType = diskTypeStr switch
        {
            "2D" => DiskType.TwoD,
            "2DD" => DiskType.TwoDD,
            "2HD" => DiskType.TwoHD,
            _ => (DiskType?)null
        };

        if (diskType == null)
        {
            Console.WriteLine("Invalid disk type. Use: 2D, 2DD, or 2HD");
            return;
        }

        if (File.Exists(diskFile))
        {
            Console.Write($"File {diskFile} already exists. Overwrite? (y/N): ");
            var response = Console.ReadLine();
            if (response?.ToLower() != "y")
            {
                Console.WriteLine("Cancelled.");
                return;
            }
        }

        using var container = _diskImageService.CreateNewDiskImage(diskFile, diskType.Value, diskName);
        Console.WriteLine($"Created {diskTypeStr} disk image: {diskFile}");
    }

    static void FormatDiskImage(string[] parameters)
    {
        if (parameters.Length < 1)
        {
            Console.WriteLine("Usage: format <disk-file>");
            return;
        }

        var diskFile = parameters[0];
        using var container = _diskImageService.OpenDiskImage(diskFile);
        _fileSystemService.FormatDisk(container);
        Console.WriteLine($"Formatted disk: {diskFile}");
    }

    static void ListFiles(string[] parameters)
    {
        if (parameters.Length < 1)
        {
            Console.WriteLine("Usage: list <disk-file>");
            return;
        }

        var diskFile = parameters[0];
        using var container = _diskImageService.OpenDiskImage(diskFile, true);
        var fileSystem = _fileSystemService.OpenFileSystem(container);

        if (!fileSystem.IsFormatted)
        {
            Console.WriteLine("Disk is not formatted with Hu-BASIC file system.");
            return;
        }

        var files = _fileSystemService.ListFiles(fileSystem);
        var fileList = files.ToList();

        if (!fileList.Any())
        {
            Console.WriteLine("No files found.");
            return;
        }

        Console.WriteLine("Filename         Ext  Type   Size   Load   Exec   Modified");
        Console.WriteLine("---------------- ---- ------ ------ ------ ------ --------");

        foreach (var file in fileList)
        {
            var typeStr = GetFileTypeString(file);
            var protectedStr = file.IsProtected ? "*" : " ";
            
            Console.WriteLine($"{file.FileName,-16} {file.Extension,-4} {typeStr,-6} {file.Size,6} {file.LoadAddress:X4}   {file.ExecuteAddress:X4}   {file.ModifiedDate:yyyy-MM-dd}{protectedStr}");
        }

        var fsInfo = _fileSystemService.GetFileSystemInfo(fileSystem);
        Console.WriteLine();
        Console.WriteLine($"Files: {fileList.Count}");
        Console.WriteLine($"Free space: {fsInfo.FreeClusters * fsInfo.ClusterSize} bytes ({fsInfo.FreeClusters}/{fsInfo.TotalClusters} clusters)");
    }

    static string GetFileTypeString(FileEntry file)
    {
        if (file.Attributes.IsBinary) return "BIN";
        if (file.Attributes.IsBasic) return "BASIC";
        if (file.Attributes.IsAscii) return "ASCII";
        return "UNK";
    }

    static void ImportTextFile(string[] parameters)
    {
        if (parameters.Length < 3)
        {
            Console.WriteLine("Usage: import-text <disk-file> <host-file> <disk-filename>");
            return;
        }

        var diskFile = parameters[0];
        var hostFile = parameters[1];
        var diskFileName = parameters[2];

        // パラメータ検証
        if (string.IsNullOrWhiteSpace(diskFile))
        {
            Console.WriteLine("Error: Disk file path cannot be empty");
            return;
        }

        if (string.IsNullOrWhiteSpace(hostFile))
        {
            Console.WriteLine("Error: Host file path cannot be empty");
            return;
        }

        if (!HuBasicFileNameValidator.IsValidFileName(diskFileName))
        {
            Console.WriteLine($"Error: Invalid disk filename '{diskFileName}'");
            Console.WriteLine("Valid filenames: 1-13 characters, no spaces or special characters");
            
            var suggested = HuBasicFileNameValidator.CreateValidFileName(diskFileName);
            Console.WriteLine($"Suggested filename: {suggested}");
            return;
        }

        if (!File.Exists(hostFile))
        {
            Console.WriteLine($"Host file not found: {hostFile}");
            return;
        }

        // ファイルサイズチェック
        var hostFileInfo = new FileInfo(hostFile);
        if (hostFileInfo.Length > 65535)
        {
            Console.WriteLine($"Host file too large: {hostFileInfo.Length:N0} bytes (max: 65,535)");
            return;
        }

        if (hostFileInfo.Length > 10 * 1024 * 1024)
        {
            Console.WriteLine($"Host file too large for processing: {hostFileInfo.Length:N0} bytes (max: 10MB)");
            return;
        }

        try
        {
            using var container = _diskImageService.OpenDiskImage(diskFile);
            var fileSystem = _fileSystemService.OpenFileSystem(container);

            if (!fileSystem.IsFormatted)
            {
                Console.WriteLine("Disk is not formatted. Use 'format' command first.");
                return;
            }

            // 既存ファイルチェック
            var existingFile = fileSystem.GetFile(diskFileName);
            if (existingFile != null)
            {
                Console.Write($"File '{diskFileName}' already exists. Overwrite? (y/N): ");
                var response = Console.ReadLine();
                if (response?.ToLower() != "y")
                {
                    Console.WriteLine("Import cancelled.");
                    return;
                }
            }

            _fileSystemService.ImportTextFile(fileSystem, hostFile, diskFileName);
            Console.WriteLine($"Imported text file: {hostFile} -> {diskFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to import text file: {ex.Message}");
        }
    }

    static void ExportTextFile(string[] parameters)
    {
        if (parameters.Length < 3)
        {
            Console.WriteLine("Usage: export-text <disk-file> <disk-filename> <host-file>");
            return;
        }

        var diskFile = parameters[0];
        var diskFileName = parameters[1];
        var hostFile = parameters[2];

        using var container = _diskImageService.OpenDiskImage(diskFile, true);
        var fileSystem = _fileSystemService.OpenFileSystem(container);

        if (!fileSystem.IsFormatted)
        {
            Console.WriteLine("Disk is not formatted with Hu-BASIC file system.");
            return;
        }

        _fileSystemService.ExportTextFile(fileSystem, diskFileName, hostFile);
        Console.WriteLine($"Exported text file: {diskFileName} -> {hostFile}");
    }

    static void ImportBinaryFile(string[] parameters)
    {
        if (parameters.Length < 3)
        {
            Console.WriteLine("Usage: import-binary <disk-file> <host-file> <disk-filename> [load-addr] [exec-addr]");
            return;
        }

        var diskFile = parameters[0];
        var hostFile = parameters[1];
        var diskFileName = parameters[2];

        // パラメータ検証
        if (string.IsNullOrWhiteSpace(diskFile))
        {
            Console.WriteLine("Error: Disk file path cannot be empty");
            return;
        }

        if (string.IsNullOrWhiteSpace(hostFile))
        {
            Console.WriteLine("Error: Host file path cannot be empty");
            return;
        }

        if (!HuBasicFileNameValidator.IsValidFileName(diskFileName))
        {
            Console.WriteLine($"Error: Invalid disk filename '{diskFileName}'");
            Console.WriteLine("Valid filenames: 1-13 characters, no spaces or special characters");
            
            var suggested = HuBasicFileNameValidator.CreateValidFileName(diskFileName);
            Console.WriteLine($"Suggested filename: {suggested}");
            return;
        }

        ushort loadAddr = 0;
        ushort execAddr = 0;

        if (parameters.Length > 3)
        {
            if (!ushort.TryParse(parameters[3], NumberStyles.HexNumber, null, out loadAddr))
            {
                Console.WriteLine("Invalid load address format. Use hexadecimal (e.g., 8000)");
                return;
            }
            
            try
            {
                HuBasicFileNameValidator.ValidateAddress(loadAddr, "Load");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Warning: {ex.Message}");
                Console.Write("Continue anyway? (y/N): ");
                var response = Console.ReadLine();
                if (response?.ToLower() != "y")
                {
                    Console.WriteLine("Import cancelled.");
                    return;
                }
            }
        }

        if (parameters.Length > 4)
        {
            if (!ushort.TryParse(parameters[4], NumberStyles.HexNumber, null, out execAddr))
            {
                Console.WriteLine("Invalid execute address format. Use hexadecimal (e.g., 8000)");
                return;
            }
            
            try
            {
                HuBasicFileNameValidator.ValidateLoadExecuteAddresses(loadAddr, execAddr);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Warning: {ex.Message}");
                Console.Write("Continue anyway? (y/N): ");
                var response = Console.ReadLine();
                if (response?.ToLower() != "y")
                {
                    Console.WriteLine("Import cancelled.");
                    return;
                }
            }
        }

        if (!File.Exists(hostFile))
        {
            Console.WriteLine($"Host file not found: {hostFile}");
            return;
        }

        // ファイルサイズチェック
        var hostFileInfo = new FileInfo(hostFile);
        if (hostFileInfo.Length > 65535)
        {
            Console.WriteLine($"Host file too large: {hostFileInfo.Length:N0} bytes (max: 65,535)");
            return;
        }

        try
        {
            using var container = _diskImageService.OpenDiskImage(diskFile);
            var fileSystem = _fileSystemService.OpenFileSystem(container);

            if (!fileSystem.IsFormatted)
            {
                Console.WriteLine("Disk is not formatted. Use 'format' command first.");
                return;
            }

            // 既存ファイルチェック
            var existingFile = fileSystem.GetFile(diskFileName);
            if (existingFile != null)
            {
                Console.Write($"File '{diskFileName}' already exists. Overwrite? (y/N): ");
                var response = Console.ReadLine();
                if (response?.ToLower() != "y")
                {
                    Console.WriteLine("Import cancelled.");
                    return;
                }
            }

            _fileSystemService.ImportBinaryFile(fileSystem, hostFile, diskFileName, loadAddr, execAddr);
            Console.WriteLine($"Imported binary file: {hostFile} -> {diskFileName} (Load: {loadAddr:X4}, Exec: {execAddr:X4})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to import binary file: {ex.Message}");
        }
    }

    static void ExportBinaryFile(string[] parameters)
    {
        if (parameters.Length < 3)
        {
            Console.WriteLine("Usage: export-binary <disk-file> <disk-filename> <host-file>");
            return;
        }

        var diskFile = parameters[0];
        var diskFileName = parameters[1];
        var hostFile = parameters[2];

        using var container = _diskImageService.OpenDiskImage(diskFile, true);
        var fileSystem = _fileSystemService.OpenFileSystem(container);

        if (!fileSystem.IsFormatted)
        {
            Console.WriteLine("Disk is not formatted with Hu-BASIC file system.");
            return;
        }

        _fileSystemService.ExportBinaryFile(fileSystem, diskFileName, hostFile);
        Console.WriteLine($"Exported binary file: {diskFileName} -> {hostFile}");
    }

    static void ImportBootSector(string[] parameters)
    {
        if (parameters.Length < 2)
        {
            Console.WriteLine("Usage: import-boot <disk-file> <host-file> [label]");
            return;
        }

        var diskFile = parameters[0];
        var hostFile = parameters[1];
        var label = parameters.Length > 2 ? parameters[2] : "BOOT";

        if (!File.Exists(hostFile))
        {
            Console.WriteLine($"Host file not found: {hostFile}");
            return;
        }

        using var container = _diskImageService.OpenDiskImage(diskFile);
        var fileSystem = _fileSystemService.OpenFileSystem(container);

        _fileSystemService.ImportBootSector(fileSystem, hostFile, label);
        Console.WriteLine($"Imported boot sector: {hostFile}");
    }

    static void ExportBootSector(string[] parameters)
    {
        if (parameters.Length < 2)
        {
            Console.WriteLine("Usage: export-boot <disk-file> <host-file>");
            return;
        }

        var diskFile = parameters[0];
        var hostFile = parameters[1];

        using var container = _diskImageService.OpenDiskImage(diskFile, true);
        var fileSystem = _fileSystemService.OpenFileSystem(container);

        _fileSystemService.ExportBootSector(fileSystem, hostFile);
        Console.WriteLine($"Exported boot sector info: {hostFile}");
    }

    static void DeleteFile(string[] parameters)
    {
        if (parameters.Length < 2)
        {
            Console.WriteLine("Usage: delete <disk-file> <disk-filename>");
            return;
        }

        var diskFile = parameters[0];
        var diskFileName = parameters[1];

        using var container = _diskImageService.OpenDiskImage(diskFile);
        var fileSystem = _fileSystemService.OpenFileSystem(container);

        if (!fileSystem.IsFormatted)
        {
            Console.WriteLine("Disk is not formatted with Hu-BASIC file system.");
            return;
        }

        _fileSystemService.DeleteFile(fileSystem, diskFileName);
        Console.WriteLine($"Deleted file: {diskFileName}");
    }

    static void ShowDiskInfo(string[] parameters)
    {
        if (parameters.Length < 1)
        {
            Console.WriteLine("Usage: info <disk-file>");
            return;
        }

        var diskFile = parameters[0];
        using var container = _diskImageService.OpenDiskImage(diskFile, true);

        Console.WriteLine($"Disk Image: {diskFile}");
        Console.WriteLine($"Type: {container.DiskType}");
        Console.WriteLine($"Read-only: {container.IsReadOnly}");

        var sectors = container.GetAllSectors().ToList();
        Console.WriteLine($"Total sectors: {sectors.Count}");

        var fileSystem = _fileSystemService.OpenFileSystem(container);
        if (fileSystem.IsFormatted)
        {
            Console.WriteLine("File System: Hu-BASIC (formatted)");
            var fsInfo = _fileSystemService.GetFileSystemInfo(fileSystem);
            Console.WriteLine($"Cluster size: {fsInfo.ClusterSize} bytes");
            Console.WriteLine($"Total clusters: {fsInfo.TotalClusters}");
            Console.WriteLine($"Free clusters: {fsInfo.FreeClusters}");
            Console.WriteLine($"Free space: {fsInfo.FreeClusters * fsInfo.ClusterSize} bytes");

            var files = _fileSystemService.ListFiles(fileSystem).ToList();
            Console.WriteLine($"Files: {files.Count}");
        }
        else
        {
            Console.WriteLine("File System: Not formatted or unknown");
        }
    }

    static void RecoverTextFile(string[] parameters)
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
            using var container = _diskImageService.OpenDiskImage(diskFile, true);
            var fileSystem = _fileSystemService.OpenFileSystem(container);

            if (!fileSystem.IsFormatted)
            {
                Console.WriteLine("Disk is not formatted with Hu-BASIC file system.");
                return;
            }

            Console.WriteLine($"Attempting to recover text file: {diskFileName}");
            Console.WriteLine("Warning: This may produce partial or corrupted data.");
            Console.WriteLine();

            _fileSystemService.ExportTextFile(fileSystem, diskFileName, hostFile, allowPartialRead: true);
            Console.WriteLine($"Recovery attempt completed: {diskFileName} -> {hostFile}");
            Console.WriteLine("Please verify the recovered file contents manually.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Recovery failed: {ex.Message}");
        }
    }

    static void RecoverBinaryFile(string[] parameters)
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
            using var container = _diskImageService.OpenDiskImage(diskFile, true);
            var fileSystem = _fileSystemService.OpenFileSystem(container);

            if (!fileSystem.IsFormatted)
            {
                Console.WriteLine("Disk is not formatted with Hu-BASIC file system.");
                return;
            }

            Console.WriteLine($"Attempting to recover binary file: {diskFileName}");
            Console.WriteLine("Warning: This may produce partial or corrupted data.");
            Console.WriteLine("Corrupted sectors will be replaced with zeros or default patterns.");
            Console.WriteLine();

            _fileSystemService.ExportBinaryFile(fileSystem, diskFileName, hostFile, allowPartialRead: true);
            Console.WriteLine($"Recovery attempt completed: {diskFileName} -> {hostFile}");
            Console.WriteLine("Please verify the recovered file contents manually.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Recovery failed: {ex.Message}");
        }
    }
}