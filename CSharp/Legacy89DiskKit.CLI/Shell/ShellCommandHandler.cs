using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using Legacy89DiskKit.CharacterEncoding.Application;
using System.Globalization;

namespace Legacy89DiskKit.CLI.Shell;

public class ShellCommandHandler
{
    private readonly SlotManager _slotManager;
    private readonly CharacterEncodingService _characterEncodingService;
    private readonly IDiskContainerFactory _diskContainerFactory;

    public ShellCommandHandler(SlotManager slotManager, CharacterEncodingService characterEncodingService, IDiskContainerFactory diskContainerFactory)
    {
        _slotManager = slotManager;
        _characterEncodingService = characterEncodingService;
        _diskContainerFactory = diskContainerFactory;
    }

    public void ExecuteCommand(ShellCommand command)
    {
        try
        {
            switch (command.Name)
            {
                case "":
                    break;
                case "help":
                    ShowHelp();
                    break;
                case "slot":
                    HandleSlotCommand(command);
                    break;
                case "slots":
                    ShowAllSlots();
                    break;
                case "open":
                    HandleOpenCommand(command);
                    break;
                case "close":
                    HandleCloseCommand(command);
                    break;
                case "new":
                    HandleNewCommand(command);
                    break;
                case "format":
                    HandleFormatCommand(command);
                    break;
                case "list":
                case "ls":
                    HandleListCommand(command);
                    break;
                case "info":
                    HandleInfoCommand(command);
                    break;
                case "import-text":
                    HandleImportTextCommand(command);
                    break;
                case "export-text":
                    HandleExportTextCommand(command);
                    break;
                case "import-binary":
                    HandleImportBinaryCommand(command);
                    break;
                case "export-binary":
                    HandleExportBinaryCommand(command);
                    break;
                case "delete":
                case "del":
                    HandleDeleteCommand(command);
                    break;
                case "copy":
                case "cp":
                    HandleCopyCommand(command);
                    break;
                case "move":
                case "mv":
                    HandleMoveCommand(command);
                    break;
                case "pwd":
                    HandlePwdCommand(command);
                    break;
                case "cd":
                    HandleCdCommand(command);
                    break;
                case "debug":
                    HandleDebugCommand(command);
                    break;
                case "exit":
                case "quit":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command.Name}");
                    Console.WriteLine("Type 'help' for available commands.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void HandleSlotCommand(ShellCommand command)
    {
        if (command.Arguments.Length == 0)
        {
            Console.WriteLine($"Current slot: {_slotManager.CurrentSlot}");
            Console.WriteLine(_slotManager.CurrentDiskSlot.GetStatusInfo());
            return;
        }

        if (!int.TryParse(command.Arguments[0], out var slotNumber) || slotNumber < 0 || slotNumber >= SlotManager.MaxSlots)
        {
            Console.WriteLine($"Invalid slot number. Use 0-{SlotManager.MaxSlots - 1}");
            return;
        }

        _slotManager.SwitchToSlot(slotNumber);
        Console.WriteLine($"Switched to slot {slotNumber}");
        Console.WriteLine(_slotManager.CurrentDiskSlot.GetStatusInfo());
    }

    private void ShowAllSlots()
    {
        Console.WriteLine("Slot Status:");
        foreach (var slot in _slotManager.GetAllSlots())
        {
            var indicator = slot.SlotNumber == _slotManager.CurrentSlot ? "*" : " ";
            Console.WriteLine($"{indicator}{slot.SlotNumber}: {slot.GetStatusInfo()}");
        }
    }

    private void HandleOpenCommand(ShellCommand command)
    {
        if (command.Arguments.Length == 0)
        {
            Console.WriteLine("Usage: open <disk-file> [slot] [--readonly]");
            return;
        }

        var diskFile = ExpandPath(command.Arguments[0]);
        var slotNumber = _slotManager.CurrentSlot;
        
        if (command.Arguments.Length > 1 && int.TryParse(command.Arguments[1], out var specifiedSlot))
        {
            slotNumber = specifiedSlot;
        }

        var readOnly = command.HasOption("readonly") || command.HasOption("ro");

        if (!File.Exists(diskFile))
        {
            Console.WriteLine($"File not found: {diskFile}");
            return;
        }

        _slotManager.MountDisk(slotNumber, diskFile, readOnly);
        _slotManager.SwitchToSlot(slotNumber);
        Console.WriteLine($"Opened {diskFile} in slot {slotNumber}");
        Console.WriteLine(_slotManager.CurrentDiskSlot.GetStatusInfo());
    }

    private void HandleCloseCommand(ShellCommand command)
    {
        var slotNumber = _slotManager.CurrentSlot;
        
        if (command.Arguments.Length > 0 && int.TryParse(command.Arguments[0], out var specifiedSlot))
        {
            slotNumber = specifiedSlot;
        }

        if (_slotManager.IsSlotEmpty(slotNumber))
        {
            Console.WriteLine($"Slot {slotNumber} is already empty");
            return;
        }

        _slotManager.UnmountSlot(slotNumber);
        Console.WriteLine($"Closed disk in slot {slotNumber}");
    }

    private void HandleNewCommand(ShellCommand command)
    {
        if (command.Arguments.Length < 3)
        {
            Console.WriteLine("Usage: new <disk-file> <type> <name> [slot]");
            Console.WriteLine("Types: 2D, 2DD, 2HD");
            return;
        }

        var diskFile = ExpandPath(command.Arguments[0]);
        var diskTypeStr = command.Arguments[1].ToUpper();
        var diskName = command.Arguments[2];
        var slotNumber = _slotManager.CurrentSlot;

        if (command.Arguments.Length > 3 && int.TryParse(command.Arguments[3], out var specifiedSlot))
        {
            slotNumber = specifiedSlot;
        }

        if (!Enum.TryParse<DiskType>(diskTypeStr.Replace("2", "Two"), out var diskType))
        {
            Console.WriteLine($"Invalid disk type: {diskTypeStr}. Use 2D, 2DD, or 2HD.");
            return;
        }

        _slotManager.CreateNewDisk(slotNumber, diskFile, diskType, diskName);
        _slotManager.SwitchToSlot(slotNumber);
        Console.WriteLine($"Created new disk image: {diskFile} ({diskTypeStr}) \"{diskName}\" in slot {slotNumber}");
    }

    private void HandleFormatCommand(ShellCommand command)
    {
        if (command.Arguments.Length == 0)
        {
            Console.WriteLine("Usage: format <filesystem-type> [slot]");
            Console.WriteLine("Filesystems: hu-basic, fat12, n88-basic, msx-dos, cdos");
            return;
        }

        var fileSystemTypeStr = command.Arguments[0];
        var slotNumber = _slotManager.CurrentSlot;

        if (command.Arguments.Length > 1 && int.TryParse(command.Arguments[1], out var specifiedSlot))
        {
            slotNumber = specifiedSlot;
        }

        if (!Enum.TryParse<FileSystemType>(ConvertFileSystemName(fileSystemTypeStr), true, out var fileSystemType))
        {
            Console.WriteLine($"Invalid filesystem type: {fileSystemTypeStr}");
            Console.WriteLine("Supported types: hu-basic, fat12, n88-basic, msx-dos, cdos");
            return;
        }

        if (_slotManager.IsSlotEmpty(slotNumber))
        {
            Console.WriteLine($"Slot {slotNumber} is empty. Create or open a disk first.");
            return;
        }

        _slotManager.FormatDisk(slotNumber, fileSystemType);
        Console.WriteLine($"Formatted disk in slot {slotNumber} with {fileSystemTypeStr} filesystem");
    }

    private void HandleListCommand(ShellCommand command)
    {
        var slotNumber = _slotManager.CurrentSlot;
        
        if (command.Arguments.Length > 0 && int.TryParse(command.Arguments[0], out var specifiedSlot))
        {
            slotNumber = specifiedSlot;
        }

        var slot = _slotManager.GetSlot(slotNumber);
        if (slot.IsEmpty)
        {
            Console.WriteLine($"Slot {slotNumber} is empty");
            return;
        }

        if (slot.FileSystem == null)
        {
            Console.WriteLine($"Disk in slot {slotNumber} is not formatted");
            return;
        }

        var files = slot.FileSystem.GetFiles();
        Console.WriteLine($"Files in slot {slotNumber} ({slot.GetDisplayName()}):");
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

    private void HandleInfoCommand(ShellCommand command)
    {
        var slotNumber = _slotManager.CurrentSlot;
        
        if (command.Arguments.Length > 0 && int.TryParse(command.Arguments[0], out var specifiedSlot))
        {
            slotNumber = specifiedSlot;
        }

        var slot = _slotManager.GetSlot(slotNumber);
        if (slot.IsEmpty)
        {
            Console.WriteLine($"Slot {slotNumber} is empty");
            return;
        }

        Console.WriteLine($"Disk Information for slot {slotNumber}:");
        Console.WriteLine($"File: {slot.DiskPath}");
        Console.WriteLine($"Container Type: {Path.GetExtension(slot.DiskPath ?? "").ToUpper()}");
        Console.WriteLine($"Disk Type: {slot.Container?.DiskType}");
        Console.WriteLine($"Read-Only: {slot.Container?.IsReadOnly}");
        Console.WriteLine($"Filesystem: {slot.FileSystemType}");
        
        if (slot.FileSystem != null)
        {
            var files = slot.FileSystem.GetFiles();
            Console.WriteLine($"Files: {files.Count()}");
        }
    }

    private void HandleImportTextCommand(ShellCommand command)
    {
        if (command.Arguments.Length < 2)
        {
            Console.WriteLine("Usage: import-text <host-file> <disk-filename> [slot] [--machine <machine>]");
            return;
        }

        var hostFile = ExpandPath(command.Arguments[0]);
        var diskFileName = command.Arguments[1];
        var slotNumber = _slotManager.CurrentSlot;

        if (command.Arguments.Length > 2 && int.TryParse(command.Arguments[2], out var specifiedSlot))
        {
            slotNumber = specifiedSlot;
        }

        var slot = _slotManager.GetSlot(slotNumber);
        if (slot.IsEmpty || slot.FileSystem == null)
        {
            Console.WriteLine($"Slot {slotNumber} is empty or not formatted");
            return;
        }

        if (slot.Container?.IsReadOnly == true)
        {
            Console.WriteLine($"Slot {slotNumber} is mounted read-only");
            return;
        }

        var machineTypeStr = command.GetOption("machine");
        var machineType = GetMachineTypeForFileSystem(slot.FileSystemType ?? "HuBasic", machineTypeStr);

        if (!File.Exists(hostFile))
        {
            Console.WriteLine($"File not found: {hostFile}");
            return;
        }

        var hostText = File.ReadAllText(hostFile);
        var machineData = _characterEncodingService.EncodeText(hostText, machineType);
        
        slot.FileSystem.WriteFile(diskFileName, machineData, isText: true);
        Console.WriteLine($"Imported text file: {hostFile} -> {diskFileName} (machine: {machineType})");
    }

    private void HandleExportTextCommand(ShellCommand command)
    {
        if (command.Arguments.Length < 2)
        {
            Console.WriteLine("Usage: export-text <disk-filename> <host-file> [slot] [--machine <machine>]");
            return;
        }

        var diskFileName = command.Arguments[0];
        var hostFile = ExpandPath(command.Arguments[1]);
        var slotNumber = _slotManager.CurrentSlot;

        if (command.Arguments.Length > 2 && int.TryParse(command.Arguments[2], out var specifiedSlot))
        {
            slotNumber = specifiedSlot;
        }

        var slot = _slotManager.GetSlot(slotNumber);
        if (slot.IsEmpty || slot.FileSystem == null)
        {
            Console.WriteLine($"Slot {slotNumber} is empty or not formatted");
            return;
        }

        var machineTypeStr = command.GetOption("machine");
        var machineType = GetMachineTypeForFileSystem(slot.FileSystemType ?? "HuBasic", machineTypeStr);

        var machineData = slot.FileSystem.ReadFile(diskFileName);
        var unicodeText = _characterEncodingService.DecodeText(machineData, machineType);
        
        File.WriteAllText(hostFile, unicodeText);
        Console.WriteLine($"Exported text file: {diskFileName} -> {hostFile} (machine: {machineType})");
    }

    private void HandleImportBinaryCommand(ShellCommand command)
    {
        if (command.Arguments.Length < 2)
        {
            Console.WriteLine("Usage: import-binary <host-file> <disk-filename> [slot] [load-address] [exec-address]");
            return;
        }

        var hostFile = ExpandPath(command.Arguments[0]);
        var diskFileName = command.Arguments[1];
        var slotNumber = _slotManager.CurrentSlot;

        if (command.Arguments.Length > 2 && int.TryParse(command.Arguments[2], out var specifiedSlot))
        {
            slotNumber = specifiedSlot;
        }

        var slot = _slotManager.GetSlot(slotNumber);
        if (slot.IsEmpty || slot.FileSystem == null)
        {
            Console.WriteLine($"Slot {slotNumber} is empty or not formatted");
            return;
        }

        if (slot.Container?.IsReadOnly == true)
        {
            Console.WriteLine($"Slot {slotNumber} is mounted read-only");
            return;
        }

        ushort loadAddress = 0;
        ushort execAddress = 0;

        if (command.Arguments.Length > 3 && !ushort.TryParse(command.Arguments[3], NumberStyles.HexNumber, null, out loadAddress))
        {
            Console.WriteLine("Invalid load address (use hex format, e.g., 8000)");
            return;
        }

        if (command.Arguments.Length > 4 && !ushort.TryParse(command.Arguments[4], NumberStyles.HexNumber, null, out execAddress))
        {
            Console.WriteLine("Invalid exec address (use hex format, e.g., 8000)");
            return;
        }

        if (!File.Exists(hostFile))
        {
            Console.WriteLine($"File not found: {hostFile}");
            return;
        }

        var binaryData = File.ReadAllBytes(hostFile);
        slot.FileSystem.WriteFile(diskFileName, binaryData, isText: false, loadAddress, execAddress);
        Console.WriteLine($"Imported binary file: {hostFile} -> {diskFileName} (Load: ${loadAddress:X4}, Exec: ${execAddress:X4})");
    }

    private void HandleExportBinaryCommand(ShellCommand command)
    {
        if (command.Arguments.Length < 2)
        {
            Console.WriteLine("Usage: export-binary <disk-filename> <host-file> [slot]");
            return;
        }

        var diskFileName = command.Arguments[0];
        var hostFile = ExpandPath(command.Arguments[1]);
        var slotNumber = _slotManager.CurrentSlot;

        if (command.Arguments.Length > 2 && int.TryParse(command.Arguments[2], out var specifiedSlot))
        {
            slotNumber = specifiedSlot;
        }

        var slot = _slotManager.GetSlot(slotNumber);
        if (slot.IsEmpty || slot.FileSystem == null)
        {
            Console.WriteLine($"Slot {slotNumber} is empty or not formatted");
            return;
        }

        var binaryData = slot.FileSystem.ReadFile(diskFileName);
        File.WriteAllBytes(hostFile, binaryData);
        Console.WriteLine($"Exported binary file: {diskFileName} -> {hostFile}");
    }

    private void HandleDeleteCommand(ShellCommand command)
    {
        if (command.Arguments.Length == 0)
        {
            Console.WriteLine("Usage: delete <disk-filename> [slot]");
            return;
        }

        var diskFileName = command.Arguments[0];
        var slotNumber = _slotManager.CurrentSlot;

        if (command.Arguments.Length > 1 && int.TryParse(command.Arguments[1], out var specifiedSlot))
        {
            slotNumber = specifiedSlot;
        }

        var slot = _slotManager.GetSlot(slotNumber);
        if (slot.IsEmpty || slot.FileSystem == null)
        {
            Console.WriteLine($"Slot {slotNumber} is empty or not formatted");
            return;
        }

        if (slot.Container?.IsReadOnly == true)
        {
            Console.WriteLine($"Slot {slotNumber} is mounted read-only");
            return;
        }

        slot.FileSystem.DeleteFile(diskFileName);
        Console.WriteLine($"Deleted file: {diskFileName}");
    }

    private void HandleCopyCommand(ShellCommand command)
    {
        if (command.Arguments.Length < 2)
        {
            Console.WriteLine("Usage: copy <source> <destination>");
            Console.WriteLine("Format: [slot:]filename or host:path");
            Console.WriteLine("Examples:");
            Console.WriteLine("  copy 0:FILE.TXT 1:");
            Console.WriteLine("  copy 0:FILE.TXT host:/tmp/file.txt");
            Console.WriteLine("  copy host:/tmp/file.txt 1:FILE.TXT");
            return;
        }

        var source = command.Arguments[0];
        var destination = command.Arguments[1];

        HandleFileCopy(source, destination, false);
    }

    private void HandleMoveCommand(ShellCommand command)
    {
        if (command.Arguments.Length < 2)
        {
            Console.WriteLine("Usage: move <source> <destination>");
            Console.WriteLine("Format: [slot:]filename or host:path");
            return;
        }

        var source = command.Arguments[0];
        var destination = command.Arguments[1];

        HandleFileCopy(source, destination, true);
    }

    private void HandleFileCopy(string source, string destination, bool deleteSource)
    {
        var (sourceSlot, sourceFile, isSourceHost) = ParseFileLocation(source);
        var (destSlot, destFile, isDestHost) = ParseFileLocation(destination);

        if (isSourceHost && isDestHost)
        {
            Console.WriteLine("Cannot copy between host files");
            return;
        }

        if (isSourceHost)
        {
            sourceFile = ExpandPath(sourceFile);
            if (!File.Exists(sourceFile))
            {
                Console.WriteLine($"Source file not found: {sourceFile}");
                return;
            }

            var slot = _slotManager.GetSlot(destSlot);
            if (slot.IsEmpty || slot.FileSystem == null)
            {
                Console.WriteLine($"Destination slot {destSlot} is empty or not formatted");
                return;
            }

            if (slot.Container?.IsReadOnly == true)
            {
                Console.WriteLine($"Destination slot {destSlot} is mounted read-only");
                return;
            }

            var data = File.ReadAllBytes(sourceFile);
            slot.FileSystem.WriteFile(destFile, data, isText: false);
            Console.WriteLine($"Copied {sourceFile} -> slot {destSlot}:{destFile}");

            if (deleteSource)
            {
                File.Delete(sourceFile);
                Console.WriteLine($"Deleted source file: {sourceFile}");
            }
        }
        else if (isDestHost)
        {
            var slot = _slotManager.GetSlot(sourceSlot);
            if (slot.IsEmpty || slot.FileSystem == null)
            {
                Console.WriteLine($"Source slot {sourceSlot} is empty or not formatted");
                return;
            }

            var data = slot.FileSystem.ReadFile(sourceFile);
            destFile = ExpandPath(destFile);
            File.WriteAllBytes(destFile, data);
            Console.WriteLine($"Copied slot {sourceSlot}:{sourceFile} -> {destFile}");

            if (deleteSource)
            {
                if (slot.Container?.IsReadOnly == true)
                {
                    Console.WriteLine($"Warning: Cannot delete from read-only slot {sourceSlot}");
                }
                else
                {
                    slot.FileSystem.DeleteFile(sourceFile);
                    Console.WriteLine($"Deleted source file from slot {sourceSlot}: {sourceFile}");
                }
            }
        }
        else
        {
            var sourceSlotObj = _slotManager.GetSlot(sourceSlot);
            var destSlotObj = _slotManager.GetSlot(destSlot);

            if (sourceSlotObj.IsEmpty || sourceSlotObj.FileSystem == null)
            {
                Console.WriteLine($"Source slot {sourceSlot} is empty or not formatted");
                return;
            }

            if (destSlotObj.IsEmpty || destSlotObj.FileSystem == null)
            {
                Console.WriteLine($"Destination slot {destSlot} is empty or not formatted");
                return;
            }

            if (destSlotObj.Container?.IsReadOnly == true)
            {
                Console.WriteLine($"Destination slot {destSlot} is mounted read-only");
                return;
            }

            var data = sourceSlotObj.FileSystem.ReadFile(sourceFile);
            destSlotObj.FileSystem.WriteFile(destFile, data, isText: false);
            Console.WriteLine($"Copied slot {sourceSlot}:{sourceFile} -> slot {destSlot}:{destFile}");

            if (deleteSource)
            {
                if (sourceSlotObj.Container?.IsReadOnly == true)
                {
                    Console.WriteLine($"Warning: Cannot delete from read-only slot {sourceSlot}");
                }
                else
                {
                    sourceSlotObj.FileSystem.DeleteFile(sourceFile);
                    Console.WriteLine($"Deleted source file from slot {sourceSlot}: {sourceFile}");
                }
            }
        }
    }

    private (int slot, string file, bool isHost) ParseFileLocation(string location)
    {
        if (location.StartsWith("host:"))
        {
            return (0, location[5..], true);
        }

        var colonIndex = location.IndexOf(':');
        if (colonIndex > 0 && int.TryParse(location[..colonIndex], out var slot))
        {
            var fileName = location[(colonIndex + 1)..];
            return (slot, fileName, false);
        }

        return (_slotManager.CurrentSlot, location, false);
    }

    private MachineType GetMachineTypeForFileSystem(string fileSystemType, string? machineTypeStr)
    {
        if (machineTypeStr != null)
        {
            if (Enum.TryParse<MachineType>(ConvertMachineName(machineTypeStr), true, out var machineType))
            {
                return machineType;
            }
        }

        return fileSystemType.ToLower() switch
        {
            "hubasic" => MachineType.X1,
            "fat12" => MachineType.Pc8801,
            "n88basic" => MachineType.Pc8801,
            "msxdos" => MachineType.Msx1,
            "cdos" => MachineType.Pc8801,
            _ => MachineType.X1
        };
    }

    private string ConvertFileSystemName(string name)
    {
        return name.ToLower() switch
        {
            "hu-basic" or "hubasic" => "HuBasic",
            "fat12" => "Fat12",
            "fat16" => "Fat16",
            "cpm" => "Cpm",
            "n88-basic" or "n88basic" => "N88Basic",
            "msx-dos" or "msxdos" => "MsxDos",
            "cdos" => "Cdos",
            _ => name
        };
    }

    private string ConvertMachineName(string name)
    {
        return name.ToLower() switch
        {
            "x1" => "X1",
            "x1turbo" or "x1-turbo" => "X1Turbo",
            "pc8801" or "pc-8801" => "Pc8801",
            "pc8801mk2" or "pc-8801mk2" or "pc8801-mk2" => "Pc8801Mk2",
            "msx1" or "msx-1" => "Msx1",
            "msx2" or "msx-2" => "Msx2",
            _ => name
        };
    }

    private string GetModeString(byte mode)
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

    private string FormatDate(DateTime date)
    {
        if (date == DateTime.MinValue)
            return "----/--/--";
        
        return date.ToString("yyyy/MM/dd");
    }

    private void HandlePwdCommand(ShellCommand command)
    {
        Console.WriteLine(Directory.GetCurrentDirectory());
    }

    private void HandleCdCommand(ShellCommand command)
    {
        if (command.Arguments.Length == 0)
        {
            Console.WriteLine("Usage: cd <directory>");
            return;
        }

        var directory = command.Arguments[0];
        
        try
        {
            directory = ExpandPath(directory);
            
            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Directory not found: {directory}");
                return;
            }

            Directory.SetCurrentDirectory(directory);
            Console.WriteLine($"Changed directory to: {directory}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to change directory: {ex.Message}");
        }
    }

    private void HandleDebugCommand(ShellCommand command)
    {
        if (command.Arguments.Length == 0)
        {
            Console.WriteLine("Usage: debug <disk-file>");
            return;
        }

        var diskFile = ExpandPath(command.Arguments[0]);
        if (!File.Exists(diskFile))
        {
            Console.WriteLine($"File not found: {diskFile}");
            return;
        }

        DebugCommand.DumpDiskHeader(diskFile, _diskContainerFactory);
    }

    private string ExpandPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        if (path == "~")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        else if (path.StartsWith("~/"))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path[2..]);
        }
        
        return Path.GetFullPath(path);
    }

