using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofBundleManifestCodecTest
{
    [Fact]
    public void Codec_CanRoundTripManifest()
    {
        var manifest = new HostProofBundleManifest(
            BaseName: "proof",
            ReportFileName: "proof.md",
            TranscriptFileName: "proof.jsonl",
            RequestScriptFileName: "proof.requests.jsonl",
            OpenMode: "OpenDiskPath",
            ExchangeMode: "observable");

        var payload = HostProofBundleManifestCodec.Serialize(manifest);
        var roundTrip = HostProofBundleManifestCodec.Deserialize(payload);

        Assert.Equal(manifest, roundTrip);
    }
}
