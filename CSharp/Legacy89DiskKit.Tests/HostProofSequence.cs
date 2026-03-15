using Legacy89DiskKit.Application.Fdc.Hosts.Protocol;

namespace Legacy89DiskKit.Tests;

internal static class HostProofSequence
{
    public static IReadOnlyList<EmulatorHostRequest> CreateReadOnlyD88ByPathSequence(string imagePath, int driveNumber = 0)
    {
        return
        [
            new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
            new EmulatorHostRequest(
                EmulatorHostRequestKind.OpenDiskPath,
                ImagePath: imagePath,
                DriveNumber: driveNumber,
                ReadOnly: true),
            new EmulatorHostRequest(EmulatorHostRequestKind.SelectDrive, DriveNumber: driveNumber),
            new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 1, RegisterValue: 0),
            new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 2, RegisterValue: 1),
            new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 0, RegisterValue: 0x80),
            new EmulatorHostRequest(EmulatorHostRequestKind.Advance, AdvanceMicroseconds: 1000),
            new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3),
            new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3),
            new EmulatorHostRequest(EmulatorHostRequestKind.CloseDisk, DriveNumber: driveNumber)
        ];
    }

    public static IReadOnlyList<EmulatorHostRequest> CreateReadOnlyD88ByBufferSequence(byte[] imageData, string imageFormat = "d88", int driveNumber = 0)
    {
        return
        [
            new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
            new EmulatorHostRequest(
                EmulatorHostRequestKind.OpenDiskImage,
                ImageFormat: imageFormat,
                ImageDataBase64: Convert.ToBase64String(imageData),
                DriveNumber: driveNumber,
                ReadOnly: true),
            new EmulatorHostRequest(EmulatorHostRequestKind.SelectDrive, DriveNumber: driveNumber),
            new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 1, RegisterValue: 0),
            new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 2, RegisterValue: 1),
            new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 0, RegisterValue: 0x80),
            new EmulatorHostRequest(EmulatorHostRequestKind.Advance, AdvanceMicroseconds: 1000),
            new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3)
        ];
    }

    public static IReadOnlyList<EmulatorHostRequest> CreateReadOnlyRawByBufferSequence(byte[] imageData, string imageFormat = "2d", int driveNumber = 0)
    {
        return
        [
            new EmulatorHostRequest(EmulatorHostRequestKind.QueryCapabilities),
            new EmulatorHostRequest(
                EmulatorHostRequestKind.OpenDiskImage,
                ImageFormat: imageFormat,
                ImageDataBase64: Convert.ToBase64String(imageData),
                DriveNumber: driveNumber,
                ReadOnly: true),
            new EmulatorHostRequest(EmulatorHostRequestKind.SelectDrive, DriveNumber: driveNumber),
            new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 1, RegisterValue: 0),
            new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 2, RegisterValue: 1),
            new EmulatorHostRequest(EmulatorHostRequestKind.WriteRegister, RegisterAddress: 0, RegisterValue: 0x80),
            new EmulatorHostRequest(EmulatorHostRequestKind.Advance, AdvanceMicroseconds: 1000),
            new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3),
            new EmulatorHostRequest(EmulatorHostRequestKind.ReadRegister, RegisterAddress: 3)
        ];
    }
}
