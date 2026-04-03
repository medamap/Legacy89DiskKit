using Legacy89DiskKit.Domain.CharacterEncoding.Model;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
namespace Legacy89DiskKit.Application.FileSystem;

public sealed class BootEntryImportService : IBootEntryImportService
{
    public void ImportEntry(IDiskContainer container, IFileSystem fileSystem, BootEntryImportMetadata metadata, byte[] payload)
    {
        var fsInfo = fileSystem.GetFileSystemInfo();

        if (metadata.MachineFamily == MachineType.X1)
        {
            ImportX1Entry(fileSystem, fsInfo, metadata, payload);
            return;
        }

        if (metadata.MachineFamily == MachineType.PC8801)
        {
            ImportPc88Entry(fileSystem, fsInfo, metadata, payload);
            return;
        }

        if (metadata.MachineFamily == MachineType.MSX || metadata.MachineFamily == MachineType.MSX2)
        {
            ImportMsxEntry(fileSystem, fsInfo, metadata, payload);
            return;
        }

        throw new InvalidOperationException($"Unsupported machine family for import: {metadata.MachineFamily}");
    }

    private void ImportX1Entry(IFileSystem fileSystem, DiskFileSystemInfo fsInfo, BootEntryImportMetadata metadata, byte[] payload)
    {
        if (metadata.Mode == "SectorResident")
        {
            fileSystem.WriteBootArea(payload);
            return;
        }

        if (metadata.Mode == "FileBacked")
        {
            if (fsInfo.FileSystemName != "Hu-BASIC")
            {
                throw new InvalidOperationException("Destination file system must be Hu-BASIC to accept file-backed X1 boot entries.");
            }

            var fileName = metadata.DisplayName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException("File-backed boot entry missing display name.");
            }
            var nameParts = fileName.Split('.', 2);
            var expectedName = nameParts[0].TrimEnd();
            var expectedExt = nameParts.Length > 1 ? nameParts[1].TrimEnd() : "";
            if (metadata.StartRecord == null)
            {
                throw new InvalidOperationException("File-backed boot import requires an explicit start record.");
            }

            var bootArea = fileSystem.ReadBootArea();
            if (bootArea.Length < 32)
            {
                bootArea = new byte[256];
            }

            var encoder = Legacy89DiskKitApplication.ResolveEncoder(fsInfo);
            var nameBytes = encoder.EncodeText(expectedName.PadRight(13, ' ').Substring(0, 13));
            var extBytes = encoder.EncodeText(expectedExt.PadRight(3, ' ').Substring(0, 3));

            bootArea[0] = 0x01; // Bootable flag

            Array.Copy(nameBytes, 0, bootArea, 0x01, Math.Min(13, nameBytes.Length));
            for(int i = nameBytes.Length; i < 13; i++) bootArea[0x01+i] = 0x20;

            Array.Copy(extBytes, 0, bootArea, 0x0E, Math.Min(3, extBytes.Length));
            for(int i = extBytes.Length; i < 3; i++) bootArea[0x0E+i] = 0x20;

            bootArea[0x11] = 0x20; // Password padding (usually 0x20 when no password)

            BitConverter.TryWriteBytes(bootArea.AsSpan(0x12, 2), (ushort)payload.Length);
            BitConverter.TryWriteBytes(bootArea.AsSpan(0x14, 2), metadata.LoadAddress ?? 0);
            BitConverter.TryWriteBytes(bootArea.AsSpan(0x16, 2), metadata.ExecutionAddress ?? 0);
            BitConverter.TryWriteBytes(bootArea.AsSpan(0x1E, 2), metadata.StartRecord.Value);

            fileSystem.WriteBootArea(bootArea);
            return;
        }

        throw new InvalidOperationException($"Unsupported X1 boot mode for import: {metadata.Mode}");
    }

    private void ImportPc88Entry(IFileSystem fileSystem, DiskFileSystemInfo fsInfo, BootEntryImportMetadata metadata, byte[] payload)
    {
        if (metadata.Mode != "SectorResident")
        {
            throw new InvalidOperationException($"Unsupported PC-8801 boot mode for import: {metadata.Mode}");
        }

        fileSystem.WriteBootArea(payload);
    }

    private void ImportMsxEntry(IFileSystem fileSystem, DiskFileSystemInfo fsInfo, BootEntryImportMetadata metadata, byte[] payload)
    {
        if (metadata.Mode != "SectorResident")
        {
            throw new InvalidOperationException($"Unsupported MSX boot mode for import: {metadata.Mode}");
        }

        fileSystem.WriteBootArea(payload);
    }
}
