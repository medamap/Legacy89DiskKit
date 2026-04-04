namespace Legacy89DiskKit.Domain.CharacterEncoding.Model;

public sealed record CharacterEncodingProfile(
    string EncodingId,
    string DisplayName,
    MachineType MachineType
);
