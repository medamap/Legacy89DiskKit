using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Legacy89DiskKit.Application.Fdc.Hosts.Scripting;
using Legacy89DiskKit.Domain.Fdc.Model;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostProofReportBuilderTest
{
    [Fact]
    public void Build_DetectsCapabilitiesAndSignals()
    {
        var transcript = new[]
        {
            new EmulatorHostTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: null,
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null,
                        Capabilities: new EmulatorHostCapabilities(1, true, true, true, true, true)),
                    [])),
            new EmulatorHostTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.OpenDiskPath, ImagePath: "/tmp/test.d88"),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: new FdcVisibleState(0, 0, 1, 0, 0, 0, true, false, false),
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: 1000),
                    [])),
            new EmulatorHostTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: 0x41,
                        VisibleState: new FdcVisibleState(0, 0, 1, 0x41, 0, 0, false, true, true),
                        IrqAsserted: true,
                        DrqAsserted: true,
                        PendingAdvanceMicroseconds: null),
                    [])),
            new EmulatorHostTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.CloseDisk),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: null,
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null),
                    [])),
        };

        var report = EmulatorHostProofReportBuilder.Build(transcript, "OpenDiskPath", "observable");

        Assert.True(report.CapabilityHandshakeSucceeded);
        Assert.True(report.SupportsPathOpen);
        Assert.True(report.SupportsBufferOpen);
        Assert.True(report.SupportsNotificationExchange);
        Assert.True(report.SupportsPlainStdio);
        Assert.True(report.SupportsObservableStdio);
        Assert.True(report.DiskOpenSucceeded);
        Assert.True(report.BusyObserved);
        Assert.True(report.IrqObserved);
        Assert.True(report.DrqObserved);
        Assert.True(report.DataReadSucceeded);
        Assert.True(report.CloseSucceeded);
    }
}
