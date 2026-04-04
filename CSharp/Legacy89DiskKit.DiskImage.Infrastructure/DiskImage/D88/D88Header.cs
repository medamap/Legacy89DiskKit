using Legacy89DiskKit.DiskImage.Domain.Model;

namespace Legacy89DiskKit.DiskImage.Infrastructure.D88;

public sealed class D88Header
{
    public string ImageName { get; init; } = "";

    public bool WriteProtect { get; init; }

    public DiskType MediaType { get; init; }

    public uint DiskSize { get; init; }

    public uint[] TrackOffsets { get; init; } = new uint[164];
}
