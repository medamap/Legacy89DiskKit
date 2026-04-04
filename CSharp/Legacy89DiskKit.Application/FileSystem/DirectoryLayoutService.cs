using System.Security.Cryptography;
using System.Text;
using Legacy89DiskKit.Application.CharacterEncoding;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Interface.Layout;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.Application.FileSystem;
public class DirectoryLayoutService
{
    private readonly EncoderRegistry _encoderRegistry;
    public DirectoryLayoutService()
    {
        _encoderRegistry = new EncoderRegistry();
        _encoderRegistry.Register("X1", new X1CharacterEncoder());
        _encoderRegistry.Register("SJIS", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("Shift-JIS", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("MSX", new ShiftJisCharacterEncoder());
        _encoderRegistry.Register("PC88", new ShiftJisCharacterEncoder());
    }

    public DirectoryEntryLayout GetLayout(IFileSystem fileSystem)
    {
        return GetProvider(fileSystem).ReadDirectoryLayout();
    }

    public string ExportPlan(IFileSystem fileSystem)
    {
        var layout = GetLayout(fileSystem);
        var lines = new List<string>(layout.Items.Count);
        foreach (var item in layout.Items.OrderBy(item => item.Order))
        {
            if (item.Kind == DirectoryLayoutItemKind.VirtualLabel)
            {
                lines.Add($"# {BuildLabelText(item)}");
                continue;
            }

            lines.Add($"{CreateStableId(item.Id)} {item.DisplayName}");
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    public DirectoryLayoutValidationResult ValidatePlan(IFileSystem fileSystem, string text)
    {
        var layout = GetLayout(fileSystem);
        var plan = ParsePlan(text);
        return ValidatePlan(fileSystem, layout, plan);
    }

    public DirectoryLayoutValidationResult ApplyPlan(IFileSystem fileSystem, string text, bool strict = false)
    {
        var provider = GetProvider(fileSystem);
        var validation = ValidatePlan(fileSystem, text);
        if (!validation.IsValid || (strict && validation.WarningCount > 0))
        {
            return validation;
        }

        if (validation.ProposedLayout == null)
        {
            throw new InvalidOperationException("No proposed layout is available.");
        }

        provider.ApplyDirectoryLayout(validation.ProposedLayout);
        return validation;
    }

    public DirectoryEntryLayout MoveEntryBefore(IFileSystem fileSystem, string sourceName, string targetName)
    {
        var provider = GetProvider(fileSystem);
        var layout = provider.ReadDirectoryLayout();
        var source = FindByDisplayName(layout, sourceName);
        var target = FindByDisplayName(layout, targetName);
        var items = layout.Items.ToList();
        items.Remove(source);
        var targetIndex = items.FindIndex(item => item.Id == target.Id);
        items.Insert(targetIndex, source);
        var updated = Reindex(layout, items);
        provider.ApplyDirectoryLayout(updated);
        return updated;
    }

    public DirectoryEntryLayout InsertLabelBefore(IFileSystem fileSystem, string labelText, string targetName)
    {
        var provider = GetProvider(fileSystem);
        var layout = provider.ReadDirectoryLayout();
        var target = FindByDisplayName(layout, targetName);
        var items = layout.Items.ToList();
        var targetIndex = items.FindIndex(item => item.Id == target.Id);
        var label = CreateLabel(fileSystem, labelText, items.Count);
        items.Insert(targetIndex, label);
        var updated = Reindex(layout, items);
        provider.ApplyDirectoryLayout(updated);
        return updated;
    }

    public DirectoryEntryLayout SortEntries(IFileSystem fileSystem, DirectorySortBy sortBy)
    {
        var provider = GetProvider(fileSystem);
        var layout = provider.ReadDirectoryLayout();
        var virtualPositions = layout.Items.Select((item, index) => new { item, index }).Where(x => x.item.Kind == DirectoryLayoutItemKind.VirtualLabel).ToDictionary(x => x.index, x => x.item);
        var sortedFiles = layout.Items.Where(item => item.Kind == DirectoryLayoutItemKind.FileEntry).OrderBy(item => GetSortKey(item, sortBy), StringComparer.OrdinalIgnoreCase).ThenBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();
        var rebuilt = new List<DirectoryLayoutItem>(layout.Items.Count);
        var fileIndex = 0;
        for (var i = 0; i < layout.Items.Count; i++)
        {
            if (virtualPositions.TryGetValue(i, out var virtualItem))
            {
                rebuilt.Add(virtualItem);
                continue;
            }

            rebuilt.Add(sortedFiles[fileIndex++]);
        }

        var updated = Reindex(layout, rebuilt);
        provider.ApplyDirectoryLayout(updated);
        return updated;
    }

    private DirectoryLayoutValidationResult ValidatePlan(IFileSystem fileSystem, DirectoryEntryLayout layout, DirectoryLayoutTextPlan plan)
    {
        var messages = new List<DirectoryLayoutValidationMessage>();
        var fsInfo = fileSystem.GetFileSystemInfo();
        var supportsLayout = fileSystem is IDirectoryLayoutProvider;
        if (!supportsLayout)
        {
            messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Error, 0, "Directory layout is not supported for this file system."));
            return new DirectoryLayoutValidationResult(plan, messages, null);
        }

        var encoder = ResolveEncoder(fsInfo);
        var filesByStableId = layout.Items.Where(item => item.Kind == DirectoryLayoutItemKind.FileEntry).ToDictionary(item => CreateStableId(item.Id), item => item, StringComparer.OrdinalIgnoreCase);
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var proposedItems = new List<DirectoryLayoutItem>();
        var consumedFileIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in plan.Entries)
        {
            if (entry.IsLabel)
            {
                DirectoryLayoutItem label;
                try
                {
                    label = CreateLabel(fileSystem, entry.Text, proposedItems.Count);
                }
                catch (Exception ex)
                {
                    messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Error, entry.LineNumber, ex.Message));
                    continue;
                }

                proposedItems.Add(label);
                continue;
            }

            if (!seenIds.Add(entry.StableId))
            {
                messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Error, entry.LineNumber, $"Duplicate entry id: {entry.StableId}"));
                continue;
            }

            if (!filesByStableId.TryGetValue(entry.StableId, out var item))
            {
                messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Error, entry.LineNumber, $"Unknown entry id: {entry.StableId}"));
                continue;
            }

            consumedFileIds.Add(entry.StableId);
            var renamedEntry = ApplyDisplayName(item.Entry!, entry.Text, fsInfo, encoder, entry.LineNumber, messages);
            if (renamedEntry == null)
            {
                continue;
            }

            if (!usedNames.Add(renamedEntry.FullName))
            {
                messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Error, entry.LineNumber, $"Duplicate target file name: {renamedEntry.FullName}"));
                continue;
            }

            proposedItems.Add(item with { DisplayName = renamedEntry.FullName, Entry = renamedEntry });
        }

        foreach (var item in layout.Items.Where(item => item.Kind == DirectoryLayoutItemKind.FileEntry))
        {
            var stableId = CreateStableId(item.Id);
            if (consumedFileIds.Contains(stableId))
            {
                continue;
            }

            messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Warning, 0, $"Entry omitted from plan and moved to the end: {item.DisplayName}"));
            if (!usedNames.Add(item.DisplayName))
            {
                messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Error, 0, $"Duplicate target file name: {item.DisplayName}"));
                continue;
            }

            proposedItems.Add(item);
        }

        var capacity = GetDirectoryCapacity(layout, fileSystem);
        if (proposedItems.Count >= capacity)
        {
            messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Error, 0, $"Directory layout exceeds capacity: {proposedItems.Count}/{capacity - 1}"));
        }

        var proposedLayout = messages.Any(message => message.Severity == DirectoryLayoutValidationSeverity.Error) ? null : Reindex(layout, proposedItems);
        return new DirectoryLayoutValidationResult(plan, messages, proposedLayout);
    }

    private DirectoryLayoutTextPlan ParsePlan(string text)
    {
        var entries = new List<DirectoryLayoutTextPlanEntry>();
        var lines = text.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var lineNumber = i + 1;
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("#"))
            {
                var label = line[1..].Trim();
                entries.Add(new DirectoryLayoutTextPlanEntry(lineNumber, true, string.Empty, label));
                continue;
            }

            var firstWhitespace = line.IndexOfAny([' ', '\t']);
            if (firstWhitespace < 0)
            {
                entries.Add(new DirectoryLayoutTextPlanEntry(lineNumber, false, line.Trim(), string.Empty));
                continue;
            }

            var stableId = line[..firstWhitespace].Trim();
            var displayName = line[(firstWhitespace + 1)..].Trim();
            entries.Add(new DirectoryLayoutTextPlanEntry(lineNumber, false, stableId, displayName));
        }

        return new DirectoryLayoutTextPlan(entries);
    }

    private static IDirectoryLayoutProvider GetProvider(IFileSystem fileSystem)
    {
        if (fileSystem is IDirectoryLayoutProvider provider)
        {
            return provider;
        }

        throw new InvalidOperationException("Directory layout is not supported for this file system.");
    }

    private static DirectoryLayoutItem FindByDisplayName(DirectoryEntryLayout layout, string displayName)
    {
        var item = layout.Items.FirstOrDefault(candidate => candidate.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
        if (item == null)
        {
            throw new InvalidOperationException($"Directory entry not found: {displayName}");
        }

        return item;
    }

    private static DirectoryEntryLayout Reindex(DirectoryEntryLayout layout, IReadOnlyList<DirectoryLayoutItem> items)
    {
        return layout with
        {
            Items = items.Select((item, index) => item with { Order = index }).ToArray()
        };
    }

    private static string GetSortKey(DirectoryLayoutItem item, DirectorySortBy sortBy)
    {
        return sortBy switch
        {
            DirectorySortBy.Extension => item.Entry?.Extension ?? item.VirtualLabel?.Extension ?? string.Empty,
            DirectorySortBy.Type => item.Entry?.FileSystemMetadata is HuBasicFileMetadata metadata ? metadata.FileType.ToString() : item.Kind.ToString(),
            _ => item.Entry?.FileName ?? item.VirtualLabel?.FileName ?? item.DisplayName
        };
    }

    private FileEntry? ApplyDisplayName(FileEntry entry, string displayName, DiskFileSystemInfo fsInfo, ICharacterEncoder encoder, int lineNumber, List<DirectoryLayoutValidationMessage> messages)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Error, lineNumber, "File name is required."));
            return null;
        }

        var parts = SplitDisplayName(NormalizeHostText(displayName), fsInfo);
        if (parts == null)
        {
            messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Error, lineNumber, $"Invalid file name: {displayName}"));
            return null;
        }

        if (!ValidateNameForFileSystem(parts.Value.Name, parts.Value.Extension, fsInfo, encoder, lineNumber, messages))
        {
            return null;
        }

        byte[]? rawName = entry.RawFileName;
        byte[]? rawExtension = entry.RawExtension;
        if (fsInfo.FileSystemName == "Hu-BASIC")
        {
            rawName = EncodePadded(encoder, parts.Value.Name, 13);
            rawExtension = EncodePadded(encoder, parts.Value.Extension, 3);
        }

        return entry with
        {
            FileName = parts.Value.Name,
            Extension = parts.Value.Extension,
            RawFileName = rawName,
            RawExtension = rawExtension
        };
    }

    private DirectoryLayoutItem CreateLabel(IFileSystem fileSystem, string text, int seed)
    {
        var fsInfo = fileSystem.GetFileSystemInfo();
        if (fsInfo.FileSystemName != "Hu-BASIC")
        {
            throw new InvalidOperationException("Layout labels are only supported for Hu-BASIC.");
        }

        var encoder = ResolveEncoder(fsInfo);
        var label = ParseLabelText(text, fsInfo, encoder);
        var id = $"label:{seed + 1}:{label.FileName}.{label.Extension}";
        return new DirectoryLayoutItem(id, seed, DirectoryLayoutItemKind.VirtualLabel, BuildDisplayName(label.FileName, label.Extension), null, label);
    }

    private static (string Name, string Extension)? SplitDisplayName(string displayName, DiskFileSystemInfo fsInfo)
    {
        var trimmed = displayName.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (fsInfo.FileSystemName == "Hu-BASIC")
        {
            var lastDot = trimmed.LastIndexOf('.');
            if (lastDot <= 0 || lastDot == trimmed.Length - 1)
            {
                return (trimmed, string.Empty);
            }

            var extension = trimmed[(lastDot + 1)..];
            if (extension.Length > 3)
            {
                return (trimmed, string.Empty);
            }

            return (trimmed[..lastDot], extension);
        }

        var dot = trimmed.LastIndexOf('.');
        return dot > 0 ? (trimmed[..dot], trimmed[(dot + 1)..]) : (trimmed, string.Empty);
    }

    private static bool ValidateNameForFileSystem(string fileName, string extension, DiskFileSystemInfo fsInfo, ICharacterEncoder encoder, int lineNumber, List<DirectoryLayoutValidationMessage> messages)
    {
        if (fsInfo.FileSystemName == "Hu-BASIC")
        {
            if (encoder.EncodeText(fileName).Length > 13)
            {
                messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Error, lineNumber, $"File name exceeds 13 bytes: {fileName}"));
                return false;
            }

            if (encoder.EncodeText(extension).Length > 3)
            {
                messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Error, lineNumber, $"Extension exceeds 3 bytes: {extension}"));
                return false;
            }

            if (!IsRoundTripSafe(encoder, fileName) || !IsRoundTripSafe(encoder, extension))
            {
                messages.Add(new DirectoryLayoutValidationMessage(DirectoryLayoutValidationSeverity.Error, lineNumber, $"File name contains characters not representable on {fsInfo.PlatformId}: {BuildDisplayName(fileName, extension)}"));
                return false;
            }
        }

        return true;
    }

    private static string NormalizeHostText(string text) => text;
    private static byte[] EncodePadded(ICharacterEncoder encoder, string text, int width)
    {
        var encoded = encoder.EncodeText(text);
        var padded = Enumerable.Repeat((byte)0x20, width).ToArray();
        Array.Copy(encoded, padded, Math.Min(encoded.Length, padded.Length));
        return padded;
    }

    private static bool IsRoundTripSafe(ICharacterEncoder encoder, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        var roundTrip = encoder.DecodeText(encoder.EncodeText(text)).TrimEnd(' ');
        return string.Equals(roundTrip, text, StringComparison.Ordinal);
    }

    private ICharacterEncoder ResolveEncoder(DiskFileSystemInfo fsInfo)
    {
        return new CharacterEncodingResolver(_encoderRegistry).ResolveEncoder(fsInfo);
    }

    private static int GetDirectoryCapacity(DirectoryEntryLayout layout, IFileSystem fileSystem)
    {
        return fileSystem.GetFileSystemInfo().FileSystemName == "Hu-BASIC" ? 128 : layout.Items.Count + 1;
    }

    private static string BuildDisplayName(string fileName, string extension)
    {
        return string.IsNullOrEmpty(extension) ? fileName : $"{fileName}.{extension}";
    }

    private static string BuildLabelText(DirectoryLayoutItem item)
    {
        if (item.VirtualLabel == null)
        {
            return item.DisplayName;
        }

        return BuildDisplayName(item.VirtualLabel.FileName, item.VirtualLabel.Extension);
    }

    private static VirtualDirectoryLabelEntry ParseLabelText(string text, DiskFileSystemInfo fsInfo, ICharacterEncoder encoder)
    {
        var normalizedText = NormalizeHostText(text.Trim());
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            throw new InvalidOperationException("Label text is required.");
        }

        var firstDot = normalizedText.IndexOf('.');
        string fileName;
        string extension;
        if (firstDot < 0)
        {
            fileName = normalizedText;
            extension = string.Empty;
        }
        else
        {
            fileName = normalizedText[..firstDot];
            var remaining = normalizedText[(firstDot + 1)..];
            var secondDot = remaining.IndexOf('.');
            extension = secondDot >= 0 ? remaining[..secondDot] : remaining;
        }

        fileName = TrimToEncodedBytes(fileName.Trim(), encoder, 13);
        extension = TrimToEncodedBytes(extension.Trim(), encoder, 3);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException("Label name is required.");
        }

        if (!ValidateLabelPart(fileName, encoder) || (!string.IsNullOrEmpty(extension) && !ValidateLabelPart(extension, encoder)))
        {
            throw new InvalidOperationException($"Label text contains characters not representable on {fsInfo.PlatformId}: {normalizedText}");
        }

        return new VirtualDirectoryLabelEntry(fileName, extension, 0x44, 0x01, 0, 0xFFFF, 0xFFFE, 0xFFFF, 0x3FFF);
    }

    private static bool ValidateLabelPart(string text, ICharacterEncoder encoder)
    {
        return IsRoundTripSafe(encoder, text);
    }

    private static string TrimToEncodedBytes(string text, ICharacterEncoder encoder, int maxBytes)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var value = text;
        while (encoder.EncodeText(value).Length > maxBytes && value.Length > 0)
        {
            value = value[..^1];
        }

        return value;
    }

    public static string CreateStableId(string rawId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawId));
        return Convert.ToHexString(hash[..4]);
    }
}