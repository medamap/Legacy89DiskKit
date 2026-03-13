namespace Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;

public sealed record HuBasicFatChainResult(
    IReadOnlyList<int> Chain,
    int TerminalFlag
);
