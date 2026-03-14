using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofReportBuilderTest
{
    [Fact]
    public void Builder_CanSummarizeTranscript()
    {
        var transcript = new[]
        {
            new HostProofTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: null,
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null,
                        Capabilities: new EmulatorHostCapabilities(
                            ProtocolVersion: 1,
                            SupportsPathOpen: true,
                            SupportsBufferOpen: true,
                            SupportsNotificationExchange: true,
                            SupportsPlainStdio: false,
                            SupportsObservableStdio: true)),
                    [])),
            new HostProofTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.OpenDiskPath),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: new Legacy89DiskKit.Domain.Fdc.Model.FdcVisibleState(0, 0, 0, 0, 1, 0, false, false, false),
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null),
                    [])),
            new HostProofTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.Advance),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: new Legacy89DiskKit.Domain.Fdc.Model.FdcVisibleState(0, 0, 1, 0, 1, 0, true, true, true),
                        IrqAsserted: true,
                        DrqAsserted: true,
                        PendingAdvanceMicroseconds: null),
                    [])),
            new HostProofTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: 0x41,
                        VisibleState: null,
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null),
                    [])),
            new HostProofTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.CloseDisk),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: null,
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null),
                    []))
        };

        var report = HostProofReportBuilder.Build(transcript, "OpenDiskPath", "observable");

        Assert.True(report.CapabilityHandshakeSucceeded);
        Assert.True(report.SupportsPathOpen);
        Assert.True(report.DiskOpenSucceeded);
        Assert.True(report.BusyObserved);
        Assert.True(report.IrqObserved);
        Assert.True(report.DrqObserved);
        Assert.True(report.DataReadSucceeded);
        Assert.True(report.CloseSucceeded);
    }
}
