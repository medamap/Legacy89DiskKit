using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostObservableProtocolSessionTest
{
    [Fact]
    public void Session_CanReturnResponseWithNotifications()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x41 });

        var adapter = Legacy89DiskKitApplication.CreateEventDrivenEmulatorFdcHostAdapter();
        adapter.OpenDisk(0, container);
        var session = new EmulatorHostObservableProtocolSession(adapter);

        session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.SelectDrive, DriveNumber: 0)));
        session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 1, RegisterValue: 0)));
        session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 2, RegisterValue: 1)));

        var payload = session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 0, RegisterValue: 0x80)));
        var exchange = EmulatorHostProtocolCodec.DeserializeExchange(payload);

        Assert.True(exchange.Response.VisibleState?.Busy);
        Assert.Contains(exchange.Notifications, x => x.Kind == EmulatorHostNotificationKind.AdvanceRequested);
    }

    [Fact]
    public void Session_CanReturnSignalNotificationsAfterAdvance()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x41 });

        var adapter = Legacy89DiskKitApplication.CreateEventDrivenEmulatorFdcHostAdapter();
        adapter.OpenDisk(0, container);
        var session = new EmulatorHostObservableProtocolSession(adapter);

        session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.SelectDrive, DriveNumber: 0)));
        session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 1, RegisterValue: 0)));
        session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 2, RegisterValue: 1)));
        session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 0, RegisterValue: 0x80)));

        var payload = session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.Advance, AdvanceMicroseconds: 1000)));
        var exchange = EmulatorHostProtocolCodec.DeserializeExchange(payload);

        Assert.True(exchange.Response.IrqAsserted);
        Assert.True(exchange.Response.DrqAsserted);
        Assert.Contains(exchange.Notifications, x => x.Kind == EmulatorHostNotificationKind.IrqChanged && x.SignalState == true);
        Assert.Contains(exchange.Notifications, x => x.Kind == EmulatorHostNotificationKind.DrqChanged && x.SignalState == true);
    }

    [Fact]
    public void Session_CanReturnCapabilitiesWithoutNotifications()
    {
        var session = new EmulatorHostObservableProtocolSession(Legacy89DiskKitApplication.CreateEventDrivenEmulatorFdcHostAdapter());

        var payload = session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(
            new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities)));
        var exchange = EmulatorHostProtocolCodec.DeserializeExchange(payload);

        Assert.NotNull(exchange.Response.Capabilities);
        Assert.False(exchange.Response.Capabilities!.SupportsPlainStdio);
        Assert.True(exchange.Response.Capabilities.SupportsObservableStdio);
        Assert.Empty(exchange.Notifications);
    }
}
