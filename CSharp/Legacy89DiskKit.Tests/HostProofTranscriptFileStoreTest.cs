using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofTranscriptFileStoreTest
{
    [Fact]
    public async Task FileStore_CanRoundTripTranscriptFile()
    {
        var entries = new[]
        {
            new HostProofTranscriptEntry(
                new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
                new EmulatorHostExchange(
                    new EmulatorHostResponse(
                        RegisterValue: null,
                        VisibleState: null,
                        IrqAsserted: false,
                        DrqAsserted: false,
                        PendingAdvanceMicroseconds: null),
                    []))
        };

        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.jsonl");

        try
        {
            await HostProofTranscriptFileStore.SaveAsync(filePath, entries);
            var roundTrip = await HostProofTranscriptFileStore.LoadAsync(filePath);

            Assert.Single(roundTrip);
            Assert.Equal(entries[0].Request.Kind, roundTrip[0].Request.Kind);
            Assert.Equal(
                HostProofTranscriptCodec.SerializeLines(entries),
                HostProofTranscriptCodec.SerializeLines(roundTrip));
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}
