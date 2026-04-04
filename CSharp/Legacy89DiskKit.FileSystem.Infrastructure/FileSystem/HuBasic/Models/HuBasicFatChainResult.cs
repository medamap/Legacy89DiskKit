namespace Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Models;

public sealed record HuBasicFatChainResult(
    IReadOnlyList<int> Chain,
    int TerminalFlag
);
