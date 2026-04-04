using System.Text;
using Legacy89DiskKit.Fdc.Application.Hosts.Protocol;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostProtocolStdioRunnerTest
{
    [Fact]
    public async Task Runner_CanProcessLineDelimitedRequestsOverInjectedStreams()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Legacy89DiskKit.DiskImage.Domain.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x31, 0x32 });

        var adapter = CreateEventDrivenEmulatorFdcHostAdapter();
        adapter.OpenDisk(0, container);

        var requests = string.Join(Environment.NewLine, new[]
        {
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.SelectDrive, DriveNumber: 0)),
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 1, RegisterValue: 0)),
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 2, RegisterValue: 1)),
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 0, RegisterValue: 0x80)),
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.Advance, AdvanceMicroseconds: 1000)),
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3)),
            EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3))
        }) + Environment.NewLine;

        using var reader = new StringReader(requests);
        var builder = new StringBuilder();
        using var writer = new StringWriter(builder);

        var runner = new EmulatorHostProtocolStdioRunner(
            new EmulatorHostProtocolTextSession(new EmulatorHostProtocolEndpoint(adapter)),
            reader,
            writer);

        await runner.RunAsync();

        var responses = builder
            .ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(EmulatorHostProtocolCodec.DeserializeResponse)
            .ToArray();

        Assert.Equal(7, responses.Length);
        Assert.True(responses[3].VisibleState?.Busy);
        Assert.True(responses[4].IrqAsserted);
        Assert.True(responses[4].DrqAsserted);
        Assert.Equal((byte?)0x31, responses[5].RegisterValue);
        Assert.Equal((byte?)0x32, responses[6].RegisterValue);
    }

    private static Legacy89DiskKit.Fdc.Application.Hosts.EventDrivenEmulatorFdcHostAdapter CreateEventDrivenEmulatorFdcHostAdapter()
    {
        return new Legacy89DiskKit.Fdc.Application.Hosts.EventDrivenEmulatorFdcHostAdapter(
            new Legacy89DiskKit.Drive.Application.DriveMountService(),
            new Legacy89DiskKit.Drive.Application.MountedMediumBindingService(),
            new Legacy89DiskKit.DiskImage.Infrastructure.Factory.DiskContainerFactory());
    }
}
