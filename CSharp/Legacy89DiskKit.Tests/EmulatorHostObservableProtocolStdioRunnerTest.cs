using System.Text;
using Legacy89DiskKit.Application;
using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostObservableProtocolStdioRunnerTest
{
    [Fact]
    public async Task Runner_CanEmitNotificationAwareExchanges()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x21 });

        var adapter = Legacy89DiskKitApplication.CreateEventDrivenEmulatorFdcHostAdapter();
        adapter.OpenDisk(0, container);

        var requests = string.Join(Environment.NewLine, new[]
        {
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.SelectDrive, DriveNumber: 0)),
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 1, RegisterValue: 0)),
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 2, RegisterValue: 1)),
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 0, RegisterValue: 0x80)),
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.Advance, AdvanceMicroseconds: 1000))
        }) + Environment.NewLine;

        using var reader = new StringReader(requests);
        var builder = new StringBuilder();
        using var writer = new StringWriter(builder);

        var runner = new EmulatorHostObservableProtocolStdioRunner(
            new EmulatorHostObservableProtocolSession(adapter),
            reader,
            writer);

        await runner.RunAsync();

        var exchanges = builder
            .ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(EmulatorHostProtocolCodec.DeserializeExchange)
            .ToArray();

        Assert.Equal(5, exchanges.Length);
        Assert.Contains(exchanges[3].Notifications, x => x.Kind == EmulatorHostNotificationKind.AdvanceRequested);
        Assert.Contains(exchanges[4].Notifications, x => x.Kind == EmulatorHostNotificationKind.IrqChanged && x.SignalState == true);
        Assert.Contains(exchanges[4].Notifications, x => x.Kind == EmulatorHostNotificationKind.DrqChanged && x.SignalState == true);
    }
}
