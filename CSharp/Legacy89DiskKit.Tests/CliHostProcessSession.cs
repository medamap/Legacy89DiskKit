using System.Diagnostics;
using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Xunit;

namespace Legacy89DiskKit.Tests;

internal sealed class CliHostProcessSession : IAsyncDisposable
{
    private readonly Process _process;

    public CliHostProcessSession(bool observable = true)
    {
        var cliDllPath = GetRepoPath("CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll");
        Assert.True(File.Exists(cliDllPath), $"CLI assembly was not found: {cliDllPath}");

        var arguments = observable
            ? $"\"{cliDllPath}\" host stdio --observable"
            : $"\"{cliDllPath}\" host stdio";

        _process = new Process
        {
            StartInfo = new ProcessStartInfo("dotnet", arguments)
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = GetRepoPath(string.Empty)
            }
        };

        _process.Start();
    }

    public async Task<EmulatorHostExchange> SendExchangeAsync(
        EmulatorHostRequest request,
        ICollection<HostProofTranscriptEntry>? transcript = null)
    {
        var responseLine = await SendRawLineAsync(EmulatorHostProtocolCodec.SerializeRequest(request));
        var exchange = EmulatorHostProtocolCodec.DeserializeExchange(responseLine);
        transcript?.Add(new HostProofTranscriptEntry(request, exchange));
        return exchange;
    }

    public async Task<EmulatorHostResponse> SendResponseAsync(EmulatorHostRequest request)
    {
        var responseLine = await SendRawLineAsync(EmulatorHostProtocolCodec.SerializeRequest(request));
        return EmulatorHostProtocolCodec.DeserializeResponse(responseLine);
    }

    public async Task<string> SendRawLineAsync(string payload)
    {
        await _process.StandardInput.WriteLineAsync(payload);
        await _process.StandardInput.FlushAsync();

        var responseLine = await _process.StandardOutput.ReadLineAsync();
        Assert.False(string.IsNullOrWhiteSpace(responseLine), "The CLI host process did not produce a response line.");
        return responseLine!;
    }

    public async ValueTask DisposeAsync()
    {
        _process.StandardInput.Close();
        if (!_process.WaitForExit(2000))
        {
            _process.Kill(entireProcessTree: true);
            _process.WaitForExit(2000);
        }

        await Task.CompletedTask;
    }

    private static string GetRepoPath(string relativePath)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../.."));
        return Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
