using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

namespace Legacy89DiskKit.Tests;

internal static class HostProofRequestScriptCodec
{
    public static string SerializeLines(IEnumerable<EmulatorHostRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);

        return string.Join(
            Environment.NewLine,
            requests.Select(EmulatorHostProtocolCodec.SerializeRequest));
    }

    public static IReadOnlyList<EmulatorHostRequest> DeserializeLines(string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return payload
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(EmulatorHostProtocolCodec.DeserializeRequest)
            .ToArray();
    }
}
