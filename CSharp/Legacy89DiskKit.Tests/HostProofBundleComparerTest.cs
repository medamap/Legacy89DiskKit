using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofBundleComparerTest
{
    [Fact]
    public async Task Comparer_CanAcceptSufficientBundle()
    {
        var report = new HostProofReport(
            OpenMode: "OpenDiskPath",
            ExchangeMode: "observable",
            CapabilityHandshakeSucceeded: true,
            SupportsPathOpen: true,
            SupportsBufferOpen: false,
            SupportsNotificationExchange: true,
            SupportsPlainStdio: false,
            SupportsObservableStdio: true,
            DiskOpenSucceeded: true,
            BusyObserved: true,
            IrqObserved: true,
            DrqObserved: true,
            DataReadSucceeded: true,
            CloseSucceeded: true);

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
                            SupportsBufferOpen: false,
                            SupportsNotificationExchange: true,
                            SupportsPlainStdio: false,
                            SupportsObservableStdio: true)),
                    [])),
            new HostProofTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.OpenDiskPath),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: new Legacy89DiskKit.Domain.Fdc.Model.FdcVisibleState(0, 0, 0, 0, 0, 0, false, false, false),
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null),
                    [])),
            new HostProofTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.Advance),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: new Legacy89DiskKit.Domain.Fdc.Model.FdcVisibleState(0, 0, 1, 0, 0, 0, true, true, true),
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

        var outputDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            await HostProofBundleWriter.WriteAsync(outputDirectory, "proof", report, transcript);
            var bundle = await HostProofBundleReader.ReadAsync(outputDirectory, "proof");
            var mismatches = HostProofBundleComparer.Compare(
                bundle,
                new HostProofExpectation(
                    RequirePathOpen: true,
                    RequireBufferOpen: false,
                    RequireNotificationExchange: true,
                    RequireDiskOpen: true,
                    RequireBusyObserved: true,
                    RequireIrqObserved: true,
                    RequireDrqObserved: true,
                    RequireDataRead: true,
                    RequireClose: true));

            Assert.Empty(mismatches);
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }
}
