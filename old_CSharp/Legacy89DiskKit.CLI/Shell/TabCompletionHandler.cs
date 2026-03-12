namespace Legacy89DiskKit.CLI.Shell;

public class TabCompletionHandler
{
    private readonly SlotManager _slotManager;
    
    public TabCompletionHandler(SlotManager slotManager)
    {
        _slotManager = slotManager;
    }

    public string[] GetCompletions(string input, int cursorPosition)
    {
        if (string.IsNullOrWhiteSpace(input) || cursorPosition > input.Length)
            return Array.Empty<string>();

        var beforeCursor = input[..cursorPosition];
        var afterCursor = input[cursorPosition..];
        
        var parts = ShellCommand.Parse(beforeCursor);
        
        if (parts.Name == "" && parts.Arguments.Length == 0)
            return GetCommandCompletions("");
        
        var lastWord = GetLastWord(beforeCursor);
        var prefix = beforeCursor[..^lastWord.Length];

        if (parts.Arguments.Length == 0 && !beforeCursor.EndsWith(' '))
        {
            return GetCommandCompletions(parts.Name);
        }

        // For open command with arguments
        if (beforeCursor.EndsWith(' '))
        {
            var completions = GetArgumentCompletions(parts.Name, parts.Arguments.Length, "");
            return completions.Select(c => prefix + " " + c + afterCursor).ToArray();
        }
        else
        {
            var completions = GetArgumentCompletions(parts.Name, parts.Arguments.Length - 1, lastWord);
            return completions.Select(c => prefix + c + afterCursor).ToArray();
        }
    }

    private string[] GetCommandCompletions(string prefix)
    {
        var commands = new[]
        {
            "help", "slot", "slots", "open", "close", "new", "format",
            "list", "ls", "info", "import-text", "export-text", 
            "import-binary", "export-binary", "delete", "del",
            "copy", "cp", "move", "mv", "pwd", "cd", "exit", "quit"
        };

        return commands.Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                      .OrderBy(c => c)
                      .ToArray();
    }

    private string[] GetArgumentCompletions(string command, int argumentIndex, string prefix)
    {
        
        switch (command.ToLower())
        {
            case "open":
            case "import-text":
            case "import-binary":
            case "new":
                if (argumentIndex == 0 || (command == "new" && argumentIndex == 0))
                    return GetFilePathCompletions(prefix);
                break;
                
            case "export-text":
            case "export-binary":
                if (argumentIndex == 1)
                    return GetFilePathCompletions(prefix);
                else if (argumentIndex == 0)
                    return GetDiskFileCompletions(prefix);
                break;
                
            case "cd":
                if (argumentIndex == 0)
                    return GetDirectoryCompletions(prefix);
                break;
                
            case "format":
                if (argumentIndex == 0)
                    return GetFileSystemTypeCompletions(prefix);
                break;
                
            case "copy":
            case "cp":
            case "move":
            case "mv":
                return GetCopyMoveCompletions(prefix);
                
            case "delete":
            case "del":
                if (argumentIndex == 0)
                    return GetDiskFileCompletions(prefix);
                break;
        }

        return Array.Empty<string>();
    }

