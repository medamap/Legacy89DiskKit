using Legacy89DiskKit.Application;
using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Xunit;

namespace Legacy89DiskKit.Tests;

public class EmulatorHostProtocolEndpointTest
{
    [Fact]
    public void Codec_CanRoundTripRequestAndResponse()
    {
        var request = new EmulatorHostRequest(
            EmulatorHostRequestKind.WriteRegister,
            DriveNumber: 0,
            RegisterAddress: 1,
            RegisterValue: 0x12);
        var requestPayload = EmulatorHostProtocolCodec.SerializeRequest(request);
        var requestRoundTrip = EmulatorHostProtocolCodec.DeserializeRequest(requestPayload);

        Assert.Equal(request, requestRoundTrip);

        var response = new EmulatorHostResponse(0x34, null, false, true, 1000);
        var responsePayload = EmulatorHostProtocolCodec.SerializeResponse(response);
        var responseRoundTrip = EmulatorHostProtocolCodec.DeserializeResponse(responsePayload);

        Assert.Equal(response, responseRoundTrip);
    }

    [Fact]
    public void Endpoint_CanHandleJsonRequests()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x41, 0x42, 0x43 });

        var adapter = Legacy89DiskKitApplication.CreateEventDrivenEmulatorFdcHostAdapter();
        adapter.OpenDisk(0, container);
        var endpoint = new EmulatorHostProtocolEndpoint(adapter);

        endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.SelectDrive, DriveNumber: 0)));
        endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 1, RegisterValue: 0)));
        endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 2, RegisterValue: 1)));

        var commandPayload = endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 0, RegisterValue: 0x80)));
        var commandResponse = EmulatorHostProtocolCodec.DeserializeResponse(commandPayload);

        Assert.Equal(1000, commandResponse.PendingAdvanceMicroseconds);
        Assert.True(commandResponse.VisibleState?.Busy);

        var advancePayload = endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.Advance, AdvanceMicroseconds: 1000)));
        var advanceResponse = EmulatorHostProtocolCodec.DeserializeResponse(advancePayload);

        Assert.True(advanceResponse.IrqAsserted);
        Assert.True(advanceResponse.DrqAsserted);

        var firstByte = EmulatorHostProtocolCodec.DeserializeResponse(endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3))));
        var secondByte = EmulatorHostProtocolCodec.DeserializeResponse(endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3))));
        var thirdByte = EmulatorHostProtocolCodec.DeserializeResponse(endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3))));

        Assert.Equal((byte?)0x41, firstByte.RegisterValue);
        Assert.Equal((byte?)0x42, secondByte.RegisterValue);
        Assert.Equal((byte?)0x43, thirdByte.RegisterValue);
    }

    [Fact]
    public void Endpoint_CanOpenAndCloseDiskByPath()
    {
        using var container = D88DiskContainer.CreateNewInMemory("TESTDISK", Domain.DiskImage.Model.DiskType.TwoD);
        container.WriteSector(0, 0, 1, new byte[] { 0x51 });

        var imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d88");
        File.WriteAllBytes(imagePath, container.ToImageData());

        try
        {
            var endpoint = new EmulatorHostProtocolEndpoint(Legacy89DiskKitApplication.CreateEventDrivenEmulatorFdcHostAdapter());

            var openPayload = endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(
                new EmulatorHostRequest(EmulatorHostRequestKind.OpenDiskPath, ImagePath: imagePath, DriveNumber: 0, ReadOnly: true)));
            var openResponse = EmulatorHostProtocolCodec.DeserializeResponse(openPayload);

            Assert.False(openResponse.IrqAsserted);
            Assert.NotNull(openResponse.VisibleState);

            endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.SelectDrive, DriveNumber: 0)));
            endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 1, RegisterValue: 0)));
            endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 2, RegisterValue: 1)));
            endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 0, RegisterValue: 0x80)));
            endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.Advance, AdvanceMicroseconds: 1000)));

            var firstByte = EmulatorHostProtocolCodec.DeserializeResponse(
                endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3))));

            Assert.Equal((byte?)0x51, firstByte.RegisterValue);

            var closePayload = endpoint.Handle(EmulatorHostProtocolCodec.SerializeRequest(
                new EmulatorHostRequest(EmulatorHostRequestKind.CloseDisk, DriveNumber: 0)));
            var closeResponse = EmulatorHostProtocolCodec.DeserializeResponse(closePayload);

            Assert.Null(closeResponse.VisibleState);
        }
        finally
        {
            File.Delete(imagePath);
        }
    }
}
