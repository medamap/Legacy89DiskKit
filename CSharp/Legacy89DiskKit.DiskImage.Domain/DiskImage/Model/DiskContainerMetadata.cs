namespace Legacy89DiskKit.Domain.DiskImage.Model;

public sealed record DiskContainerMetadata(
    string ImageFormat,
    DiskType DiskType,
    DiskGeometryInfo Geometry,
    bool IsWriteProtected,
    long DeclaredImageSize
);
