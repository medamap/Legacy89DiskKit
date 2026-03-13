using Legacy89DiskKit.Domain.DiskImage.Exception;
using Legacy89DiskKit.Domain.DiskImage.Model;
using System.Text;

namespace Legacy89DiskKit.Infrastructure.DiskImage.D88;

public static class D88ImageParser
{
    public static D88Header ParseHeader(byte[] imageData)
    {
        using var stream = new MemoryStream(imageData);
        using var reader = new BinaryReader(stream);

        var imageName = reader.ReadBytes(17);
        var diskName = Encoding.ASCII.GetString(imageName).TrimEnd('\0');

        reader.BaseStream.Seek(17 + 9, SeekOrigin.Begin);
        var protect = reader.ReadByte();
        var mediaTypeByte = reader.ReadByte();

        if (!Enum.IsDefined(typeof(DiskType), mediaTypeByte))
        {
            throw new DiskImageException($"Invalid media type: 0x{mediaTypeByte:X2}");
        }

        var mediaType = (DiskType)mediaTypeByte;
        var diskSize = reader.ReadUInt32();

        reader.BaseStream.Seek(0x20, SeekOrigin.Begin);
        var trackOffsets = new uint[164];
        for (int i = 0; i < 164; i++)
        {
            trackOffsets[i] = reader.ReadUInt32();
        }

        return new D88Header
        {
            ImageName = diskName,
            WriteProtect = protect != 0,
            MediaType = mediaType,
            DiskSize = diskSize,
            TrackOffsets = trackOffsets
        };
    }

    public static Dictionary<(int Cylinder, int Head, int Sector), D88SectorData> ParseSectors(byte[] imageData, D88Header header)
    {
        var sectors = new Dictionary<(int Cylinder, int Head, int Sector), D88SectorData>();

        for (int track = 0; track < 164; track++)
        {
            if (header.TrackOffsets[track] == 0)
            {
                continue;
            }

            ParseTrack(imageData, header, track, header.TrackOffsets[track], sectors);
        }

        return sectors;
    }

    private static void ParseTrack(
        byte[] imageData,
        D88Header header,
        int trackIndex,
        uint offset,
        Dictionary<(int Cylinder, int Head, int Sector), D88SectorData> sectors)
    {
        using var stream = new MemoryStream(imageData);
        using var reader = new BinaryReader(stream);
        reader.BaseStream.Seek(offset, SeekOrigin.Begin);

        var sectorsInTrack = 0;

        while (reader.BaseStream.Position < imageData.Length)
        {
            if (reader.BaseStream.Position + 16 > imageData.Length)
            {
                break;
            }

            var cylinder = reader.ReadByte();
            var head = reader.ReadByte();
            var sector = reader.ReadByte();
            var sectorSizeN = reader.ReadByte();
            var sectorCount = reader.ReadUInt16();
            var density = reader.ReadByte();
            var deleted = reader.ReadByte();
            var status = reader.ReadByte();
            reader.ReadBytes(5);
            var actualSize = reader.ReadUInt16();

            if (reader.BaseStream.Position + actualSize > imageData.Length)
            {
                break;
            }

            var data = reader.ReadBytes(actualSize);
            sectors[(cylinder, head, sector)] = new D88SectorData
            {
                Cylinder = cylinder,
                Head = head,
                Sector = sector,
                SectorSizeN = sectorSizeN,
                SectorCount = sectorCount,
                Density = density,
                Deleted = deleted != 0,
                Status = status,
                ActualSize = actualSize,
                Data = data
            };

            sectorsInTrack++;

            if (sectorsInTrack >= sectorCount)
            {
                break;
            }

            if (trackIndex < 163 && header.TrackOffsets[trackIndex + 1] > 0 &&
                reader.BaseStream.Position >= header.TrackOffsets[trackIndex + 1])
            {
                break;
            }
        }
    }
}
