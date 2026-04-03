using System.Diagnostics;
using Xunit;

namespace Legacy89DiskKit.Tests;

internal sealed record CliCommandResult(
    int ExitCode,
    string StandardOutput,
    string StandardError);

internal static class CliCommandRunner
{
    public static async Task<CliCommandResult> RunAsync(params string[] arguments)
    {
        return await RunAsync(arguments, null);
    }

    public static async Task<CliCommandResult> RunAsync(
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string?>? environmentVariables)
    {
        var cliDllPath = GetRepoPath("CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll");
        Assert.True(File.Exists(cliDllPath), $"CLI assembly was not found: {cliDllPath}");

        var escapedArguments = string.Join(
            " ",
            new[] { $"\"{cliDllPath}\"" }.Concat(arguments.Select(EscapeArgument)));

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo("dotnet", escapedArguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = GetRepoPath(string.Empty)
            }
        };

        if (environmentVariables != null)
        {
            foreach (var pair in environmentVariables)
            {
                if (pair.Value == null)
                {
                    process.StartInfo.Environment.Remove(pair.Key);
                }
                else
                {
                    process.StartInfo.Environment[pair.Key] = pair.Value;
                }
            }
        }

        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new CliCommandResult(process.ExitCode, stdout, stderr);
    }

    private static string EscapeArgument(string value)
    {
        return value.Contains(' ', StringComparison.Ordinal)
            ? $"\"{value}\""
            : value;
    }

    private static string GetRepoPath(string relativePath)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../.."));
        return Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
