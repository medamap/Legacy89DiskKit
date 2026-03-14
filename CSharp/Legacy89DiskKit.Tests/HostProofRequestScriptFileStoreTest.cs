using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofRequestScriptFileStoreTest
{
    [Fact]
    public async Task FileStore_CanRoundTripRequestScriptFile()
    {
        var requests = HostProofSequence.CreateReadOnlyD88ByBufferSequence([0x00, 0x01], driveNumber: 1);
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.jsonl");

        try
        {
            await HostProofRequestScriptFileStore.SaveAsync(filePath, requests);
            var roundTrip = await HostProofRequestScriptFileStore.LoadAsync(filePath);

            Assert.Equal(
                HostProofRequestScriptCodec.SerializeLines(requests),
                HostProofRequestScriptCodec.SerializeLines(roundTrip));
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}