    private void ShowHelp()
    {
        Console.WriteLine("Legacy89DiskKit Interactive Shell");
        Console.WriteLine();
        Console.WriteLine("Slot Management:");
        Console.WriteLine("  slot [number]                               Show or switch to slot");
        Console.WriteLine("  slots                                       Show all slots");
        Console.WriteLine("  open <disk-file> [slot] [--readonly]       Open disk image");
        Console.WriteLine("  close [slot]                                Close disk in slot");
        Console.WriteLine();
        Console.WriteLine("Disk Operations:");
        Console.WriteLine("  new <disk-file> <type> <name> [slot]       Create new disk");
        Console.WriteLine("  format <filesystem> [slot]                 Format disk");
        Console.WriteLine("  info [slot]                                 Show disk information");
        Console.WriteLine();
        Console.WriteLine("File Operations:");
        Console.WriteLine("  list [slot]                                 List files");
        Console.WriteLine("  import-text <host-file> <disk-file> [slot] [--machine <type>]");
        Console.WriteLine("  export-text <disk-file> <host-file> [slot] [--machine <type>]");
        Console.WriteLine("  import-binary <host-file> <disk-file> [slot] [load] [exec]");
        Console.WriteLine("  export-binary <disk-file> <host-file> [slot]");
        Console.WriteLine("  delete <disk-file> [slot]                  Delete file");
        Console.WriteLine();
        Console.WriteLine("Cross-Slot Operations:");
        Console.WriteLine("  copy <source> <destination>                Copy file");
        Console.WriteLine("  move <source> <destination>                Move file");
        Console.WriteLine("    Format: [slot:]filename or host:path");
        Console.WriteLine("    Examples: copy 0:FILE.TXT 1:, copy host:/tmp/file.txt 1:FILE.TXT");
        Console.WriteLine();
        Console.WriteLine("Directory Management:");
        Console.WriteLine("  pwd                                         Show current directory");
        Console.WriteLine("  cd <directory>                              Change directory");
        Console.WriteLine("    Note: Supports ~ for home directory");
        Console.WriteLine();
        Console.WriteLine("System:");
        Console.WriteLine("  help                                        Show this help");
        Console.WriteLine("  exit                                        Exit shell");
        Console.WriteLine();
        Console.WriteLine("Disk Types: 2D, 2DD, 2HD");
        Console.WriteLine("Filesystems: hu-basic, fat12, n88-basic, msx-dos, cdos");
        Console.WriteLine("Machine Types: x1, x1turbo, pc8801, pc8801mk2, msx1, msx2");
    }
}