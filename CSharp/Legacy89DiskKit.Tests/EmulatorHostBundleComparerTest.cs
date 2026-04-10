using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Legacy89DiskKit.Fdc.Application.Hosts.Scripting;
using Legacy89DiskKit.Domain.Fdc.Model;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostBundleComparerTest
{
    [Fact]
    public void Compare_ReturnsNoMismatchesForEventDrivenD88Baseline()
    {
        var bundle = new EmulatorHostBundle(
            new EmulatorHostBundleManifest("proof", "proof.md", "proof.jsonl", "proof.requests.jsonl", "OpenDiskPath", "observable"),
            MarkdownReport: "# report",
            Transcript:
            [
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
            ],
            RequestScript: []);

        var mismatches = EmulatorHostBundleComparer.Compare(
            bundle,
            EmulatorHostProofExpectationCatalog.EventDrivenFirstProofD88());

        Assert.Empty(mismatches);
    }
}
