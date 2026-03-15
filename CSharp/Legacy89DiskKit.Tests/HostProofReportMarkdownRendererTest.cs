using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofReportMarkdownRendererTest
{
    [Fact]
    public void Renderer_CanRenderMarkdownSummary()
    {
        var report = new HostProofReport(
            OpenMode: "OpenDiskPath",
            ExchangeMode: "observable",
            CapabilityHandshakeSucceeded: true,
            SupportsPathOpen: true,
            SupportsBufferOpen: true,
            SupportsNotificationExchange: true,
            SupportsPlainStdio: false,
            SupportsObservableStdio: true,
            DiskOpenSucceeded: true,
            BusyObserved: true,
            IrqObserved: true,
            DrqObserved: true,
            DataReadSucceeded: true,
            CloseSucceeded: true);

        var markdown = HostProofReportMarkdownRenderer.Render(report);

        Assert.Contains("# Host Proof Report", markdown);
        Assert.Contains("- Open mode: OpenDiskPath", markdown);
        Assert.Contains("- SupportsObservableStdio: True", markdown);
        Assert.Contains("- Data read succeeded: True", markdown);
    }
}
