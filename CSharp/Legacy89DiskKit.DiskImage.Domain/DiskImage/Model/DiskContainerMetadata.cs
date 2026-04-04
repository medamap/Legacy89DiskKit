namespace Legacy89DiskKit.DiskImage.Domain.Model;

public sealed record DiskContainerMetadata(
    string ImageFormat,
    DiskType DiskType,
    DiskGeometryInfo Geometry,
    bool IsWriteProtected,
    long DeclaredImageSize
);
