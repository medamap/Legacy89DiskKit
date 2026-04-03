using System.Text;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;

using DomainAttr = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Application.FileSystem;

public class FileTransferService
{
    private readonly ICharacterEncoder _encoder;

    public FileTransferService(ICharacterEncoder encoder)
    {
        _encoder = encoder;
    }

    public void ImportFile(IFileSystem fs, string hostPath, string diskFileName, bool isAscii = true, TextTransferOptions? textOptions = null)
    {
        byte[] diskData;
        textOptions ??= new TextTransferOptions();

        if (isAscii)
        {
            string text = File.ReadAllText(hostPath);
            text = TransformTabs(text, textOptions.TabMode, textOptions.TabWidth);
            diskData = _encoder.EncodeText(text);
            diskData = ConstrainTextPayloadForWrite(fs, diskData, textOptions.TruncateOnOverflow);
            
            // Ensure 0x1A terminator for Hu-BASIC text files
            if (diskData.Length == 0 || diskData[^1] != 0x1A)
            {
                var newData = new byte[diskData.Length + 1];
                Array.Copy(diskData, newData, diskData.Length);
                newData[^1] = 0x1A;
                diskData = newData;
            }
        }
        else
        {
            diskData = File.ReadAllBytes(hostPath);
        }

        var attributes = fs.CreateDefaultAttributes(isAscii);
        fs.WriteFile(diskFileName, diskData, attributes);
    }

    public void ExportFile(IFileSystem fs, string diskFileName, string hostPath, TextTransferOptions? textOptions = null)
    {
        byte[] diskData = fs.ReadFile(diskFileName);
        textOptions ??= new TextTransferOptions();
        
        // Find the file entry to check attributes
        var entry = fs.GetFiles().FirstOrDefault(f => f.FullName.Equals(diskFileName, StringComparison.OrdinalIgnoreCase));
        if (entry == null) throw new FileNotFoundException($"File not found on disk: {diskFileName}");

        if (ShouldTreatAsPlainText(entry))
        {
            string newline = textOptions.NewlineOverride ?? Environment.NewLine;
            string text = _encoder.DecodeText(diskData, newline);
            text = TransformTabs(text, textOptions.TabMode, textOptions.TabWidth);
            var utf8WithoutBom = new UTF8Encoding(false);
            File.WriteAllText(hostPath, text, utf8WithoutBom);
        }
        else
        {
            File.WriteAllBytes(hostPath, diskData);
        }
    }

    private static bool ShouldTreatAsPlainText(FileEntry entry)
    {
        return entry.FileSystemMetadata switch
        {
            HuBasicFileMetadata hu => hu.FileType == HuBasicFileType.Ascii,
            XDosFileMetadata xd => xd.FileType == XDosFileType.Asc,
            _ => entry.Attributes.IsAscii
        };
    }

    private static byte[] ConstrainTextPayloadForWrite(IFileSystem fs, byte[] payload, bool truncateOnOverflow)
    {
        const int huBasicMaxTextPayload = 0xFFFF - 1;
        if (fs.GetFileSystemInfo().FileSystemName != "Hu-BASIC" || payload.Length <= huBasicMaxTextPayload)
        {
            return payload;
        }

        if (!truncateOnOverflow)
        {
            throw new InvalidOperationException("Text payload exceeds the Hu-BASIC 65535-byte limit after tab conversion.");
        }

        return payload.Take(huBasicMaxTextPayload).ToArray();
    }

    private static string TransformTabs(string text, string tabMode, int tabWidth)
    {
        var normalizedMode = tabMode.Trim().ToLowerInvariant();
        if (normalizedMode == "keep")
        {
            return text;
        }

        if (tabWidth <= 0)
        {
            throw new InvalidOperationException("Tab width must be greater than zero.");
        }

        return normalizedMode switch
        {
            "spaces" => ExpandTabs(text, tabWidth),
            "remove" => text.Replace("\t", string.Empty, StringComparison.Ordinal),
            _ => throw new InvalidOperationException($"Unsupported tab mode: {tabMode}")
        };
    }

    private static string ExpandTabs(string text, int tabWidth)
    {
        var sb = new StringBuilder(text.Length);
        var column = 0;
        foreach (var ch in text)
        {
            switch (ch)
            {
                case '\t':
                    var spaces = tabWidth - (column % tabWidth);
                    if (spaces == 0)
                    {
                        spaces = tabWidth;
                    }

                    sb.Append(' ', spaces);
                    column += spaces;
                    break;
                case '\r':
                    sb.Append(ch);
                    column = 0;
                    break;
                case '\n':
                    sb.Append(ch);
                    column = 0;
                    break;
                default:
                    sb.Append(ch);
                    column++;
                    break;
            }
        }

        return sb.ToString();
    }
}
