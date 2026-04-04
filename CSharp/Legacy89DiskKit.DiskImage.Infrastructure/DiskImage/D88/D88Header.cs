using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.Infrastructure.DiskImage.D88;

public sealed class D88Header
{
    public string ImageName { get; init; } = "";

    public bool WriteProtect { get; init; }

    public DiskType MediaType { get; init; }

    public uint DiskSize { get; init; }

    public uint[] TrackOffsets { get; init; } = new uint[164];
}
