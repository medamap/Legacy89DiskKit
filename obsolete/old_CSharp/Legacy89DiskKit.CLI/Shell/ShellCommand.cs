namespace Legacy89DiskKit.CLI.Shell;

public class ShellCommand
{
    public string Name { get; }
    public string[] Arguments { get; }
    public Dictionary<string, string> Options { get; }

    public ShellCommand(string name, string[] arguments, Dictionary<string, string> options)
    {
        Name = name;
        Arguments = arguments;
        Options = options;
    }

    public string? GetOption(string key)
    {
        return Options.TryGetValue(key, out var value) ? value : null;
    }

    public bool HasOption(string key)
    {
        return Options.ContainsKey(key);
    }

    public string GetArgument(int index, string defaultValue = "")
    {
        return index < Arguments.Length ? Arguments[index] : defaultValue;
    }

    public static ShellCommand Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new ShellCommand("", Array.Empty<string>(), new Dictionary<string, string>());
        }

        var parts = SplitCommandLine(input.Trim());
        if (parts.Length == 0)
        {
            return new ShellCommand("", Array.Empty<string>(), new Dictionary<string, string>());
        }

        var commandName = parts[0].ToLower();
        var arguments = new List<string>();
        var options = new Dictionary<string, string>();

        for (int i = 1; i < parts.Length; i++)
        {
            var part = parts[i];
            
            if (part.StartsWith("--"))
            {
                var optionName = part[2..];
                string? optionValue = null;
                
                if (i + 1 < parts.Length && !parts[i + 1].StartsWith("-"))
                {
                    optionValue = parts[i + 1];
                    i++;
                }
                
                options[optionName] = optionValue ?? "true";
            }
            else if (part.StartsWith("-") && part.Length > 1)
            {
                var optionName = part[1..];
                string? optionValue = null;
                
                if (i + 1 < parts.Length && !parts[i + 1].StartsWith("-"))
                {
                    optionValue = parts[i + 1];
                    i++;
                }
                
                options[optionName] = optionValue ?? "true";
            }
            else
            {
                arguments.Add(part);
            }
        }

        return new ShellCommand(commandName, arguments.ToArray(), options);
    }

    private static string[] SplitCommandLine(string commandLine)
    {
        var parts = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;
        var escaped = false;

        for (int i = 0; i < commandLine.Length; i++)
        {
            var c = commandLine[i];

            if (escaped)
            {
                current.Append(c);
                escaped = false;
            }
            else if (c == '\\')
            {
                escaped = true;
            }
            else if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            parts.Add(current.ToString());
        }

        return parts.ToArray();
    }
}