    private string[] GetFilePathCompletions(string prefix)
    {
        try
        {
            var expandedPrefix = ExpandPath(prefix);
            
            var directory = Path.GetDirectoryName(expandedPrefix);
            if (string.IsNullOrEmpty(directory))
            {
                directory = expandedPrefix;
                expandedPrefix = Path.Combine(expandedPrefix, "");
            }
            
            var filePrefix = Path.GetFileName(expandedPrefix);

            if (!Directory.Exists(directory))
            {
                return Array.Empty<string>();
            }

            var entries = new List<string>();
            
            foreach (var dir in Directory.GetDirectories(directory))
            {
                var name = Path.GetFileName(dir);
                if (string.IsNullOrEmpty(filePrefix) || name.StartsWith(filePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var relativePath = GetRelativePath(prefix, dir);
                    entries.Add(relativePath + "/");
                }
            }
            
            foreach (var file in Directory.GetFiles(directory))
            {
                var name = Path.GetFileName(file);
                if (string.IsNullOrEmpty(filePrefix) || name.StartsWith(filePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var relativePath = GetRelativePath(prefix, file);
                    entries.Add(relativePath);
                }
            }

            return entries.OrderBy(e => e).ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private string[] GetDirectoryCompletions(string prefix)
    {
        try
        {
            var expandedPrefix = ExpandPath(prefix);
            var directory = Path.GetDirectoryName(expandedPrefix) ?? ".";
            var dirPrefix = Path.GetFileName(expandedPrefix);

            if (!Directory.Exists(directory))
                return Array.Empty<string>();

            var entries = new List<string>();
            
            foreach (var dir in Directory.GetDirectories(directory))
            {
                var name = Path.GetFileName(dir);
                if (name.StartsWith(dirPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var relativePath = GetRelativePath(prefix, dir);
                    entries.Add(relativePath + "/");
                }
            }

            return entries.OrderBy(e => e).ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private string[] GetDiskFileCompletions(string prefix)
    {
        var slot = _slotManager.CurrentDiskSlot;
        if (slot.IsEmpty || slot.FileSystem == null)
            return Array.Empty<string>();

        try
        {
            var files = slot.FileSystem.GetFiles();
            var fileNames = files.Select(f => $"{f.FileName}.{f.Extension}".TrimEnd('.'))
                                .Where(f => f.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                .OrderBy(f => f)
                                .ToArray();
            return fileNames;
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private string[] GetFileSystemTypeCompletions(string prefix)
    {
        var types = new[] { "hu-basic", "fat12", "n88-basic", "msx-dos", "cdos" };
        return types.Where(t => t.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                   .OrderBy(t => t)
                   .ToArray();
    }

    private string[] GetCopyMoveCompletions(string prefix)
    {
        if (prefix.Contains(':'))
        {
            var colonIndex = prefix.IndexOf(':');
            var slotPart = prefix[..colonIndex];
            var filePart = prefix[(colonIndex + 1)..];
            
            if (int.TryParse(slotPart, out var slotNumber) && slotNumber >= 0 && slotNumber < SlotManager.MaxSlots)
            {
                var slot = _slotManager.GetSlot(slotNumber);
                if (!slot.IsEmpty && slot.FileSystem != null)
                {
                    try
                    {
                        var files = slot.FileSystem.GetFiles();
                        var fileNames = files.Select(f => $"{f.FileName}.{f.Extension}".TrimEnd('.'))
                                            .Where(f => f.StartsWith(filePart, StringComparison.OrdinalIgnoreCase))
                                            .Select(f => $"{slotNumber}:{f}")
                                            .OrderBy(f => f)
                                            .ToArray();
                        return fileNames;
                    }
                    catch { }
                }
            }
        }
        else if (prefix.StartsWith("host:"))
        {
            var pathPart = prefix[5..];
            var completions = GetFilePathCompletions(pathPart);
            return completions.Select(c => "host:" + c).ToArray();
        }
        else
        {
            var results = new List<string>();
            
            for (int i = 0; i < SlotManager.MaxSlots; i++)
            {
                if (!_slotManager.IsSlotEmpty(i) && i.ToString().StartsWith(prefix))
                {
                    results.Add($"{i}:");
                }
            }
            
            if ("host:".StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                results.Add("host:");
            }
            
            var diskFiles = GetDiskFileCompletions(prefix);
            results.AddRange(diskFiles);
            
            return results.OrderBy(r => r).ToArray();
        }

        return Array.Empty<string>();
    }

    private string GetLastWord(string input)
    {
        var lastSpace = input.LastIndexOf(' ');
        return lastSpace >= 0 ? input[(lastSpace + 1)..] : input;
    }

    private string GetRelativePath(string prefix, string fullPath)
    {
        if (prefix.StartsWith("~/"))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (fullPath.StartsWith(home))
            {
                return "~/" + fullPath[(home.Length + 1)..].Replace('\\', '/');
            }
        }
        
        var prefixDir = Path.GetDirectoryName(prefix);
        if (!string.IsNullOrEmpty(prefixDir))
        {
            return Path.Combine(prefixDir, Path.GetFileName(fullPath)).Replace('\\', '/');
        }
        
        return Path.GetFileName(fullPath);
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
}