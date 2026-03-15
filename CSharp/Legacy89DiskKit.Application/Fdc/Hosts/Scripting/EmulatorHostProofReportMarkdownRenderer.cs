using System.Text;

namespace Legacy89DiskKit.Application.Fdc.Hosts.Scripting;

public static class EmulatorHostProofReportMarkdownRenderer
{
    public static string Render(EmulatorHostProofReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# Host Proof Report");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Open mode: {report.OpenMode}");
        builder.AppendLine($"- Exchange mode: {report.ExchangeMode}");
        builder.AppendLine($"- Capability handshake succeeded: {report.CapabilityHandshakeSucceeded}");
        builder.AppendLine();
        builder.AppendLine("## Capabilities");
        builder.AppendLine();
        builder.AppendLine($"- SupportsPathOpen: {report.SupportsPathOpen}");
        builder.AppendLine($"- SupportsBufferOpen: {report.SupportsBufferOpen}");
        builder.AppendLine($"- SupportsNotificationExchange: {report.SupportsNotificationExchange}");
        builder.AppendLine($"- SupportsPlainStdio: {report.SupportsPlainStdio}");
        builder.AppendLine($"- SupportsObservableStdio: {report.SupportsObservableStdio}");
        builder.AppendLine();
        builder.AppendLine("## Proof");
        builder.AppendLine();
        builder.AppendLine($"- Disk open succeeded: {report.DiskOpenSucceeded}");
        builder.AppendLine($"- Busy observed: {report.BusyObserved}");
        builder.AppendLine($"- IRQ observed: {report.IrqObserved}");
        builder.AppendLine($"- DRQ observed: {report.DrqObserved}");
        builder.AppendLine($"- Data read succeeded: {report.DataReadSucceeded}");
        builder.AppendLine($"- Close succeeded: {report.CloseSucceeded}");
        return builder.ToString();
    }
}
