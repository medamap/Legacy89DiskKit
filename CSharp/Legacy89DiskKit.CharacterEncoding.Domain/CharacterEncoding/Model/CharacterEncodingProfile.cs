namespace Legacy89DiskKit.CharacterEncoding.Domain.Model;

public sealed record CharacterEncodingProfile(
    string EncodingId,
    string DisplayName,
    MachineType MachineType
);
