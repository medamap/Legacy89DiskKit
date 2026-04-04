using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;

namespace Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;

public static class HuBasicLabelRules
{
    public static bool IsVirtualLabelEntry(FileEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        if (entry.FileSystemMetadata is not HuBasicFileMetadata metadata)
        {
            return false;
        }

        if (metadata.FileType != HuBasicFileType.Ascii)
        {
            return false;
        }

        bool looksDecorative = entry.FullName.All(ch => ch is '-' or '.' or ' ');
        bool hasSentinelAddresses = entry.LoadAddress == 0xFFFF &&
                                    entry.ExecutionAddress == 0xFFFF &&
                                    (entry.EndAddress == 0xFFFF || entry.Size == 0);
        bool suspiciousCluster = entry.StartCluster >= 0x7FFF;
        bool labelFlags = metadata.HasPassword && metadata.IsWriteProtected && !metadata.IsHidden && !metadata.IsVerify;

        return (looksDecorative || suspiciousCluster || hasSentinelAddresses) &&
               (labelFlags || suspiciousCluster || hasSentinelAddresses);
    }

    public static bool CanMergeLabelEntries(VirtualDirectoryLabelEntry previous, VirtualDirectoryLabelEntry current)
    {
        if (previous == null) throw new ArgumentNullException(nameof(previous));
        if (current == null) throw new ArgumentNullException(nameof(current));

        if (!string.IsNullOrEmpty(previous.Extension) || string.IsNullOrEmpty(previous.FileName))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(current.Extension) || string.IsNullOrEmpty(current.FileName))
        {
            return false;
        }

        if (!current.FileName.StartsWith(".", StringComparison.Ordinal))
        {
            return false;
        }

        return previous.RawModeByte == current.RawModeByte &&
               previous.PasswordByte == current.PasswordByte &&
               previous.Size == current.Size &&
               previous.LoadAddress == current.LoadAddress &&
               previous.EndAddress == current.EndAddress &&
               previous.ExecutionAddress == current.ExecutionAddress &&
               previous.StartCluster == current.StartCluster;
    }
}
