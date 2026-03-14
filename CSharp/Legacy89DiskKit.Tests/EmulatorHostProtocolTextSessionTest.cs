using System.Text;
using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostProtocolTextSessionTest
{
    [Fact]
    public void Session_CanHandleSingleRequestLine()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x41 });

        var adapter = Legacy89DiskKitApplication.CreateEventDrivenEmulatorFdcHostAdapter();
        adapter.OpenDisk(0, container);
        var endpoint = new EmulatorHostProtocolEndpoint(adapter);
        var session = new EmulatorHostProtocolTextSession(endpoint);

        session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.SelectDrive, DriveNumber: 0)));
        session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 1, RegisterValue: 0)));
        session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 2, RegisterValue: 1)));

        var payload = session.HandleLine(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 0, RegisterValue: 0x80)));
        var response = EmulatorHostProtocolCodec.DeserializeResponse(payload);

        Assert.Equal(1000, response.PendingAdvanceMicroseconds);
        Assert.True(response.VisibleState?.Busy);
    }

    [Fact]
    public async Task Session_CanProcessLineDelimitedRequestsOverTextStreams()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x41, 0x42 });

        var adapter = Legacy89DiskKitApplication.CreateEventDrivenEmulatorFdcHostAdapter();
        adapter.OpenDisk(0, container);
        var endpoint = new EmulatorHostProtocolEndpoint(adapter);
        var session = new EmulatorHostProtocolTextSession(endpoint);

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

        await session.RunAsync(reader, writer);

        var responses = builder
            .ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(EmulatorHostProtocolCodec.DeserializeResponse)
            .ToArray();

        Assert.Equal(7, responses.Length);
        Assert.True(responses[3].VisibleState?.Busy);
        Assert.True(responses[4].IrqAsserted);
        Assert.True(responses[4].DrqAsserted);
        Assert.Equal((byte?)0x41, responses[5].RegisterValue);
        Assert.Equal((byte?)0x42, responses[6].RegisterValue);
    }
}
