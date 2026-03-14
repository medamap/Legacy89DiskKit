using System.Diagnostics;
using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostCliProcessBridgeTest
{
    private static string GetRepoPath(string relativePath)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../.."));
        return Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    [Fact]
    public async Task CliHostStdioObservable_CanServeReadOnlyD88Flow()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x41, 0x42 });

        var imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");
        await File.WriteAllBytesAsync(imagePath, container.ToImageData());

        var cliDllPath = GetRepoPath("csharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll");
        Assert.True(File.Exists(cliDllPath), $"CLI assembly was not found: {cliDllPath}");

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo("dotnet", $"\"{cliDllPath}\" host stdio --observable")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = GetRepoPath(string.Empty)
            }
        };

        process.Start();

        try
        {
            var sequence = HostProofSequence.CreateReadOnlyD88ByPathSequence(imagePath);

            var capabilities = await SendExchangeAsync(process, sequence[0]);
            Assert.NotNull(capabilities.Response.Capabilities);
            Assert.True(capabilities.Response.Capabilities!.SupportsObservableStdio);
            Assert.True(capabilities.Response.Capabilities.SupportsPathOpen);

            var openExchange = await SendExchangeAsync(process, sequence[1]);
            Assert.NotNull(openExchange.Response.VisibleState);

            await SendExchangeAsync(process, sequence[2]);
            await SendExchangeAsync(process, sequence[3]);
            await SendExchangeAsync(process, sequence[4]);

            var commandExchange = await SendExchangeAsync(process, sequence[5]);
            Assert.Contains(commandExchange.Notifications, x => x.Kind == EmulatorHostNotificationKind.AdvanceRequested);

            var advanceExchange = await SendExchangeAsync(process, sequence[6]);
            Assert.True(advanceExchange.Response.IrqAsserted);
            Assert.True(advanceExchange.Response.DrqAsserted);

            var firstByte = await SendExchangeAsync(process, sequence[7]);
            var secondByte = await SendExchangeAsync(process, sequence[8]);

            Assert.Equal((byte?)0x41, firstByte.Response.RegisterValue);
            Assert.Equal((byte?)0x42, secondByte.Response.RegisterValue);

            var closeExchange = await SendExchangeAsync(process, sequence[9]);
            Assert.Null(closeExchange.Response.VisibleState);
        }
        finally
        {
            process.StandardInput.Close();
            if (!process.WaitForExit(2000))
            {
                process.Kill(entireProcessTree: true);
            }

            File.Delete(imagePath);
        }
    }

    private static async Task<EmulatorHostExchange> SendExchangeAsync(Process process, EmulatorHostRequest request)
    {
        var payload = EmulatorHostProtocolCodec.SerializeRequest(request);
        await process.StandardInput.WriteLineAsync(payload);
        await process.StandardInput.FlushAsync();

        var responseLine = await process.StandardOutput.ReadLineAsync();
        Assert.False(string.IsNullOrWhiteSpace(responseLine), "The CLI host process did not produce a response line.");
        return EmulatorHostProtocolCodec.DeserializeExchange(responseLine!);
    }
}
